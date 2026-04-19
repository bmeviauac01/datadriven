# Exam Preparation and AI-Assisted Practice

This page contains helper materials, sample exercises, and AI-based study strategies for the Data-driven Systems course exam. All examples can be run against the [sample database](../../db/index.md); the main tables are summarised below.

!!! info "Sample database – main tables"
    - **Product** (ID, Name, Price, Stock, VATID, CategoryID, Description)
    - **Category** (ID, Name, ParentCategoryID)
    - **VAT** (ID, Percentage)
    - **Customer** (ID, Name, Login, Password, Email, MainCustomerSiteID)
    - **CustomerSite** (ID, CustomerID, ZipCode, City, Street, Tel, Fax)
    - **Order** (ID, CustomerID, Date, DeadLine, StatusID, PaymentMethodID, SiteID)
    - **OrderItem** (ID, OrderID, ProductID, Amount, Price)
    - **Status** (ID, Name)
    - **PaymentMethod** (ID, Method, Deadline)

---

## How to Use AI for Exam Preparation

Large language models (ChatGPT, GitHub Copilot, Gemini, etc.) are excellent tools for **generating exam-style exercises** and getting **instant feedback** on your solutions. The following workflow is proven to be effective.

### General workflow

```
1.  Give the AI context (schema, technology, difficulty level).
2.  Ask it for an exercise.
3.  Solve the exercise ON YOUR OWN – do not look at the solution yet!
4.  Ask the AI to evaluate your solution.
5.  Ask it to show a better/alternative solution and explain it.
6.  Repeat steps 2–5 for other topics.
```

!!! warning "Important"
    AI can sometimes generate incorrect code. Always run the output in your own environment, and if the result looks suspicious, question it!

### Useful AI prompts – template collection

Copy the prompts below into an AI chat (e.g. ChatGPT, Copilot). Replace the parts in `[ ]` with your own values.

---

#### Exercise generation – general template

```
You are a database course examiner at BME (Budapest University of Technology).
The database schema is:
  Product(ID, Name, Price, Stock, VATID, CategoryID)
  Category(ID, Name, ParentCategoryID)
  VAT(ID, Percentage)
  Customer(ID, Name, Login, Password, Email, MainCustomerSiteID)
  CustomerSite(ID, CustomerID, ZipCode, City, Street)
  Order(ID, CustomerID, Date, DeadLine, StatusID, PaymentMethodID, SiteID)
  OrderItem(ID, OrderID, ProductID, Amount, Price)
  Status(ID, Name)
  PaymentMethod(ID, Method, Deadline)

Give me a [DIFFICULTY: easy/medium/hard] [TOPIC] exercise.
Do not provide the solution yet – only the problem statement!
```

---

#### Solution evaluation

```
Evaluate my [SQL/C# LINQ/EF/MongoDB] solution to the previous exercise.
Point out any mistakes and explain why. If it is correct, show an alternative solution.

My solution:
[PASTE YOUR SOLUTION HERE]
```

---

#### Exam simulation

```
Simulate an exam for the Data-driven Systems course!
Ask me 5 exercises one at a time from the following topics:
[e.g. SQL queries, transaction management, Entity Framework, MongoDB]
Wait for my answer after each exercise before moving to the next.
At the end, give me an overall evaluation.
```

---

## Sample Exercises by Topic

### SQL Queries

The following exercises relate to the `SQL` and `Transaction` topics, as well as the MSSQL seminar material.

??? example "Exercise 1 – Filtering and sorting"
    **Task:** List the name and price of all products whose price is between 1000 and 5000, and that have a description. Order the results by price in descending order.

    ??? success "Solution"
        ```sql
        SELECT Name, Price
        FROM Product
        WHERE Price BETWEEN 1000 AND 5000
          AND Description IS NOT NULL
        ORDER BY Price DESC
        ```

??? example "Exercise 2 – Aggregation and grouping"
    **Task:** Show the average price and the number of products per category. Only include categories that have at least 3 products.

    ??? success "Solution"
        ```sql
        SELECT c.Name, COUNT(p.ID) AS ProductCount, AVG(p.Price) AS AvgPrice
        FROM Category c
             INNER JOIN Product p ON p.CategoryID = c.ID
        GROUP BY c.Name
        HAVING COUNT(p.ID) >= 3
        ```

