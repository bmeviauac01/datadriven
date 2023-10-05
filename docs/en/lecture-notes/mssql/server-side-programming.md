# Microsoft SQL Server programming

The language of the Microsoft SQL Server platform is **T-SQL**. The T-SQL language is platform-specific, meaning the language can only be used in MSSQL server - although other platforms have similar languages. The T-SQL language and the **database server-side programming tools** it supports extend the originally declarative SQL language with imperative tools such as variables, branches, procedures, and additional tools such as triggers and cursors.

## Server-side programming

!!! abstract "Server-side programming"
    By server-side or database server-side programming, we mean that we execute not only commands to query and modify data in the database, but also carry out business logic inside the database.

To understand _when_ it is worthwhile to use server-side programming tools, it is first important to understand _why_ we would consider writing business logic in the database at all.

!!! question "Why would we want to implement business logic tasks in the database?"
    In a layered architecture, the lower layer provides services to the layer above it. So the upper layer "can't get around" the layer below; the operations have to go through the lower layer. But when you consider C#/Java/C++/ etc. code, we may not be able to guarantee such rules in the codebase. If we implement a complex set of rules and logic in a C# class, for example, it is difficult to guarantee that this class cannot be "bypassed."

    However, if the logic is in the database, it cannot be bypassed or circumvented. This will also be due to the fact that server-side programming gives us tools that ensure the execution of certain logic under all circumstances (see triggers later).

There are advantages and disadvantages to server-side programming. When considering the implementation of a functionality, in addition to knowing the layered architecture, we also need to look at what the technologies allow and which of the possible alternatives has the most benefits.

If we implement business functionality in the database, we ensure the following **benefits**.

- The responsibility of the database for managing consistency becomes even more evident. The relational model places great emphasis on consistency, but not all business consistency rules can be described directly in the relational model. Just think of the example of the Neptune system, where courses have an enrollment limit. This is a business rule, and if we break it, our data is inconsistent in the business sense. If the database is responsible for complying with this rule, we can ensure that the data is always consistent.

- We can reduce data traffic going out of the database. We often query data to display it to the user, which we cannot reduce. But if we query data only to make a decision based on it in the business logic layer, it is possible to avoid transferring the data between the database and the business logic if we bring the logic into the database instead. This is also more secure because no data is sent over the network unnecessarily (where sensitive data may be intercepted or outputted into error messages and log files by accident).

- The logic written in the database server can also be thought of as an interface that hides the details of data access and modification from the user (here: data access layer or business logic layer). On the one hand, this provides us with a level of abstraction, and on the other hand, it can aid parallel, faster development. While one development team builds the logic in the database, another team can write the application on top of it because the interface is defined earlier. Fixing errors is also more straightforward when the error is in the database. In this case, it is enough to fix the code in the database. Any system built on top of it will work correctly right away (unlike fixing a bug in Java code, because then a new version of the Java application has to be released and installed too).

Of course, there are **disadvantages** to server-side programming.

- The language we use is platform-dependent. We cannot transfer solutions from one database system to another. Moreover, programming knowledge itself is not easily transferable. A C++ programmer can code in C# more quickly than if he did not have such knowledge. But this is not true for server-side programming. One platform does not support the same tools as the other. The syntax of the languages also differs significantly. Database server-side programming requires an entirely new approach and different techniques.

- The load of the database server is increased. If a server performs more tasks, it will require more resources. Databases are critical points of data-driven systems, primarily since classical relational databases do not support horizontal scaling too well (load balancing between multiple servers). If the database server is responsible for more tasks, it can quickly become the bottleneck.

- These techniques are no longer evolving. We might even call them outdated used only in legacy applications. This server-side world is less common nowadays in software development projects.

## Basics of the T-SQL language

The T-SQL language is the language of Microsoft SQL Server, which, in addition to the standard SQL statements, allows you to:

- use variables,
- write branches and cycles,
- create stored procedures ("methods"),
- use cursors (iterators),
- define triggers (event-handling procedures),
- and much more.

