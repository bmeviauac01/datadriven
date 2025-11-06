# Transactions

The goal is to examine transaction handling of MS SQL Server, understand the practical limits to serializable isolation level, and controlling data dependency in read committed isolation level.

## Pre-requisites

Required tools to complete the tasks:

- Microsoft SQL Server (LocalDB or Express edition)
- SQL Server Management Studio
- Database initialization script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/datadriven/master/overrides/db/mssql.sql)

Recommended to review:

- Transaction properties, isolation levels
- [Microsoft SQL Server usage guide](../../db/mssql.md)

## How to work during the seminar

The seminar is lead by the instructor. After getting to know the tools we use, the exercises are solved together. Experienced behavior is summarized and explained.

!!! info ""
    This guide summarizes and explains the behavior. Before looking at these provided answers, we should think first!

## Exercise 1: Create a database in MS SQL Server

We need a database first. Usually, the database is located on a central server, but we often run a server on our machine for development.

1. Connect to Microsoft SQL Server using SQL Server Management Studio. Start Management Studio and use the following connection details:

    - Server name: `(localdb)\mssqllocaldb`
    - Authentication: `Windows authentication`

1. Create a new database (if it does not exist yet); the name should be your Neptun code: in _Object Explorer_ right-click _Databases_ and choose _Create Database_.