??? example "Exercise 3 – Join and subquery"
    **Task:** Find all order items where the recorded unit price differs from the product's current price.

    ??? success "Solution"
        ```sql
        SELECT oi.ID, p.Name, oi.Price AS RecordedPrice, p.Price AS CurrentPrice
        FROM OrderItem oi
             INNER JOIN Product p ON oi.ProductID = p.ID
        WHERE oi.Price <> p.Price
        ```

??? example "Exercise 4 – Outer join"
    **Task:** List all payment methods and the number of orders placed with each. Include payment methods that have received no orders.

    ??? success "Solution"
        ```sql
        SELECT pm.Method, COUNT(o.ID) AS OrderCount
        FROM PaymentMethod pm
             LEFT OUTER JOIN [Order] o ON o.PaymentMethodID = pm.ID
        GROUP BY pm.Method
        ```

??? example "Exercise 5 – Stored procedure"
    **Task:** Write a stored procedure that accepts a customer ID as a parameter and returns the total value of all orders placed by that customer (sum of `OrderItem.Amount * OrderItem.Price`).

    ??? success "Solution"
        ```sql
        CREATE OR ALTER PROCEDURE GetCustomerOrderTotal
            @CustomerID INT
        AS
        BEGIN
            SELECT SUM(oi.Amount * oi.Price) AS TotalAmount
            FROM [Order] o
                 INNER JOIN OrderItem oi ON oi.OrderID = o.ID
            WHERE o.CustomerID = @CustomerID
        END
        ```

---

### LINQ Queries

The following exercises relate to the `Linq` material.

??? example "Exercise 1 – Filtering and projection"
    **Task:** Using LINQ, retrieve the name and price of all products whose price is less than 2000.

    ??? success "Solution"
        ```csharp
        // Fluent syntax
        var result = products.Where(p => p.Price < 2000)
                             .Select(p => new { p.Name, p.Price });

        // Query syntax
        var result = from p in products
                     where p.Price < 2000
                     select new { p.Name, p.Price };
        ```

??? example "Exercise 2 – Join"
    **Task:** Join the `products` and `vat` lists and display each product's name alongside its VAT percentage.

    ??? success "Solution"
        ```csharp
        var result = from p in products
                     join v in vat on p.VATID equals v.ID
                     select new { p.Name, v.Percentage };
        ```

??? example "Exercise 3 – Grouping"
    **Task:** Group products by VAT ID and provide the product count and average price for each group.

    ??? success "Solution"
        ```csharp
        var result = products
            .GroupBy(p => p.VATID)
            .Select(g => new
            {
                VatID    = g.Key,
                Count    = g.Count(),
                AvgPrice = g.Average(p => p.Price)
            });
        ```

---

### Entity Framework Core

The following exercises relate to the `Entity Framework` material and the EF seminar.

??? example "Exercise 1 – Query with navigation property"
    **Task:** Using EF Core, retrieve the list of orders (including the customer name) whose status is "Delivered".

    ??? success "Solution"
        ```csharp
        using (var db = new AdatvezDbContext())
        {
            var orders = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Status)
                .Where(o => o.Status.Name == "Delivered")
                .Select(o => new { o.Customer.Name, o.Date })
                .ToList();
        }
        ```

??? example "Exercise 2 – Update"
    **Task:** Using EF Core, increase the price of all products with a stock of 0 by 10%.

    ??? success "Solution"
        ```csharp
        using (var db = new AdatvezDbContext())
        {
            var products = db.Products.Where(p => p.Stock == 0).ToList();
            foreach (var p in products)
                p.Price = (int)(p.Price * 1.1);
            db.SaveChanges();
        }
        ```

??? example "Exercise 3 – Insert new record"
    **Task:** Add a new top-level category called "Electronics" (no parent category).

    ??? success "Solution"
        ```csharp
        using (var db = new AdatvezDbContext())
        {
            db.Categories.Add(new Category { Name = "Electronics", ParentCategoryID = null });
            db.SaveChanges();
        }
        ```

---

### MongoDB Operations

The following exercises relate to the `MongoDB` material and the MongoDB seminar. Both MongoDB Shell and .NET Driver syntax are shown.

