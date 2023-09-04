# Microsoft SQL Server programming

The seminar's goal is to get to know the server-side programming capabilities of the Microsoft SQL Server platform.

## Pre-requisites

Required tools to complete the tasks:

- Microsoft SQL Server (LocalDB or Express edition)
- SQL Server Management Studio
- Database initialization script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/datadriven/master/overrides/db/mssql.sql)

Recommended to review:

- SQL language
- Microsoft SQL Server programming (stored procedures, triggers)
- [Microsoft SQL Server usage guide](../../db/mssql.md)

## How to work during the seminar

The first four exercises are solved together with the instructor. The final exercise is individual work if time permits.

!!! info ""
    This guide summarizes and explains the behavior. Before looking at these provided answers, we should think first!

## Exercise 0: Create/check the database

The database resides on each machine; thus, the database you created previously might not be available. First, check if your database exists, and if it does not, create and initialize it. (See the instructions [in the first seminar material](../transactions/index.md).)

## Exercise 1: SQL commands (review)

Write SQL commands/queries for the following exercises.

1. How many uncompleted orders are there (look for a status other than "Delivered")?

    ??? example "Solution"
        ```sql
        select count(*)
        from [Order] o join Status s on o.StatusID = s.ID
        where s.Name != 'Delivered'
        ```

        We see a `join` and an aggregation here. (There are other syntaxes for joining tables; refer to the lecture notes.)

1. Which payment methods have _not_ been used at all?

    ??? example "Solution"
        ```sql
        select p.Method
        from [Order] o right outer join PaymentMethod p on o.PaymentMethodID = p.ID
        where o.ID is null
        ```

        The key in the solution is the `outer join`, through which we can see the payment method records that have _no_ orders.

1. Let us insert a new customer and query the auto-assigned primary key!

    ??? example "Solution"
        ```sql
        insert into Customer(Name, Login, Password, Email)
        values ('Test Test', 'tt', '********', 'tt@email.com')

        select @@IDENTITY
        ```

        It is recommended (though not required) to name the columns after `insert` to be unambiguous. No value was assigned to the ID column, as the definition of that column mandates that the database automatically assigns a new value upon insert. We can query this ID after the insert is completed.

1. One of the categories has the wrong name. Let us change _Tricycle_ to _Tricycles_!

    ??? example "Solution"
        ```sql
        update Category
        set Name = 'Tricycles'
        where Name = 'Tricycle'
        ```

1. Which category contains the largest number of products?

    ??? example "Solution"
        ```sql
        select top 1 Name, (select count(*) from Product where Product.CategoryID = c.ID) as cnt
        from Category c
        order by cnt desc
        ```

        There are many ways this query can be formulated. This is only one possible solution. It also serves as an example of the usage of subqueries.

## Exercise 2: Inserting a new product category

Create a new stored procedure that helps inserting a new product category. The procedure's inputs are the name of the new category, and optionally the name of the parent category. Raise an error if the category already exists, or the parent category does not exist. Let the database generate the primary key for the insertion.

??? example "Solution"
    **Stored procedure**

    ```sql
    create or alter procedure AddNewCategory
        @Name nvarchar(50),
        @ParentName nvarchar(50)
    as

    begin tran

    -- Is there a category with identical name?
    declare @ID int
    select @ID = ID
    from Category with (TABLOCKX)
    where upper(Name) = upper(@Name)

    if @ID is not null
    begin
        rollback
        raiserror ('Category %s already exists',16,1,@Name)
        return
    end

    declare @ParentID int
    if @ParentName is not null
    begin
        select @ParentID = ID
        from Category
        where upper(Name) = upper(@ParentName)

        if @ParentID is null
        begin
            rollback
            raiserror ('Category %s does not exist',16,1,@ParentName)
            return
        end
    end

    insert into Category
    values(@Name,@ParentID)

    commit
    ```

    **Testing**

    Let us open a new _Query_ window and execute the following testing instructions.

    `exec AddNewCategory 'Beach balls', NULL`

    This shall succeed. Let us verify the table contents afterward.

    Let us repeat the same command; it shall fail now.

    We can also try with a parent category.

    `exec AddNewCategory 'LEGO Star Wars', 'LEGO'`

## Exercise 3: Maintenance of order status

Create a trigger that updates the status of each item of an order when the status of the order changes. Do this only for those items of the order that have the same status the order had before the change. Other items in the order should not be affected.

