# Microsoft SQL Server programozása

A példák a [minta adatbázison](../../db/index.md) futtathatóak.

## Változók

Változó deklaráció:

```sql
DECLARE @num int

SELECT @num
-- NULL
```

Értékadás a `SET` utasítással, vagy a deklarációban:

```sql
DECLARE @num int = 5

SELECT @num
-- 5

SET @num = 3

SELECT @num
-- 3
```

A változó _nem_ kötődik az utasítás blokkhoz. A változó az un. _batch_-en belül vagy tárolt eljáráson belül érvényes:

```sql
BEGIN
  DECLARE @num int
  SET @num = 3
END

SELECT @num
-- Ez működik, a változó az utasítás blokkon kívül is elérhető.
-- 3

GO -- új batch-et kezd

SELECT @num
-- Hiba: Must declare the scalar variable "@num".
```

Változó értékadása lekérdezéssel:

```sql
DECLARE @name nvarchar(max)

SELECT @name = Name
FROM Customer
WHERE ID = 1

SELECT @name
```

Ha a lekérdezés több sorral tér vissza, az _utolsó_ érték marad a változóban:

```sql
DECLARE @name nvarchar(max)

SELECT @name = Name
FROM Customer
-- több illeszkedő sor is lesz

SELECT @name
-- SELECT utolsó eredménye kerül a változóba
```

Ha a lekérdezés nem tér vissza eredménnyel, a változó értéke nem változik:

```sql
DECLARE @name nvarchar(max)
SET @name = 'aaa'

SELECT @name = Name
FROM Customer
WHERE ID = 99999999
-- nincs illeszkedő sor

SELECT @name
-- aaa
```

## Utasítás blokkok és vezérlési szerkezetek

Utasítás blokk:

```sql
BEGIN
  DECLARE @num int
  SET @num = 3
END
```

Elágazás (ha létezik a felhasználó, frissítsük az email címét):

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

Ciklus (generáljunk legalább 1000 terméket - pl. teszteléshez):

```sql
WHILE (SELECT COUNT(*) FROM Product) < 1000
BEGIN
    INSERT INTO Product(Name,Price,Stock,VATID,CategoryID)
    VALUES ('Abc', 1, 1, 3, 13)
END
```

## Tárolt eljárás

ÁFA kulcs rögzítése a `VAT` táblába, olyan kulcs nem rögzíthető mely már létezik:

```sql
create or alter procedure InsertNewVAT -- tárolt eljárás létrehozása, neve
    @Percentage int                    -- tárolt eljárás paraméterei
as

begin tran                            -- nem megismételhető olvasás elkerülése végett
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

Tárolt eljárás meghívása:

```sql
exec InsertNewVAT 27
```

Tárolt eljárás törlése:

```sql
drop procedure InsertNewVAT
```

SQL Server 2016 előtt nem volt `create or alter`, csak külön-külön.

## Tárolt függvény

Áfakulcsok lekérése egy adott százalék felett:

```sql
CREATE FUNCTION VATPercentages(@min int)
RETURNS TABLE
AS RETURN
(
    SELECT ID, Percentage FROM VAT
    WHERE Percentage > @min
);
```

Használata:

```sql
SELECT *
FROM VATPercentages(20)
```

## Hibakezelés

ÁFA kulcs rögzítése az ÁFA táblába, ha már létezik az ÁFA kulcs, dobjon hibát:

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

Hiba lekezelése (elkapása):

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

Naplózzuk a termékek törlését egy napló táblába:

```sql
-- Napló ábla létrehozása
create table AuditLog([Description] [nvarchar](max) NULL)
go

-- Naplózó trigger
create or alter trigger ProductDeleteLog
  on Product
  for delete
as
insert into AuditLog(Description)
select 'Product deleted: ' + convert(nvarchar, d.Name) from deleted d
```

Tegyük fel, hogy a vevőknek két email címe is van: egy a bejelentkezéshez, és megadhatnak egy másikat, amit az értesítésekhez használni akarnak. Hogy ne kelljen mindig mindkét email címet lekérdezni, és választani a kettő közül, legyen elérhető adatbázisban a valóban használt email cím:

```sql
-- Plusz email cím oszlopok a vevőknek
alter table Customer
add [NotificationEmail] nvarchar(max), [EffectiveEmail] nvarchar(max)
go

-- Használt email címet frissítő trigger
create or alter trigger CustomerEmailUpdate
  on Customer
  for insert, update
as
update Customer
set EffectiveEmail = ISNULL(i.NotificationEmail, i.Email)
from Customer c join inserted i on c.ID = i.ID
```

A megrendelés táblába felvett végösszeg oszlopot (amely a megrendelés teljes nettó ára) automatikusan tartsuk karban:

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

## Kurzor

Keressük meg azon termékeket, amiből alig van raktáron, és ha a legutolsó eladás több, mint egy éve volt, akkor adjunk kedvezményt a termékre:

```sql
DECLARE @ProductName nvarchar(max)
DECLARE @ProductID int
DECLARE @LastOrder datetime
DECLARE @Price float

DECLARE products_cur CURSOR FAST_FORWARD READ_ONLY
FOR
  SELECT Id, Name, Price FROM Product
  WHERE Stock < 3

OPEN products_cur
FETCH FROM products_cur INTO @ProductID, @ProductName, @Price
WHILE @@FETCH_STATUS = 0
BEGIN

  SELECT @LastOrder = MAX(Order.Date)
  FROM Order JOIN OrderItem ON Order.Id = OrderItem.OrderId
  WHERE OrderItem.ProductID = @ProductId

  PRINT 'Product ' + convert(nvarchar, @ProductID) + ' last order '
    + convert(nvarchar, @LastOrder)

  IF @LastOrder IS NULL OR @LastOrder < DATEADD(year, -1, GETDATE())
  BEGIN
    UPDATE Product
    SET Price = @Price * 0.75
    WHERE Id = @Product
  END

  FETCH FROM products_cur INTO @ProductID, @ProductName, @Price
END
CLOSE products_cur
DEALLOCATE products_cur
```