??? example "Exercise 1 – Filtering"
    **Task:** Retrieve all products whose price is greater than 1000 and whose category name is "Books".

    ??? success "Solution (MongoDB Shell)"
        ```javascript
        db.products.find({
            price: { $gt: 1000 },
            "category.name": "Books"
        })
        ```

    ??? success "Solution (.NET Driver)"
        ```csharp
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Gt(p => p.Price, 1000),
            Builders<Product>.Filter.Eq(p => p.Category.Name, "Books")
        );
        var products = collection.Find(filter).ToList();
        ```

??? example "Exercise 2 – Update"
    **Task:** Increase the price of all documents where `Stock` equals 0 by 500.

    ??? success "Solution (MongoDB Shell)"
        ```javascript
        db.products.updateMany(
            { stock: 0 },
            { $inc: { price: 500 } }
        )
        ```

    ??? success "Solution (.NET Driver)"
        ```csharp
        var filter = Builders<Product>.Filter.Eq(p => p.Stock, 0);
        var update = Builders<Product>.Update.Inc(p => p.Price, 500);
        collection.UpdateMany(filter, update);
        ```

??? example "Exercise 3 – Aggregation pipeline"
    **Task:** Count products per category and list the results in descending order.

    ??? success "Solution (MongoDB Shell)"
        ```javascript
        db.products.aggregate([
            { $group: { _id: "$category.name", count: { $sum: 1 } } },
            { $sort:  { count: -1 } }
        ])
        ```

---

### Transaction Management

The following exercises relate to the `Transactions` material.

??? example "Exercise 1 – Isolation level comparison"
    **Task:** What is the difference between the `READ COMMITTED` and `REPEATABLE READ` isolation levels? When should each be used?

    ??? success "Solution"
        - **READ COMMITTED**: a transaction only sees already-committed data. *Non-repeatable reads* are still possible – reading the same row twice may yield different results if another transaction modifies it in between.
        - **REPEATABLE READ**: once a transaction reads a row, no other transaction can modify that row until the first one completes. This prevents *non-repeatable reads*, but *phantom reads* can still occur.

        **When to use which?** If the business logic requires that rows read during a transaction must not change before the transaction ends (e.g., a check-then-modify pattern), use `REPEATABLE READ` or `SERIALIZABLE`.

??? example "Exercise 2 – Deadlock identification"
    **Task:** You observe two concurrent transactions:

    - T1: locks table A, then requests table B
    - T2: locks table B, then requests table A

    What happens? How can it be prevented?

    ??? success "Solution"
        This is a classic **deadlock**: T1 and T2 are each waiting for a resource held by the other, so neither can proceed.

        **Prevention:** always acquire locks in the same order (e.g., every transaction requests A before B). SQL Server automatically detects deadlocks and rolls back one of the transactions (the *deadlock victim*).

---

## Self-Test Questions – AI Prompts by Topic

Use the prompts below mainly for solo study, to simulate oral exam questions.

### SQL and MSSQL

```
Quiz me verbally on the following MSSQL topics:
- JOIN types (INNER, LEFT, RIGHT, FULL OUTER)
- Difference between WHERE and HAVING with GROUP BY
- Stored procedures and triggers
- Role and types of indexes

Ask me 3 questions in a row and evaluate each of my answers!
```

### Entity Framework

```
Quiz me verbally on the following Entity Framework Core topics:
- Code-First approach and migrations
- Navigation properties and the role of Include()
- Change tracker and how SaveChanges() works
- The N+1 query problem and how to avoid it

Ask me 3 questions in a row!
```

### MongoDB

```
Quiz me verbally on the following MongoDB topics:
- Differences between document and relational data models
- Embedding vs. referencing – when to use which?
- Main aggregation pipeline stages ($match, $group, $sort, $lookup)
- Indexes in MongoDB

Ask me 3 questions in a row!
```

### Transaction Management

```
Quiz me verbally on the following transaction management topics:
- ACID properties explained
- Isolation levels and their anomalies (dirty read, non-repeatable read, phantom read)
- Deadlocks and how to prevent them
- Optimistic vs. pessimistic concurrency control

Ask me 3 questions in a row!
```

---

## Tips for Effective Exam Preparation

!!! tip "On exam day"
    - Read the question twice before writing anything.
    - Aim for simple solutions – readable and correct code beats clever tricks.
    - Make a sketch and optimize it before writting it to the exam paper.
    - When unsure about a SQL query, work through the mental model: `FROM` → `WHERE` → `GROUP BY` → `HAVING` → `SELECT`.