1. Instantiate the sample database using the [script](https://raw.githubusercontent.com/bmeviauac01/datadriven/master/overrides/db/mssql.sql). Open a new _Query_ window, paste the script into the window, then execute it. Make sure to select the right database in the toolbar dropdown.

    ![Select database](images/sql-management-database-dropdown.png)

1. Verify that the tables are created. If the _Tables_ folder was open before, you need to refresh it.

    ![Table list](images/sql-managment-tablak.png).

## Exercise 2: Concurrent transactions

To simulate concurrent transactions, you need two _Query_ windows by clicking the _New Query_ button twice. You can align the windows next to each other by right-clicking the Query header and choosing New Vertical Tab group:

![Two query window](images/sql-management-tab-group.png)

Use the following scheduling. Transaction T1 checks the status of order 4, while transaction T2 changes the status.

1. **T1 transaction**

    ```sql
    -- List the order and the related items with their status
    SELECT s1.Name, p.Name, s2.Name
    FROM [Order] o, OrderItem oi, Status s1, Status s2, Product p
    WHERE o.Id = oi.OrderID
      AND o.ID = 4
      AND o.StatusID = s1.ID
      AND oi.StatusID = s2.ID
      AND p.ID = oi.ProductID
    ```

    !!! note "`[Order]`"
        The brackets in `[Order]` are needed to distinguish it from the `order by` command.

1. **T2 transaction**

    ```sql
    -- Change the status or the order
    UPDATE [Order]
    SET StatusID = 4
    WHERE ID = 4
    ```

1. **T1 transaction**: repeat the same command as in step 1

1. **T2 transaction**

    ```sql
    -- Change the status of each item in the order
    UPDATE OrderItem
    SET StatusID = 4
    WHERE OrderID = 4
    ```

1. **T1 transaction**: repeat the same command as in step 1

??? question "What did you experience? Why?"
    In the beginning, everything was in status "Packaged", which is fine (the items in the order and the order itself had the same status). But after we changed the status of the order, the status seemed controversial: the order and the items had different statuses. We have to understand that the database itself was **not** inconsistent, as the database's integration requirements allow this. However, from a business perspective, there was an inconsistency.

    SQL Server, by default, runs in auto-commit mode. That is, every sql statement is a transaction by itself, which is committed when completed. Thus the problem was that our modifications were executed in separate transactions.

    In order to handle the two changes together, we would need to combine them into a single transaction.

## Exercise 3: using transactions, _read committed_ isolation level

Let us repeat the previous exercise so that the two modifications form a single transaction:

- **T2 transaction** should begin with a `begin tran` command, and finish with a `commit` statement.
- When changing the status, the new status should be 3 (to have an actual change in the data).

??? question "What did you experience? Why?"
    While data modification is underway in **T2**, the query statement in **T1** will wait. It will wait until the data modification transaction is completed. The `select` statement wants to place a reader lock on the records, but the other concurrent transaction is editing these records and has an exclusive writer lock on them.

    Let us remember that the default isolation level is _read committed_. This isolation level on this platform means that data under modification cannot be accessed, not even for reading. This is a matter of implementation; the SQL standard does not specify this (e.g., in Oracle Server the previously committed state of each record is available). In other isolation levels, MSSQL Server behaves differently; e.g., in the _snapshot_ isolation level, the version of the data before the modification is accessible.

### Checking locks with SQL queries

We can also check the state of locks in a third query window using SQL queries while the **T2** transaction is running (i.e., after `BEGIN TRAN` and `UPDATE`, but before `COMMIT` or `ROLLBACK`).

Open a third query window (New Query button) and execute the following query while the **T2** transaction is active:

```sql
-- Query locks
SELECT 
    dtl.resource_type,
    DB_NAME(dtl.resource_database_id) AS database_name,
    OBJECT_NAME(P.object_id) AS object_name,
    dtl.request_mode,
    dtl.request_type,
    dtl.request_status,
    dtl.request_session_id
FROM sys.dm_tran_locks dtl
LEFT JOIN sys.partitions P ON dtl.resource_associated_entity_id = P.hobt_id
WHERE dtl.resource_database_id = DB_ID()
ORDER BY dtl.request_session_id
```

This query displays the locks placed in the current database. The `resource_type` column shows what type of resource is locked (e.g., `OBJECT`, `PAGE`, `KEY`), and the `request_mode` column shows the lock type (e.g., `S` - shared/read, `X` - exclusive/write). The `request_session_id` helps identify which session holds the lock - this can also be found in the Query window title.

We can also get useful information about blocked transactions:

```sql
-- Query blocked sessions
SELECT 
    blocking_session_id AS 'Blocking Session ID',
    session_id AS 'Blocked Session ID',
    wait_type,
    wait_time,
    wait_resource
FROM sys.dm_exec_requests
WHERE blocking_session_id <> 0
```

This query shows if there is a session waiting for a lock held by another session.

## Exercise 4: aborting transactions (_rollback_) in _read committed_ isolation level

Let us repeat the same command sequence, including the transaction, but let us abort the modification operation in the middle.

1. **T1 transaction**

    ```sql
    -- List the order and the related items with their status
    SELECT s1.Name, p.Name, s2.Name
    FROM [Order] o, OrderItem oi, Status s1, Status s2, Product p
    WHERE o.Id = oi.OrderID
      AND o.ID = 4
      AND o.StatusID = s1.ID
      AND oi.StatusID = s2.ID
      AND p.ID = oi.ProductID
    ```

1. **T2 transaction**

    ```sql
    -- Start new transaction
    BEGIN TRAN

    -- Change the order status
    UPDATE [Order]
    SET StatusID = 4
    WHERE ID = 4
    ```

1. **T1 transaction**: repeat the same command as in step 1

1. **T2 transaction**

    ```sql
    -- Abort the transaction
    ROLLBACK
    ```

??? question "What did you experience? Why?"
    Similarly to the previous exercise, the read operation was forced to wait while the modification transaction was underway. When this modification was aborted, the result of the read query arrived immediately. We are still using _read committed_ isolation level; hence we must not see data being modified. But once the modification transaction finished, either successfully with `commit` or with a `rollback`, the data records are once again available.

    Let us understand that we have just avoided the problem of dirty read. If the read query showed us the uncommitted modification, we would have seen values that would have been invalid after the `rollback`.

## Exercise 5: _Read committed snapshot_ isolation level

Before starting the exercise, abort any remaining transactions. Execute a few `rollback` statements in both windows to stop any potentially remaining transactions.

SQL Server supports a special _read committed snapshot_ isolation level, which is a variation of the _read committed_ isolation level. In this mode, when reading records being modified, we don't wait but instead receive the last committed version. This behavior must be enabled at the database level.

First, enable _read committed snapshot_ mode for our database. Execute the following command in a new query window (replace `NEPTUN` with your Neptun code):

```sql
ALTER DATABASE [NEPTUN]
SET READ_COMMITTED_SNAPSHOT ON
```

!!! warning "Important"
    This command only succeeds if there are no other active connections to the database. Close all other query windows or abort any active transactions before executing this command!

Now repeat the steps from Exercise 4.

??? question "What did you experience? How does the behavior differ from Exercise 4?"
    Now the **T1** transaction does not wait but immediately receives the result, even while the **T2** transaction is modifying the data! This is possible because in _read committed snapshot_ mode, the system returns the last committed version, without having to wait for the modification transaction to complete.

    This provides a significant performance advantage, as read transactions are not blocked by write transactions. However, it's important to understand that the data read reflects an earlier state that may have since changed.

    The _read committed snapshot_ mode is particularly useful in applications with heavy read workloads where it's unacceptable for reads to be blocked by modifications. This is also the default behavior of Oracle databases.

At the end of the exercise, we can switch back to the original setting (optional):

```sql
ALTER DATABASE [NEPTUN]
SET READ_COMMITTED_SNAPSHOT OFF
```

## Exercise 6: Placing an order using _serializable_ isolation level

Before we begin, let us get rid of any pending transactions we may have. Let us issue a few `rollback` statements in both windows.

Let us have two concurrent transactions, both placing an order. We must allow an order for a product only if we have enough stock left. To properly isolate the effect of the transactions, let use _serializable_ isolation level.

1. **T1 transaction**

    ```sql
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRAN;

    -- Check the product stock
    SELECT *
    FROM Product
    WHERE ID = 2
    ```

1. **T2 transaction**

    ```sql
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRAN;

    SELECT *
    FROM Product
    WHERE ID = 2
    ```

1. **T1 transaction**

    ```sql
    -- Check the registered, but unprocessed orders for the same product
    SELECT SUM(Amount)
    FROM OrderItem
    WHERE ProductID = 2
      AND StatusID = 1
    ```

1. **T2 transaction**

    ```sql
    SELECT SUM(Amount)
    FROM OrderItem
    WHERE ProductID = 2
      AND StatusID = 1
    ```

1. **T1 transaction**

    ```sql
    -- Since the order can be completed, store the order
    INSERT INTO OrderItem(OrderID, ProductID, Amount, StatusID)
    VALUES(2, 2, 3, 1)
    ```

1. **T2 transaction**

    ```sql
    INSERT INTO OrderItem(OrderID, ProductID, Amount, StatusID)
    VALUES(3, 2, 3, 1)
    ```

1. **T1 transaction**

    ```sql
    COMMIT
    ```

1. **T2 transaction**

    ```sql
    COMMIT
    ```

??? question "What did you experience? Why?"
    A deadlock will occur due to the _serializable_ isolation level, as both transactions require exclusive access to the table `OrderItem`. The query `select sum` - with the requirement to protect from unrepeatable reads - adds reader locks to the records. Thus the  `insert` cannot complete, as it needs write access. This effectively means that both transactions are waiting for a lock that the other one holds.

    The result of the deadlock is the abortion of one of the transactions. This is the expected and correct behavior in these cases because it prevents the same problem we are trying to avoid (to sell more products than we have in stock).

Let us repeat the same sequence of steps, but this time, the products should be different. This simulates two concurrent orders for different products.

- Before we begin, let us get rid of any pending transactions we may have. Let us issue a few `rollback` statements in both windows.
- Let us replace the `ID` or `ProductID` values: one of the transactions should use ID 2 and the other one ID 3.

??? question "What did you experience? Why?"
    Even when the two orders reference different products a deadlock occurs. The locking mechanism behind the statement `select sum` locks the entire table, because it is not able to distinguish the records by `ProductID`. This is not unexpected, as the fact that two concurrent orders referencing different products are allowed, is a business requirement; the database has no knowledge of this.

    In other words, the _serializable_ isolation level is too strict in this case. This is the reason that serializable is not frequently used in practice.

## Exercise 7: Order registration with _read committed_ isolation level

Let us consider what would happen in the previous exercise if the isolation level was left at default? Would there be any deadlock? Would the result be correct?

??? question "What would we expect? Why?"
    If we used the default isolation level, the behavior would be incorrect. The _read committed_ isolation level would not protect us from a concurrent transaction inserting a new record that could potentially result in selling more products than available. This would be a manifestation of the unrepeatable read concurrency problem.

    We can conclude thus that the _serializable_ isolation level was not unnecessary. It did in fact, protect us from a valid concurrency problem.

## Exercise 8: Manual locking

Before we begin, let us get rid of any pending transactions we may have. Let us issue a few `rollback` statements in both windows.

Using _read committed_ isolation level, let us find a solution that only prohibits concurrent orders of the _same product_. You can assume that all copies of the concurrent program run the same logic.

The solution is based on manual locks we can place on records. These locks |(similarly to the automatic ones) have a lifespan identical to the transaction.

```sql
SELECT *
FROM tablename WITH(XLOCK)
...
```

??? question "Where do we place this lock? How does the order registration process look like?"
    The key to this exercise is understanding where the lock should be placed. The question is what to lock. The answer is the **product**: we want to avoid placing orders of the same product. Thus we place a lock on the product record.

    The order process is as follows:

    1. **T1 transaction**

         ```sql hl_lines="1 5"
         SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
         BEGIN TRAN;

         SELECT *
         FROM Product WITH (XLOCK)
         WHERE ID = 2
         ```

    1. **T2 transaction**

         ```sql hl_lines="6"
         SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
         BEGIN TRAN;

         SELECT *
         FROM Product WITH (XLOCK)
         WHERE ID = 3
         ```

    1. **T1 transaction**

         ```sql
         SELECT SUM(Amount)
         FROM OrderItem
         WHERE ProductID = 2
           AND StatusID = 1
         ```

    1. **T2 transaction**

         ```sql hl_lines="3"
         SELECT SUM(Amount)
         FROM OrderItem
         WHERE ProductID = 3
           AND StatusID = 1
         ```

    1. **T1 transaction**

         ```sql
         INSERT INTO OrderItem(OrderID, ProductID, Amount, StatusID)
         VALUES(2, 2, 3, 1)
         ```

    1. **T2 transaction**

         ```sql hl_lines="2"
         INSERT INTO OrderItem(OrderID, ProductID, Amount, StatusID)
         VALUES(3, 3, 3, 1)
         ```

    1. **T1 transaction**

         ```sql
         COMMIT
         ```

    1. **T2 transaction**

         ```sql
         COMMIT
         ```

??? tip "Table locking"

    There is another option for manual locking by locking entire tables:

    ```sql
    SELECT *
    FROM tablename WITH(TABLOCKX)
    ...
    ```
    
    This might seem a simple solution, but why is this a not recommended option? In our scenario, the table lock should be placed on the order item table. But effectively, this would mean the same thing as using _serializable_ isolation level: there would be no deadlocks; however, there would be no concurrency allowed either.