Let’s look at the syntax of the language through examples. See the [official documentation](https://docs.microsoft.com/en-us/sql/t-sql/language-reference) for the detailed syntax.

!!! note ""
    The following examples can be executed on the [sample database](../../db/index.md).

### Variables

Variables must be declared before use. By convention, variable names begin with `@`. Uninitialized variables are all `NULL`.

```sql
DECLARE @num int

SELECT @num
-- NULL
```

Value assignment is possible with the `SET` statement or directly in the declaration:

```sql
DECLARE @num int = 5

SELECT @num
-- 5

SET @num = 3

SELECT @num
-- 3
```

The scope of the variable is not bound to the instruction block (between `BEGIN-END`). The variable is available within the so-called _batch_ or stored procedure:

```sql
BEGIN
  DECLARE @num int
  SET @num = 3
END

SELECT @num
-- This works, the variable is also available outside the instruction block.
-- 3

GO -- starts a new batch

SELECT @num
-- Error: Must declare the scalar variable "@num".
```

You can also assign value to a variable via a query:

```sql
DECLARE @name nvarchar (max)

SELECT @name = Name
FROM Customer
WHERE ID = 1
```

If the query returns more than one row, the _last_ value remains in the variable:

```sql
DECLARE @name nvarchar (max)

SELECT @name = Name
FROM Customer
-- there are multiple  matching rows
-- the last result of SELECT is stored in the variable
```

If the query does not yield any result, the value of the variable does not change:

```sql
DECLARE @name nvarchar (max)
SET @name = 'aaa'

SELECT @name = Name
FROM Customer
WHERE ID = 99999999
-- no matching row

SELECT @name
-- aaa
```

### Instruction blocks and control structures

An instruction block is written between `BEGIN-END` commands:

```sql
BEGIN
  DECLARE @num int
  SET @num = 3
END
```

Branching is possible by using the `IF-ELSE` structure:

```sql
DECLARE @name nvarchar (max)

SELECT @name = Name
FROM Customer
WHERE ID = 123

IF @name IS NOT NULL -- If the user exists
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

We use the `WHILE` condition and a `BEGIN-END` statement block for looping:

```sql
-- Generate at least 1000 products (e.g., for testing)
WHILE (SELECT COUNT (*) FROM Product) <1000
BEGIN
    INSERT INTO Product (Name, Price, Stock, VATID, CategoryID)
    VALUES ('Abc', 1, 1, 3, 13)
END
```

### Built-in functions

There are numerous [built-in functions](https://docs.microsoft.com/en-us/sql/t-sql/functions/functions) are available in T-SQL. Below are a few examples.

!!! note ""
    In the following examples, the results of the functions are queried with `select`. This is for the sole purpose of seeing the result. A function can be used anywhere in the language where a scalar value can be used.

String functions:

```sql
-- Concatenation
SELECT CONCAT('Happy ', 'Birthday!')
-- Happy Birthday!

-- N characters from the left
SELECT LEFT('ABCDEF', 2)
-- AB

-- Text length
SELECT LEN('ABCDEF')
-- 6

-- Substring replacement
SELECT REPLACE('Happy Birthday!', 'day', 'month')
-- Happy Birthmonth!

-- Lowercase conversion
SELECT LOWER('ABCDEF')
-- abcdef
```

Manage dates:

```sql
-- Current date and time
SELECT GETDATE()
-- 2021-09-28 10: 43: 59.120

-- Date's year component
SELECT YEAR(GETDATE ())
-- 2021

-- Specific component of the date
SELECT DATEPART(day, '12 / 20/2021 ')
SELECT DATEPART(month, '12 / 20/2021 ')
-- 20
-- 12

-- Difference between dates measured in a given unit (here: day)
SELECT DATEDIFF(day, '2021-09-28 12:10:09', '2021-11-04 13:45:09')
-- 37
```

Data type conversion:

```sql
SELECT CAST('12' as int)
-- 12

SELECT CONVERT(int, '12')
-- 12

SELECT CONVERT(int, 'aa')
-- Error: Conversion failed when converting the varchar value 'aa' to data type int.

SELECT TRY_CONVERT(int, 'aa')
-- NULL
```

`ISNULL`: result is the first argument if it is not null, otherwise the second argument (which can be null).

```sql
DECLARE @a int
DECLARE @b int = 5
SELECT ISNULL(@a, @b)
-- 5
```

!!! important ""
    Not to be confused with the `is null` condition, e.g., `UPDATE Product SET Price = 111 WHERE Price is null`

## Cursors

A cursor is an iterator used to scroll through a set of records item by item. We use it when a query returns multiple items, and we want to process them individually.

Using a cursor consists of the following steps:

1. The cursor must be declared and then opened.
1. The iteration takes place in a cycle.
1. The cursor is closed and released.

### Declaration and opening

A cursor is created with the `DECLARE` statement. We also provide the query yielding the results in the declaration. The full syntax is:

```sql
DECLARE cursor name CURSOR
  [FORWARD_ONLY | SCROLL]
  [STATIC | KEYSET DYNAMIC FAST_FORWARD]
  [READ_ONLY | SCROLL_LOCKS | OPTIMISTIC]
FOR query
[FOR UPDATE [OF column name [, ... n]]]
```

The meaning of optional flags in the declaration are (for more details, see the [documentation](https://docs.microsoft.com/en-us/sql/t-sql/language-elements/declare-cursor-transact-sql)):

- `FORWARD_ONLY`: only `FETCH NEXT` is possible
- `SCROLL`: you are free to move forward and backward in the cursor
- `STATIC`: works from a copy: the results are snapshotted when opening the cursor
- `KEYSET`: the database state at opening the cursor yields the row ids and their order, but the contents of the records are queried when fetching them
- `DYNAMIC`: each fetch gets up to date data; allows access to changes of competing transactions
- `READ_ONLY`: the contents of the cursor cannot be updated
- `SCROLL_LOCKS`: fetching locks the rows, thus guaranteeing that any subsequent `update` or `delete` statement is successful
- `OPTIMISTIC`: does not lock, uses optimistic concurrency management (to check for any changes between the time of `FETCH` and subsequent `update`)
- `FOR UPDATE`: list of columns that can be updated

The declaration is not enough to use use the cursor; it must be opened with the `OPEN` command. The pair of `OPEN` is the `CLOSE` command ending the use of the cursor. After closing, the cursor can be reopened, so we need to indicate when we no longer use the cursor; this is the `DEALLOCATE` command. (Typically, `CLOSE` and `DEALLOCATE` follow each other because we only use the cursor once.)

### Advancing the cursor

The current element of the cursor is accessed by "copying" the values ​​into local variable(s) using the `FETCH` command. The variables used here must be declared in advance. The `FETCH` statement typically get the following element (`FETCH NEXT`), but if the cursor is not `FORWARD_ONLY`, you can move back and forward too:

```sql
FETCH
  [NEXT | PRIORITY FIRST | LAST
      | ABSOLUTE {n | @nvar}
      | RELATIVE {n | @nvar}
  ]
FROM cursor_name
INTO @variable_name [, ... n]
```

We can determine whether the `FETCH` statement was successful by querying the implicit variable `@@FETCH_STATUS`. The value of the variable `@@FETCH_STATUS` is:

- 0 for a successful FETCH,
- -1 for a failed FETCH,
- -2 if the requested row is missing (when using `KEYSET`).

The complete iteration thus requires **two** `FETCH` statements and one `WHILE` loop:

```sql
-- declare, open ...
FETCH NEXT FROM cur INTO @var1, @var2
WHILE @@FETCH_STATUS = 0
BEGIN
  -- ... custom logic
  FETCH NEXT FROM cur INTO @var1, @var2
END
```

Note that the `FETCH` statement appears twice here. This is because the first one outside of the loop is used to query the very first record, and the second one inside the loop retrieves each additional record one at a time.

### Example

Let us see a complete example. Let us query products that have few items in stock left, and if the last sale was more than a year ago, discount the product price:

```sql
-- Extract the data from the cursor into these variables
DECLARE @ProductName nvarchar(max)
DECLARE @ProductID int
DECLARE @LastOrder datetime

DECLARE products_cur CURSOR SCROLL SCROLL_LOCKS -- Lock for guaranteed update
FOR
  SELECT Id, Name FROM Product WHERE Stock < 3 -- Cursor query
FOR UPDATE OF Price -- We also want to update the records

-- Typical opening, fetch, loop
OPEN products_cur
FETCH FROM products_cur INTO @ProductID, @ProductName
WHILE @@FETCH_STATUS = 0
BEGIN

  -- We can perform any operation in the cycle
  -- Find the time of the last purchase
  SELECT @LastOrder = MAX([Order].Date)
    FROM [Order] JOIN OrderItem ON [Order].Id = OrderItem.OrderId
    WHERE OrderItem.ProductID = @ProductId
  
  -- Diagnostic display
  PRINT CONCAT('ProductID:', convert(nvarchar, @ProductID), 'Last order:', ISNULL(convert(nvarchar, @LastOrder), 'No last order'))

  IF @LastOrder IS NULL OR @LastOrder < DATEADD(year, -1, GETDATE())
  BEGIN
    UPDATE Product
      SET Price = Price * 0.75
      WHERE CURRENT OF products_cur
      -- Update current cursor record
      -- Alternative: WHERE Id = @ProductID
  END

  -- Query next record and then go to the WHILE loop to verify if it was successful
  FETCH FROM products_cur INTO @ProductID, @ProductName
END
-- Stop using the cursor
CLOSE products_cur
DEALLOCATE products_cur
```

## Stored procedures and functions

The codes written in the previous examples were sent to the server and executed immediately. We can also write code that is stored by the server and can be called at any later time. In a modular programming environment, we usually call these functions, and in an object-oriented world, we call them methods. In Microsoft SQL Server, these are called stored procedures and stored functions. _Stored_ in the name indicates that the procedure code is stored in the database along with the data (and will be included in backups, for example).

The difference between a _procedure_ and a _function_ is that procedures typically have no return value, while functions do. An additional restriction in the MSSQL platform is that functions can only read the database but not make changes.

### Procedures

You can create a stored procedure with the following syntax:

```sql
CREATE [OR ALTER] PROC[EDURE] procedure_name
  [{@ parameter data_type}] [, ... n]
AS
[BEGIN]
  sql_instructions [... n]
[END]
```

The result of the `CREATE OR ALTER` statement is the creation of the stored procedure, if it does not exist, or else its update with the new contents. Prior to MSSQL Server 2016, there was no `CREATE OR ALTER`, only `CREATE PROC` and `ALTER PROC`. We can delete a stored procedure with the `DROP PROCECURE` statement, which removes the procedure from the server.

For example, Let us create a new tax percentage record in the `VAT` table, guaranteeing that only unique percentages can be added:

```sql
create or alter procedure InsertNewVAT -- create a stored procedure
    @Percentage int -- stored procedure parameters
as
  begin
  -- this is where the code begins, which the system executes when the procedure is called
  begin tran -- to avoid non-repeatable reading
  set transaction isolation level repeatable read

  declare @Count int

  select @Count = count(*)
  from VAT
  where Percentage = @Percentage

  if @Count = 0
      insert into VAT values ​​(@Percentage)
  else
      print 'error';

commit
end
```

The stored procedure is created by executing the former command, and then it can be called as follows:

```sql
exec InsertNewVAT 27
```

Stored procedures are part of our database. For example, in Microsoft SQL Server Management Studio, it is visible here:

![Stored procedure in database](../lecture-notes/mssql/img//mssql-stored-proc-in-db.png)

### Scalar functions

The declaration of a function is similar to a procedure, but we must also specify the return type:

```sql
CREATE [OR ALTER] FUNCTION name
([{@ parameter data_type}] [, ... n])
RETURNS data type
[ AS ]
BEGIN
  instructions
  RETURN scalar_value
END
```

Let us see a function with return value `int` that has no input parameters:

```sql
CREATE OR ALTER FUNCTION LargestVATPercentage()
RETURNS int
BEGIN
RETURN (SELECT MAX(Percentage) FROM VAT)
END
```

Here's how to use this function:

```sql
select dbo.LargestVATPercentage()
-- The dbo prefix is ​​the name of the schema, indicating that this is not a built-in function
-- Without this, the function is not found

-- or for example
DECLARE @maxvat int = dbo.LargestVATPercentage()
select @maxvat
```

### Table functions

A function can also yield a table as the result. In this case, the declaration looks like this:

```sql
CREATE [OR ALTER] FUNCTION name
([{@ parameter data type}] [, ... n])
RETURNS TABLE
[ AS ]
RETURN select statement
```

For example, consider retrieving VAT rates above a certain percentage:

```sql
CREATE FUNCTION VATPercentages(@min int)
RETURNS TABLE
AS RETURN
(
    SELECT ID, Percentage FROM VAT
    WHERE Percentage > @min
)
```

This function returns a table, so you can use the function anywhere a table can appear, for example:

```sql
SELECT * FROM VATPercentages(20)
```

Since the function returns a table, we can even `join` it:

```sql
SELECT VAT.Percentage, count(*)
FROM VAT JOIN VATPercentages(20) p on VAT.ID = p.Id
GROUP BY VAT.Percentage
```

## Error handling

In the stored procedure example, we wanted to prevent duplicate records from being inserted into a table. This was accomplished above by not executing the instruction. However, it would be more appropriate to report the error to the caller. This is what structured error handling is about. In case of an error, you can use the `throw` command to raise an error. This command interrupts code execution and returns control to the caller (where the error can be handled or passed on). The error has a number (between 50000 and 2147483647), a text, and an error status identifier between 0-255.

The updated procedure for recording the VAT key looks like this:

```sql hl_lines="18"
create or alter procedure InsertNewVAT
    @Percentage int
as
begin

  begin tran
  set transaction isolation level repeatable read

  declare @Count int

  select @Count = count(*)
  from VAT
  where Percentage = @Percentage

  if @Count = 0
      insert into VAT values ​​(@Percentage)
  else
      throw 51000, 'error', 1;

  commit
end
```

To handle (catch) an error, you can use the following syntax:

```sql
begin try
  exec InsertNewVAT 27
end try
begin catch
  -- access the error details with the following functions (similar to stack trace in other languages)
  SELECT
    ERROR_NUMBER () AS ErrorNumber,
    ERROR_SEVERITY () AS ErrorSeverity,
    ERROR_STATE () AS ErrorState,
    ERROR_PROCEDURE () AS ErrorProcedure,
    ERROR_LINE () AS ErrorLine,
    ERROR_MESSAGE () AS ErrorMessage;
end catch
```

Of course, it's not just user code that can throw errors. The system also signals errors identically, and we can handle them using the same tools.

## Triggers

The tools and language elements described so far have similar counterparts in other platforms. However, triggers are unique to databases. Triggers are event-handling stored procedures. We can subscribe to various events in the database, and when the event occurs, the system will execute our code defined in the trigger.

We will only discuss DML triggers. These are triggers that run due to data modification (`insert`, `update`, `delete`) operations. There are other triggers as well; e.g., you can create triggers for system events. Check the [official documentation](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-trigger-transact-sql) for more details.

### DML triggers

Using triggers, we can solve several tasks that would be difficult otherwise. Consider, for example, an **audit logging** requirement: when a change is made to a particular table, let us record a log entry. We could solve this task in C#/Java/Python by creating a class or methods for accessing the database table in question. However, nothing prevents the programmer from "bypassing" this logic and accessing the database directly. We cannot prevent this with triggers, but we can create a trigger that performs the required logging instead of the C#/Java/Python code.

Let us look at this example: logging the deletion of any products in a dedicated table:

```sql
-- Create the auditing table
create table AuditLog([Description] [nvarchar](max) NULL)
go

-- Logging trigger
create or alter trigger ProductDeleteLog
  on Product
  for delete
as
insert into AuditLog (Description)
select 'Product deleted: ' + convert(nvarchar, d.Name) from deleted d
```

Executing the commands above creates a trigger in the database (just as a stored procedure is created). This trigger is then executed automatically. So the trigger is not called by us but by the system. Nevertheless, we give the trigger a name to reference it (e.g., if we want to delete it with the `DROP TRIGGER` statement). The trigger is linked to the table in the database:

![Trigger in the database](../lecture-notes/mssql/img//mssql-trigger-in-db.png)

The syntax for defining a DML trigger is as follows:

```sql
CREATE TRIGGER trigger_name
ON { table | view }
 FOR {[DELETE] [,] [INSERT] [,] [UPDATE]}
AS
sql_instruction [... n]
```

Note that in the trigger definition, we specify the table or view. So a trigger listens for events of a single table. The events are set by listing the requested modifying operations (e.g., `for update, insert`). Note that three possible options cover all types of changes; also note, that there is no `select` event — since it is not a change.

The instructions defined in the trigger code are executed _after_ the specified events occur. This means that the changes are already performed (for example, new rows are already inserted into the table), but the transaction of the operation is not yet finished. Thus, we can make further changes as part of the same transaction (and consequently, seeing the result of the "original" command and the trigger as an atomic change) or even aborting the transaction. A particular use case for triggers is to check the consistency of data (that cannot be verified otherwise) and to abort the modification in the event of a violation. We will see an example of this soon.

Triggers are executed per _instruction_, which means they are called once per DML operation. In other words, the trigger does not handle the changes per row; instead, all changes caused by a single operation are handled at once. So, for example, if an `update` statement changes 15 rows, the trigger is called once, and we will see all 15 changes. Of course, this is also true for inserting and deleting - a deletion operation can delete multiple rows, and we can insert multiple records with a single insert command.

!!! warning "There is no row-level trigger"
    Other database platforms have row-level triggers, where the trigger is called individually for all the modified rows. Microsoft SQL Server platform does not have such a trigger!

How do we know what changes are handled in the trigger? Inside the trigger, we have access to two log tables through the implicit variables `inserted` and `deleted`. The structure of these tables is identical to the table on which the trigger is defined. These tables exist only during the trigger execution and can only be accessed from within the trigger. Their content depends on the type of operation that invoked the trigger:

|          | insert      | delete          | update                |
| -------- | ----------- | --------------- | --------------------- |
| inserted | new records | empty           | new values of records |
| deleted  | empty       | deleted records | old values ​​of records |

When inserting, the inserted records can be found in the database table (but there, we do not "see" that they have been newly inserted), and they are also available in the `inserted` table. In the case of deletion, `deleted` contains the rows already deleted from the table. Finally, in the case of `update`, we see the states before and after the change in the two log tables. We need to work with these log tables as tables; we should always expect to have more than one record in them.

!!! warning "The `inserted` and `deleted` are tables"
    The `inserted` and `deleted` tables can only be treated as tables! For example, it does not make sense to use `select @id=inserted.ID`; instead, we can use a cursor on these tables or `join` them.

We have already seen an example of audit logging implemented with a trigger. Let us look at other use-cases. Let us have a table with an email address column. When inserting and modifying, we need to check the email address value, and we must not accept text that does not look like an email address. Here **we validate a rule of consistency** with the trigger.

```sql
-- Create a function to check the email address
CREATE FUNCTION [IsEmailValid](@ email nvarchar(1000))
RETURNS bit -- true / false return value
AS
BEGIN
  IF @email is null RETURN 0 -- Cannot be null
  IF @email = '' RETURN 0 -- Cannot be an empty string
  IF @email LIKE '%_@%_._%' RETURN 1 -- Looks like an email
  RETURN 0
  -- The same in one line:
  -- RETURN CASE WHEN ISNULL(@email, '') <> '' AND @email LIKE '%_@%_._%' THEN 1 ELSE 0 END
END

-- The trigger
create or alter trigger CustomerEmailSyntaxCheck
  on Customer
  for insert, update -- Check both inserting and modifying
as
-- For both insertion and modification, the new data is in the inserted table
-- Is there an item there for which the new email address is not valid?
if exists(select 1 from inserted i where dbo.IsEmailValid(i.Email) = 0)
  throw 51234, 'invalid email address', 1 -- abort the transaction by raising the error
```

The above trigger runs after insertion or modification in the same transaction. So if we throw an error, the transaction will be aborted (unless handled by the caller). By running the trigger at the instruction level, a single faulty record interrupts the entire operation. Of course, this is what we expect due to atomicity: the indivisibility of the transaction is satisfied for the instruction as a whole, i.e., for inserting/modifying several records at once.

Another common use of triggers is **maintenance of denormalized data**. Although we try to avoid denormalization in a relational database, in practice, it may be necessary to store computed data for performance reasons. Let us look at an example of this as well. Suppose customers have two email addresses: one to sign in with, an optional second one to use for notifications. To avoid always having to query both email addresses and choosing between the two, let us make sure the _effective_ email address is available in the database "calculated" from the previous two:

```sql
-- Additional email address columns for customers
alter table Customer
add [NotificationEmail] nvarchar(max), [EffectiveEmail] nvarchar(max)
go

-- Trigger to update the effective email address
create or alter trigger CustomerEmailUpdate
  on Customer
  for insert, update
as
update Customer -- We modify the Customer table, not the inserted implicit table
set EffectiveEmail = ISNULL(i.NotificationEmail, i.Email) -- Copy one or the other value to the EffectiveEmail column
from Customer c join inserted i on c.ID = i.ID -- Records must be retrieved from the Customer table based on the inserted rows
```

!!! important "Trigger recursion"
    Note that in this trigger, an update is executed in response to an update event. This is a recursion. Recursion of DML triggers is disabled by default, so the above example does not invoke trigger recursion. However, if trigger recursion were enabled in the database, we would need to handle it.

Let us look at another example of denormalized data maintenance. In the order table, let us add a grand total column, which is the total net price of the order. We need a trigger to keep the value updated automatically:

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

In this trigger, it is worth noting that while the event occurs in the `OrderItem` table, the content to be updated is in the `Order` table. This is fine, a trigger can read and write any part of the database, and all changes are executed in the same transaction. Furthermore, we do not recalculate the total amount in the trigger but alter it in response to the changes. Although this makes the trigger code more complex, it is more effective this way.

!!! note "Sequence of triggers"
    We can define multiple triggers for an event. But the order of their execution cannot be specified. We can set the first and last triggers, but we cannot make assumptions regarding their sequence otherwise - it is considered ill-advised to design functionality where triggers need to build on each other.

### _Instead of_ triggers

A special type of trigger is the so-called _instead of trigger_. Such triggers can be defined for both tables and views. Let us look at using them on tables first. An _instead of_ trigger defined on a table, as its name suggests, runs the instruction we define in the trigger instead of the actual operation's `insert / update / delete`. E.g., when inserting, the new rows are not added to the table, and when deleting, rows are not deleted. Instead, we can define in the trigger how to perform these operations. In the overridden process, we can access the table itself and execute the necessary actions in this table. These operations do not cause recursion in the trigger. These triggers can be considered as _before_ triggers, i.e., we can perform checks before making the changes and abort the operation in case of an error.

A typical use case for an _instead of_ trigger is, for example, when we do not want to perform a deletion. This is also called _soft delete_:,instead of deleting, we only mark the records as deleted:

```sql
-- Soft delete flag column in the table with a default value of 0 (i.e., false)
alter table Product
add [IsDeleted] bit NOT NULL CONSTRAINT DF_Product_IsDeleted DEFAULT 0
go

-- Instead of trigger, the delete command does not perform the deletion
-- the following code runs instead
create or alter trigger ProductSoftDelete
  on Product
  instead of delete
as
update Product
  set IsDeleted = 1
  where ID in (select ID from deleted)
```

Another typical use case for _instead of_ triggers is views. A view is the result of a query, so inserting new data into the view does not make sense. However, you can use an _instead of_ trigger to define what to do instead of "inserting into view." Let us look at an example. In the view below, we combine data from the product and VAT tables so that the VAT percentage is displayed in the view instead of the ID of the referenced VAT record. We can insert into this view by inserting the data into the product table instead:

```sql
-- Define the view
create view ProductWithVatPercentage
as
select p.Id, p.Name, p.Price, p.Stock, v.Percentage
from Product p join Vat v is p.VATID = v.Id

-- Instead of trigger for the view
create or alter trigger ProductWithVatPercentageInsert
on ProductWithVatPercentage
instead of insert
as
  -- The insertion goes into the Product table: a new row is created for each inserted record
  -- And we find the VAT record corresponding to the provided percentage
  -- The solution is not complete because it does not handle if there is no matching VAT record
  insert into Product(Name, Price, Stock, VATID, CategoryID)
  select i.Name, i.Price, i.Stock, v.ID, 1
    from inserted i join VAT v on v.Percentage = i.Percentage

-- The trigger can be tested by inserting data into the view
insert into ProductWithVatPercentage(Name, Price, Stock, Percentage)
values ('Red ball', 1234, 22, 27)
```