??? example "Solution"
    **Trigger**

    ```sql
    create or alter trigger UpdateOrderStatus
    on [Order]
    for update
    as

    update OrderItem
    set StatusID = i.StatusID
    from OrderItem oi
    inner join inserted i on i.Id=oi.OrderID
    inner join deleted d on d.ID=oi.OrderID
    where i.StatusID != d.StatusID
    and oi.StatusID=d.StatusID
    ```

    Let us make sure we understand the `update ... from` syntax. The behavior is as follows. We use this command when some of the changes we want to make during the update require data from another table. The syntax is based on the usual `update ... set...` format extended with a `from` part, which follows the same syntax as a `select from`, including the `join` to gather information from other tables. This allows us to use the joined records and their content in the `set` statement (that is, a value from a joined record can be on the right side of an assignment).

    **Testing**

    Let us check the status of the order and each item in the order:

    ```sql
    select OrderItem.StatusID, [Order].StatusID
    from OrderItem join [Order] on OrderItem.OrderID=[Order].ID
    where OrderID = 1
    ```

    Let us change the status of the order:

    ```sql
    update [Order]
    set StatusID=4
    where ID=1
    ```

    Check the status now (the update should have updated all stauses):

    ```sql
    select OrderItem.StatusID, [Order].StatusID
    from OrderItem join [Order] on OrderItem.OrderID=[Order].ID
    where OrderID = 1
    ```

## Exercise 4: Summing the total purchases of a customer

Let us calculate and store the value of all purchases made by a customer!

1. Add a new column to the table: `alter table Customer add Total float`
1. Calculate the current totals. Let us use a cursor for iterating through all customers.

??? example "Solution"
    ```sql
    declare cur_customer cursor
        for select ID from Customer
    declare @CustomerId int
    declare @Total float

    open cur_customer
    fetch next from cur_customer into @CustomerId
    while @@FETCH_STATUS = 0
    begin

        select @Total = sum(oi.Amount * oi.Price)
        from CustomerSite s
        inner join [Order] o on o.CustomerSiteID=s.ID
        inner join OrderItem oi on oi.OrderID=o.ID
        where s.CustomerID = @CustomerId

        update Customer
        set Total = ISNULL(@Total, 0)
        where ID = @CustomerId

        fetch next from cur_customer into @CustomerId
    end

    close cur_customer
    deallocate cur_customer
    ```

    To verify check the contents of the `Customer` table.

## Exercise 5: Maintenance of the total value (individual exercise)

The values calculated in the previous exercise contain the current state. Create a trigger that updates this value whenever a related order is changed. Instead of re-calculating the value, update it with the changes made!

??? example "Solution"
    The key in the solution is recognizing which table the trigger should be placed on. We are interested in changes in order, but the total value actually depends on the items registered for an order; thus the trigger should react to changes in the order items.

    The exercise is complicated because the `inserted` and `deleted` tables may contain multiple records, possibly even related to multiple customers. A solution for overcoming this obstacle is to use a cursor to process all changes; another option, as below, is aggregating the changes by customer.

    **Trigger**

    ```sql
    create or alter trigger CustomerTotalUpdate
    on OrderItem
    for insert, update, delete
    as

    update Customer
    set Total=isnull(Total,0) + TotalChange
    from Customer
    inner join
        (select s.CustomerId, sum(Amount * Price) as TotalChange
        from CustomerSite s
        inner join [Order] o on o.CustomerSiteID=s.ID
        inner join inserted i on i.OrderID=o.ID
        group by s.CustomerId) CustomerChange on Customer.ID = CustomerChange.CustomerId

    update Customer
    set Total=isnull(Total,0) - TotalChange
    from Customer
    inner join
        (select s.CustomerId, sum(Amount * Price) as TotalChange
        from CustomerSite s
        inner join [Order] o on o.CustomerSiteID=s.ID
        inner join deleted d on d.OrderID=o.ID
        group by s.CustomerID) CustomerChange on Customer.ID = CustomerChange.CustomerId
    ```

    **Testing**

    Let us remember the total for the customers.

    ```sql
    select ID, Total
    from Customer
    ```

    Change the ordered amount.

    ```sql
    update OrderItem
    set Amount=3
    where ID=1
    ```

    Check the totals again, should have changed.

    ```sql
    select ID, Total
    from Customer
    ```
