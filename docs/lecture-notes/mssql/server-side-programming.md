# Microsoft SQL Server programming

The code below can be run on the [sample database](../../db/index.md).

## Variables

Variable declaration:

```sql
DECLARE @num int

SELECT @num
-- NULL
```

Assigning a value with `SET` instruction or part of the declaration:

```sql
DECLARE @num int = 5

SELECT @num
-- 5

SET @num = 3

SELECT @num
-- 3
```

The variable is _not_ bound to the instruction block. The variable is accessible within the so-called _batch_ or stored procedure:

```sql
BEGIN
  DECLARE @num int
  SET @num = 3
END

SELECT @num
-- This works, the variable is accessible even outside the declaring instruction block.
-- 3

GO -- begins a new batch

SELECT @num
-- Error: Must declare the scalar variable "@num".
```

Assigning value through query result:

```sql
DECLARE @name nvarchar(max)

SELECT @name = Name
FROM Customer
WHERE ID = 1

SELECT @name
```

If the query yields multiple results, the _last_ value will be assigned:

```sql
DECLARE @name nvarchar(max)

SELECT @name = Name
FROM Customer
-- will match multiple rows

SELECT @name
-- last value of the SELECT will be assinged
```

Ig the query yields no results, the variable is not changed:

```sql
DECLARE @name nvarchar(max)
SET @name = 'aaa'

SELECT @name = Name
FROM Customer
WHERE ID = 99999999
-- no matches

SELECT @name
-- aaa
```

## Instruction blocks and control flow statements

Instruction block:

```sql
BEGIN
  DECLARE @num int
  SET @num = 3
END
```

Condition (if the customer exists we update the email address):

```sql
DECLARE @name nvarchar(max)

SELECT @name = Name
FROM Customer
WHERE ID = 123

IF @name IS NOT NULL
BEGIN
  PRINT 'Updating email'
  UPDATE Customer
  SET Email = 'agh*******@gmail.com'
  WHERE ID = 123
END
ELSE
BEGIN
  PRINT 'No such customer'
END
```

Cycle (generating new products, e.g. for testing):

```sql
WHILE (SELECT COUNT(*) FROM Product) < 1000
BEGIN
    INSERT INTO Product(Name,Price,Stock,VATID,CategoryID)
    VALUES ('Abc', 1, 1, 3, 13)
END
```

## Stored procedure

Inserting a new VAT percentage into table `VAT` if it does not exist:

```sql
create or alter procedure InsertNewVAT -- create procedure with this name
    @Percentage int                    -- parameters of the procedure
as

begin tran                            -- protection against nonrepeatable read
set transaction isolation level repeatable read

declare @Count int

select @Count = count(*)
from VAT
where Percentage = @Percentage

if @Count = 0
    insert into VAT values (@Percentage)
else
    print 'error';

commit
```

Invoking the stored procedure:

```sql
exec InsertNewVAT 27
```

Deleting the stored procedure:

```sql
drop procedure InsertNewVAT
```

Before SQL Server 2016 there was no `create or alter` only the two separately.

## Stored functions

Query VAT percentages over a threshold:

```sql
CREATE FUNCTION VATPercentages(@min int)
RETURNS TABLE
AS RETURN
(
    SELECT ID, Percentage FROM VAT
    WHERE Percentage > @min
);
```

Usage:

```sql
SELECT *
FROM VATPercentages(20)
```

## Error handling

Inserting a new VAT percentage into table `VAT`; if it already exist, raise and error:

```sql
create or alter procedure InsertNewVAT
    @Percentage int
as

begin tran
set transaction isolation level repeatable read

declare @Count int

select @Count = count(*)
from VAT
where Percentage = @Percentage

if @Count = 0
    insert into VAT values (@Percentage)
else
    throw 51000, 'error', 1;

commit
```

Error handling:

