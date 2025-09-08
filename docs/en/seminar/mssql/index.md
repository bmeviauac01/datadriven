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
        SELECT COUNT(*)
        FROM [Order] o JOIN Status s ON o.StatusID = s.ID
        WHERE s.Name != 'Delivered'
        ```

        We see a `join` and an aggregation here. (There are other syntaxes for joining tables; refer to the lecture notes.)

1. Which payment methods have _not_ been used at all?

    ??? example "Solution"
        ```sql
        SELECT p.Method
        FROM [Order] o RIGHT OUTER JOIN PaymentMethod p ON o.PaymentMethodID = p.ID
        WHERE o.ID IS NULL
        ```

        The key in the solution is the `outer join`, through which we can see the payment method records that have _no_ orders.

1. Let us insert a new customer and query the auto-assigned primary key!

    ??? example "Solution"
        ```sql
        INSERT INTO Customer(Name, Login, Password, Email)
        VALUES ('Test Test', 'tt', '********', 'tt@email.com')

        SELECT @@IDENTITY
        ```

        It is recommended (though not required) to name the columns after `insert` to be unambiguous. No value was assigned to the ID column, as the definition of that column mandates that the database automatically assigns a new value upon insert. We can query this ID after the insert is completed.

1. One of the categories has the wrong name. Let us change _Tricycle_ to _Tricycles_!

    ??? example "Solution"
        ```sql
        UPDATE Category
        SET Name = 'Tricycles'
        WHERE Name = 'Tricycle'
        ```

1. Which category contains the largest number of products?

    ??? example "Solution"
        ```sql
        SELECT TOP 1 Name, (SELECT COUNT(*) FROM Product WHERE Product.CategoryID = c.ID) AS cnt
        FROM Category c
        ORDER BY cnt DESC
        ```

        There are many ways this query can be formulated. This is only one possible solution. It also serves as an example of the usage of subqueries.

## Exercise 2: Inserting a new product category

Create a new stored procedure that helps inserting a new product category. The procedure's inputs are the name of the new category, and optionally the name of the parent category. Raise an error if the category already exists, or the parent category does not exist. Let the database generate the primary key for the insertion.

??? example "Solution"
    **Stored procedure**

    ```sql
    CREATE OR ALTER PROCEDURE AddNewCategory
        @Name NVARCHAR(50),
        @ParentName NVARCHAR(50)
    AS

    BEGIN TRAN

    -- Is there a category with identical name?
    DECLARE @ID INT
    SELECT @ID = ID
    FROM Category WITH (TABLOCKX)
    WHERE UPPER(Name) = UPPER(@Name)

    IF @ID IS NOT NULL
    BEGIN
        ROLLBACK
        THROW 51000, 'Category ' + @Name + ' already exists', 1;
    END

    DECLARE @ParentID INT
    IF @ParentName IS NOT NULL
    BEGIN
        SELECT @ParentID = ID
        FROM Category
        WHERE UPPER(Name) = UPPER(@ParentName)

        IF @ParentID IS NULL
        BEGIN
            ROLLBACK
            THROW 51000, 'Category ' + @ParentName + ' does not exist', 1;
        END
    END

    INSERT INTO Category
    VALUES(@Name, @ParentID)

    COMMIT
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
    CREATE OR ALTER TRIGGER UpdateOrderStatus
    ON [Order]
    FOR UPDATE
    AS

    UPDATE OrderItem
    SET StatusID = i.StatusID
    FROM OrderItem oi
    INNER JOIN inserted i ON i.Id = oi.OrderID
    INNER JOIN deleted d ON d.ID = oi.OrderID
    WHERE i.StatusID != d.StatusID
      AND oi.StatusID = d.StatusID
    ```

    Let us make sure we understand the `update ... from` syntax. The behavior is as follows. We use this command when some of the changes we want to make during the update require data from another table. The syntax is based on the usual `update ... set...` format extended with a `from` part, which follows the same syntax as a `select from`, including the `join` to gather information from other tables. This allows us to use the joined records and their content in the `set` statement (that is, a value from a joined record can be on the right side of an assignment).

    **Testing**

    Let us check the status of the order and each item in the order:

    ```sql
    SELECT OrderItem.StatusID, [Order].StatusID
    FROM OrderItem JOIN [Order] ON OrderItem.OrderID = [Order].ID
    WHERE OrderID = 1
    ```

    Let us change the status of the order:

    ```sql
    UPDATE [Order]
    SET StatusID = 4
    WHERE ID = 1
    ```

    Check the status now (the update should have updated all stauses):

    ```sql
    SELECT OrderItem.StatusID, [Order].StatusID
    FROM OrderItem JOIN [Order] ON OrderItem.OrderID = [Order].ID
    WHERE OrderID = 1
    ```

## Exercise 4: Summing the total purchases of a customer

Let us calculate and store the value of all purchases made by a customer!

1. Add a new column to the table: `ALTER TABLE Customer ADD Total FLOAT`
1. Calculate the current totals. Let us use a cursor for iterating through all customers.

??? example "Solution"
    ```sql
    DECLARE cur_customer CURSOR
        FOR SELECT ID FROM Customer
    DECLARE @CustomerId INT
    DECLARE @Total FLOAT

    OPEN cur_customer
    FETCH NEXT FROM cur_customer INTO @CustomerId
    WHILE @@FETCH_STATUS = 0
    BEGIN

        SELECT @Total = SUM(oi.Amount * oi.Price)
        FROM CustomerSite s
        INNER JOIN [Order] o ON o.CustomerSiteID = s.ID
        INNER JOIN OrderItem oi ON oi.OrderID = o.ID
        WHERE s.CustomerID = @CustomerId

        UPDATE Customer
        SET Total = ISNULL(@Total, 0)
        WHERE ID = @CustomerId

        FETCH NEXT FROM cur_customer INTO @CustomerId
    END

    CLOSE cur_customer
    DEALLOCATE cur_customer
    ```

    To verify check the contents of the `Customer` table.

## Exercise 5: Maintenance of the total value (individual exercise)

The values calculated in the previous exercise contain the current state. Create a trigger that updates this value whenever a related order is changed. Instead of re-calculating the value, update it with the changes made!

??? example "Solution"
    The key in the solution is recognizing which table the trigger should be placed on. We are interested in changes in order, but the total value actually depends on the items registered for an order; thus the trigger should react to changes in the order items.

    The exercise is complicated because the `inserted` and `deleted` tables may contain multiple records, possibly even related to multiple customers. A solution for overcoming this obstacle is to use a cursor to process all changes; another option, as below, is aggregating the changes by customer.

    **Trigger**

    ```sql
    CREATE OR ALTER TRIGGER CustomerTotalUpdate
    ON OrderItem
    FOR INSERT, UPDATE, DELETE
    AS

    UPDATE Customer
    SET Total = ISNULL(Total, 0) + TotalChange
    FROM Customer
    INNER JOIN
        (SELECT s.CustomerId, SUM(Amount * Price) AS TotalChange
        FROM CustomerSite s
        INNER JOIN [Order] o ON o.CustomerSiteID = s.ID
        INNER JOIN inserted i ON i.OrderID = o.ID
        GROUP BY s.CustomerId) CustomerChange ON Customer.ID = CustomerChange.CustomerId

    UPDATE Customer
    SET Total = ISNULL(Total, 0) - TotalChange
    FROM Customer
    INNER JOIN
        (SELECT s.CustomerId, SUM(Amount * Price) AS TotalChange
        FROM CustomerSite s
        INNER JOIN [Order] o ON o.CustomerSiteID = s.ID
        INNER JOIN deleted d ON d.OrderID = o.ID
        GROUP BY s.CustomerID) CustomerChange ON Customer.ID = CustomerChange.CustomerId
    ```

    **Testing**

    Let us remember the total for the customers.

    ```sql
    SELECT ID, Total
    FROM Customer
    ```

    Change the ordered amount.

    ```sql
    UPDATE OrderItem
    SET Amount = 3
    WHERE ID = 1
    ```

    Check the totals again, should have changed.

    ```sql
    SELECT ID, Total
    FROM Customer
    ```