```sql
begin try
  exec InsertNewVAT 27
end try
begin catch
  SELECT
    ERROR_NUMBER() AS ErrorNumber,
    ERROR_SEVERITY() AS ErrorSeverity,
    ERROR_STATE() AS ErrorState,
    ERROR_PROCEDURE() AS ErrorProcedure,
    ERROR_LINE() AS ErrorLine,
    ERROR_MESSAGE() AS ErrorMessage;
end catch
```

## Trigger

Let us log the deletion of products into an audit log table:

```sql
-- Creating the audit log table
create table AuditLog([Description] [nvarchar](max) NULL)
go

-- Logging trigger
create or alter trigger ProductDeleteLog
  on Product
  for delete
as
insert into AuditLog(Description)
select 'Product deleted: ' + convert(nvarchar, d.Name) from deleted d
```

Let us suppose that customers have two email addresses: one for the login and another one for the newsletter, which is not necessarily specified. Let us always know the effective email address:

```sql
-- Add the new email addresses
alter table Customer
add [NotificationEmail] nvarchar(max), [EffectiveEmail] nvarchar(max)
go

-- The trigger to keep the effective address up to date
create or alter trigger CustomerEmailUpdate
  on Customer
  for insert, update
as
update Customer
set EffectiveEmail = ISNULL(i.NotificationEmail, i.Email)
from Customer c join inserted i on c.ID = i.ID
```

A total sum column on the order table should be updated to reflect the total of all items in the order:

```sql
create or alter trigger OrderTotalUpdateTrigger
  on OrderItem
  for insert, update, delete
as

update Order
set Total = isnull(Total,0) + TotalChange
from Order inner join
        (select i.OrderID, sum(Amount*Price) as TotalChange
        from inserted i
        group by i.OrderID) OrderChange
    on Order.ID = OrderChange.OrderID

update Order
set Total = isnull(Total,0) – TotalChange
from Order inner join
        (select d.OrderID, sum(Amount*Price) as TotalChange
        from deleted d
        group by d.OrderID) OrderChange
    on Order.ID = OrderChange.OrderID
```

Soft-delete, that is, _instead of_ actually deleting a product, mark it as deleted:

```sql
-- Soft delete flag column into the table with 0 (false) default value
alter table Product
add [IsDeleted] bit NOT NULL CONSTRAINT DF_Product_IsDeleted DEFAULT 0
go

-- Instead of trigger: when a delete command is issued, the records are not deleted
-- The code below is executed instead
create or alter trigger ProductSoftDelete
  on Product
  instead of delete
as
update Product
  set IsDeleted=1
  where ID in (select ID from deleted)
```

## Cursor

Let us find the products of which there are only a few in stock, and if the last time we sold one of them has been more than a year ago, then let us put the product on sale:

```sql
DECLARE @ProductName nvarchar(max)
DECLARE @ProductID int
DECLARE @LastOrder datetime

DECLARE products_cur CURSOR FAST_FORWARD READ_ONLY
FOR
  SELECT Id, Name FROM Product
  WHERE Stock < 3

OPEN products_cur
FETCH FROM products_cur INTO @ProductID, @ProductName
WHILE @@FETCH_STATUS = 0
BEGIN

  SELECT @LastOrder = MAX([Order].Date)
  FROM [Order] JOIN OrderItem ON [Order].Id = OrderItem.OrderId
  WHERE OrderItem.ProductID = @ProductId
  
  PRINT CONCAT('ProductID: ', convert(nvarchar, @ProductID), ' Last order: ', ISNULL(convert(nvarchar, @LastOrder), 'No last order'))

  IF @LastOrder IS NULL OR @LastOrder < DATEADD(year, -1, GETDATE())
  BEGIN
    UPDATE Product
    SET Price = Price * 0.75
    WHERE Id = @ProductID
  END

  FETCH FROM products_cur INTO @ProductID, @ProductName
END
CLOSE products_cur
DEALLOCATE products_cur
```
