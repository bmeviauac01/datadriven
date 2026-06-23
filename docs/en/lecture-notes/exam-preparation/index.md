# Exam Preparation and AI-Assisted Practice

General rules and guidelines for using AI in the university context: https://vik.bme.hu/hallgatoknak/altalanos/mi-hasznalat-ajanlasok

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

#### Targeted practice for weak topics

If you know which constructs you struggle with, **tell the AI explicitly** so it keeps drilling you on exactly those. This is much more effective than generic practice.

```
I am weak at [e.g. HAVING and GROUP BY, correlated subqueries, window
functions, LEFT OUTER JOIN, EF Include, MongoDB aggregation pipeline].

Give me 5 exercises, one at a time, that specifically require these
constructs. Start easy and increase the difficulty after each correct
answer. Do not provide the solution until I have answered.
After each of my answers, tell me whether I used the targeted construct
correctly, and if not, explain what I missed.
```

!!! tip "Diagnose your weak spots first"
    Not sure where you are weak? Ask the AI to find out: *"Give me a short mixed diagnostic quiz (one question per topic: joins, grouping/HAVING, subqueries, window functions, transactions). Based on my answers, tell me which topics I should practice more."* Then feed those topics back into the prompt above.

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

The following exercises relate to the `SQL`, `MSSQL server-side programming` and `Transaction` topics, as well as the MSSQL seminar material. They are intentionally harder and span the full range of the lecture notes (window functions, CTEs, correlated subqueries, conditional aggregation, XML/XQuery, server-side programming, triggers and cursors).

??? example "Exercise 1 – Window functions and per-group ranking"
    **Task:** For every category, return the **two most expensive products** (category name, product name, price). If several products share the same price, all of the tied products must be included. Also show the product's rank within its category.

    ??? success "Solution"
        Use `DENSE_RANK()` so ties share a rank and the "top 2" is not cut off arbitrarily.

        ```sql
        WITH Ranked AS
        (
            SELECT c.Name AS Category, p.Name AS Product, p.Price,
                   DENSE_RANK() OVER (PARTITION BY p.CategoryID ORDER BY p.Price DESC) AS rnk
            FROM Product p
                 INNER JOIN Category c ON c.ID = p.CategoryID
        )
        SELECT Category, Product, Price, rnk
        FROM Ranked
        WHERE rnk <= 2
        ORDER BY Category, Price DESC
        ```

??? example "Exercise 2 – CTE and paging"
    **Task:** List products ordered alphabetically by name, returning **only the second page of 10 rows** (rows 11–20). Output the row position as well. Provide both a CTE-based and an `OFFSET/FETCH` solution.

    ??? success "Solution"
        ```sql
        -- CTE + ROW_NUMBER
        WITH q AS
        (
            SELECT ROW_NUMBER() OVER (ORDER BY Name) AS rn, Name, Price
            FROM Product
        )
        SELECT rn, Name, Price
        FROM q
        WHERE rn BETWEEN 11 AND 20
        ORDER BY rn

        -- MSSQL 2012+ paging syntax
        SELECT Name, Price
        FROM Product
        ORDER BY Name
        OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY
        ```

??? example "Exercise 3 – Correlated subquery / anti-join"
    **Task:** List the names of all customers who have **never placed an order**. Show a `NOT EXISTS` solution and explain why a `LEFT JOIN ... IS NULL` solution is equivalent.

    ??? success "Solution"
        ```sql
        SELECT c.Name
        FROM Customer c
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [Order] o
            WHERE o.CustomerID = c.ID
        )
        ```

        Equivalent anti-join:

        ```sql
        SELECT c.Name
        FROM Customer c
             LEFT JOIN [Order] o ON o.CustomerID = c.ID
        WHERE o.ID IS NULL
        ```

??? example "Exercise 4 – Multi-table aggregation with HAVING"
    **Task:** Find the customers whose **total amount spent** (sum of `OrderItem.Amount * OrderItem.Price` across all their orders) exceeds 100000. Show the customer name and the total, in descending order of the total.

    ??? success "Solution"
        ```sql
        SELECT c.Name, SUM(oi.Amount * oi.Price) AS TotalSpent
        FROM Customer c
             INNER JOIN [Order] o      ON o.CustomerID = c.ID
             INNER JOIN OrderItem oi   ON oi.OrderID = o.ID
        GROUP BY c.ID, c.Name
        HAVING SUM(oi.Amount * oi.Price) > 100000
        ORDER BY TotalSpent DESC
        ```

??? example "Exercise 5 – Conditional aggregation (pivot-style)"
    **Task:** Produce **one row per category** showing the category name, how many of its products are in stock (`Stock > 0`) and how many are out of stock (`Stock = 0`).

    ??? success "Solution"
        ```sql
        SELECT c.Name,
               SUM(CASE WHEN p.Stock > 0 THEN 1 ELSE 0 END) AS InStock,
               SUM(CASE WHEN p.Stock = 0 THEN 1 ELSE 0 END) AS OutOfStock
        FROM Category c
             INNER JOIN Product p ON p.CategoryID = c.ID
        GROUP BY c.ID, c.Name
        ```

??? example "Exercise 6 – Querying XML with XQuery"
    **Task:** The `Description` column of `Product` stores XML. List the name of every product whose description specifies **more than one package** (`/product/package_parameters/number_of_packages`).

    ??? success "Solution"
        ```sql
        SELECT Name
        FROM Product
        WHERE Description.value('(/product/package_parameters/number_of_packages)[1]', 'int') > 1
        ```

        An `exist()`-based alternative for "recommended for ages 0-18 months":

        ```sql
        SELECT Name
        FROM Product
        WHERE Description.exist('(/product)[(./recommended_age)[1] eq "0-18 m"]') = 1
        ```

??? example "Exercise 7 – Correlated UPDATE (MSSQL `UPDATE ... FROM`)"
    **Task:** For every order item whose recorded price is **lower** than the product's current price, overwrite the recorded price with the product's current price.

    ??? success "Solution"
        ```sql
        UPDATE oi
        SET oi.Price = p.Price
        FROM OrderItem oi
             INNER JOIN Product p ON p.ID = oi.ProductID
        WHERE oi.Price < p.Price
        ```

??? example "Exercise 8 – Stored procedure with transaction and error handling"
    **Task:** Write a stored procedure `RecordSale @OrderID, @ProductID, @Amount` that, **inside a single transaction**, checks whether there is enough stock; if so it inserts an `OrderItem` (taking the price from `Product`) and decreases the product's stock; otherwise it raises an error and makes no change. Use an isolation level that prevents another transaction from changing the stock between the check and the update.

    ??? success "Solution"
        ```sql
        CREATE OR ALTER PROCEDURE RecordSale
            @OrderID INT,
            @ProductID INT,
            @Amount INT
        AS
        BEGIN
            SET XACT_ABORT ON;
            SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
            BEGIN TRAN;

            DECLARE @Stock INT, @Price INT;

            SELECT @Stock = Stock, @Price = Price
            FROM Product
            WHERE ID = @ProductID;

            IF @Stock IS NULL
                THROW 51001, 'No such product', 1;

            IF @Stock < @Amount
                THROW 51002, 'Not enough stock', 1;

            INSERT INTO OrderItem (OrderID, ProductID, Amount, Price)
            VALUES (@OrderID, @ProductID, @Amount, @Price);

            UPDATE Product
            SET Stock = Stock - @Amount
            WHERE ID = @ProductID;

            COMMIT;
        END
        ```

        `REPEATABLE READ` guarantees the row read in the stock check cannot be modified by another transaction before the `UPDATE` completes, preventing a lost update / overselling.

??? example "Exercise 9 – DML trigger"
    **Task:** Create an `AFTER INSERT` trigger on `OrderItem` that automatically decreases the corresponding product's stock by the inserted amount. Make sure it works correctly even when several rows are inserted at once.

    ??? success "Solution"
        A trigger fires **once per statement**, and `inserted` may contain multiple rows, so aggregate it – do not assume a single row.

        ```sql
        CREATE OR ALTER TRIGGER OrderItemDecreaseStock
            ON OrderItem
            AFTER INSERT
        AS
        BEGIN
            UPDATE p
            SET p.Stock = p.Stock - i.TotalAmount
            FROM Product p
                 INNER JOIN (SELECT ProductID, SUM(Amount) AS TotalAmount
                             FROM inserted
                             GROUP BY ProductID) i ON i.ProductID = p.ID
        END
        ```

??? example "Exercise 10 – Cursor"
    **Task:** Using a cursor, iterate over products with `Stock < 5`. For each such product: if it has **never** been ordered, delete it; otherwise raise its price by 10%.

    ??? success "Solution"
        ```sql
        DECLARE @ProductID INT, @OrderCount INT;

        DECLARE lowstock_cur CURSOR FOR
            SELECT ID FROM Product WHERE Stock < 5;

        OPEN lowstock_cur;
        FETCH NEXT FROM lowstock_cur INTO @ProductID;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SELECT @OrderCount = COUNT(*)
            FROM OrderItem
            WHERE ProductID = @ProductID;

            IF @OrderCount = 0
                DELETE FROM Product WHERE ID = @ProductID;
            ELSE
                UPDATE Product SET Price = Price * 1.1 WHERE ID = @ProductID;

            FETCH NEXT FROM lowstock_cur INTO @ProductID;
        END
        CLOSE lowstock_cur;
        DEALLOCATE lowstock_cur;
        ```

        Note: this is a teaching example for cursors; in practice a set-based `DELETE` + `UPDATE` pair would be faster.

---

### LINQ Queries

The following exercises relate to the `Linq` material. Assume in-memory lists such as `List<Product> products`, `List<VAT> vat`, `List<Category> categories`, `List<OrderItem> orderItems` are available (matching the schema fields).

??? example "Exercise 1 – Grouping with aggregation and HAVING-style filtering"
    **Task:** Group products by `VATID`. For each group return the VAT id, the product count and the average price, but **only for groups with at least 3 products**, ordered by average price descending.

    ??? success "Solution"
        ```csharp
        // Fluent syntax
        var result = products
            .GroupBy(p => p.VATID)
            .Where(g => g.Count() >= 3)
            .Select(g => new { VatID = g.Key, Count = g.Count(), AvgPrice = g.Average(p => p.Price) })
            .OrderByDescending(x => x.AvgPrice);

        // Query syntax
        var result =
            from p in products
            group p by p.VATID into g
            where g.Count() >= 3
            orderby g.Average(p => p.Price) descending
            select new { VatID = g.Key, Count = g.Count(), AvgPrice = g.Average(p => p.Price) };
        ```

??? example "Exercise 2 – Group join (left outer join in LINQ)"
    **Task:** For every VAT category list the percentage and the **number of products** that use it, including VAT categories that have no products at all.

    ??? success "Solution"
        A `join ... into` (group join) followed by `DefaultIfEmpty` realises a left outer join.

        ```csharp
        var result =
            from v in vat
            join p in products on v.ID equals p.VATID into grp
            select new { v.Percentage, Count = grp.Count() };

        // Fluent equivalent
        var result2 = vat.GroupJoin(
            products,
            v => v.ID,
            p => p.VATID,
            (v, grp) => new { v.Percentage, Count = grp.Count() });
        ```

??? example "Exercise 3 – Set operations"
    **Task:** Produce the names of products that are **either** cheaper than 1000 **or** more expensive than 100000, with no duplicates, using a set operation.

    ??? success "Solution"
        ```csharp
        var cheap     = products.Where(p => p.Price < 1000).Select(p => p.Name);
        var expensive = products.Where(p => p.Price > 100000).Select(p => p.Name);

        var result = cheap.Union(expensive); // Union removes duplicates
        ```

??? example "Exercise 4 – Correlated sub-select in a projection"
    **Task:** For each product output its name and the **total ordered amount** (sum of `OrderItem.Amount` for that product), including products that were never ordered (total 0).

    ??? success "Solution"
        ```csharp
        var result = products.Select(p => new
        {
            p.Name,
            TotalOrdered = orderItems.Where(oi => oi.ProductID == p.ID)
                                     .Sum(oi => oi.Amount)
        });
        ```

??? example "Exercise 5 – Deferred execution (conceptual)"
    **Task:** What does the following code print, and why? When is the query actually executed?

    ```csharp
    var q = products.Where(p => p.Price < 1000);
    products.Add(new Product { Name = "Cheap", Price = 10 });
    Console.WriteLine(q.Count());
    ```

    ??? success "Solution"
        It prints the count **including** the newly added "Cheap" product. `Where` returns an `IEnumerable<T>` descriptor and uses **deferred execution** – nothing is evaluated when `q` is declared. The query is only executed when it is enumerated, here by `Count()`, by which time the new item is already in the list. To "freeze" the result you would call `.ToList()` immediately after the `Where`.

??? example "Exercise 6 – Projection into a named type with a computed field"
    **Task:** Join `products` and `vat` and project into a class `PriceInfo(string Name, int GrossPrice)` where the gross price is `Price * (1 + Percentage/100)`.

    ??? success "Solution"
        ```csharp
        var result =
            from p in products
            join v in vat on p.VATID equals v.ID
            select new PriceInfo(p.Name, p.Price * (100 + v.Percentage) / 100);
        ```

---

### Entity Framework Core

The following exercises relate to the `Entity Framework` material and the EF seminar. Assume a configured `AdatvezDbContext` with `Products`, `Orders`, `OrderItems`, `Categories`, `Customers`, `VATs` `DbSet`s and the navigation properties from the lecture notes.

??? example "Exercise 1 – Eager loading across several levels"
    **Task:** Using EF Core, list each "Delivered" order showing the **customer name** and the **total value of the order** (sum of `OrderItem.Amount * OrderItem.Price`). Load only what you need.

    ??? success "Solution"
        ```csharp
        using var db = new AdatvezDbContext();

        var orders = db.Orders
            .Where(o => o.Status.Name == "Delivered")
            .Select(o => new
            {
                Customer = o.Customer.Name,
                Total = o.OrderItems.Sum(oi => oi.Amount * oi.Price)
            })
            .ToList();
        ```

        Because we **project** to exactly the columns we need, EF translates the navigation accesses into joins and a single SQL query – no `Include` is necessary when projecting.

??? example "Exercise 2 – The N+1 query problem"
    **Task:** The code below runs one query for the products and then one extra query **per product** to fetch the category. Explain why, and rewrite it so a single round-trip loads everything.

    ```csharp
    var products = db.Products.ToList();
    foreach (var p in products)
        Console.WriteLine($"{p.Name} - {p.Category.Name}");
    ```

    ??? success "Solution"
        The first query loads the products only; the navigation property `Category` is **not** loaded eagerly, so each `p.Category.Name` access triggers a separate query (via lazy/explicit loading) – the classic **N+1 problem**. Fix it with eager loading:

        ```csharp
        var products = db.Products
            .Include(p => p.Category)
            .ToList();
        foreach (var p in products)
            Console.WriteLine($"{p.Name} - {p.Category.Name}");
        ```

        `Include` emits a single `LEFT JOIN` so both the product and category data arrive in one query.

??? example "Exercise 3 – Update through a navigation property"
    **Task:** Move every product currently in the category named "Toys" into the category named "Games" (assume both exist).

    ??? success "Solution"
        ```csharp
        using var db = new AdatvezDbContext();

        var games = db.Categories.Single(c => c.Name == "Games");
        var toysProducts = db.Products
            .Where(p => p.Category.Name == "Toys")
            .ToList();

        foreach (var p in toysProducts)
            p.CategoryID = games.ID;   // change tracker records the modification

        db.SaveChanges();              // one UPDATE per changed row, in a transaction
        ```

??? example "Exercise 4 – Explicit loading"
    **Task:** You already have a single `Product` loaded and later decide you need its `VAT`. Load the related entity **without** enabling global lazy loading and **without** re-querying the product.

    ??? success "Solution"
        ```csharp
        var product = db.Products.First();
        // product.VAT is null here

        db.Entry(product).Reference(p => p.VAT).Load();
        Console.WriteLine(product.VAT.Percentage); // now populated
        ```

        For a collection navigation use `.Collection(...)` instead of `.Reference(...)`.

??? example "Exercise 5 – Configuring a relationship (Fluent API)"
    **Task:** In `OnModelCreating`, configure the one-to-many relationship between `Category` (one) and `Product` (many) explicitly, with `Product.CategoryID` as the foreign key.

    ??? success "Solution"
        ```csharp
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID);
        }
        ```

??? example "Exercise 6 – Reasoning about the generated SQL"
    **Task:** What is the difference between these two queries in terms of the SQL produced and the data returned?

    ```csharp
    var a = db.Products.Where(p => p.Name.Contains("Lego")).Select(p => p.VAT.Percentage);
    var b = db.Products.Include(p => p.VAT).Where(p => p.Name.Contains("Lego"));
    ```

    ??? success "Solution"
        - Query `a` **projects** a single scalar (`Percentage`), so the SQL selects just that column with a `LEFT JOIN` to `VAT`; the result is `IQueryable<int>`.
        - Query `b` returns whole `Product` entities **with** their `VAT` populated; the SQL selects all `Product` and `VAT` columns via a `LEFT JOIN`, returning `IQueryable<Product>`.

        Use `ToQueryString()` to inspect the generated SQL:

        ```csharp
        Console.WriteLine(a.ToQueryString());
        ```

---

### MongoDB Operations

The following exercises relate to the `MongoDB` material and the MongoDB seminar. Assume the `Product` mapping from the lecture notes (`Name`, `Price`, `Stock`, `string[] Categories`, embedded `VAT { VATCategoryName, Percentage }`). Both MongoDB Shell and .NET Driver syntax are shown where relevant.

??? example "Exercise 1 – Composite filter with builders"
    **Task:** Retrieve all products that are in the "Balls" category, cost between 500 and 1000 (inclusive), **and** have a VAT percentage that is not 27. Write the .NET Driver builder version.

    ??? success "Solution (.NET Driver)"
        ```csharp
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.AnyEq(p => p.Categories, "Balls"),
            Builders<Product>.Filter.Gte(p => p.Price, 500),
            Builders<Product>.Filter.Lte(p => p.Price, 1000),
            Builders<Product>.Filter.Ne(p => p.VAT.Percentage, 27)
        );
        var result = collection.Find(filter).ToList();
        ```

??? example "Exercise 2 – Array-field filtering"
    **Task:** Find products that belong to **at least one** category other than "Balls" and "Rackets". Then find products that are in **both** "Balls" and "Outdoor".

    ??? success "Solution (.NET Driver)"
        ```csharp
        // at least one category not in the listed ones
        var notListed = collection.Find(
            Builders<Product>.Filter.AnyNin(p => p.Categories, new[] { "Balls", "Rackets" })).ToList();

        // contains all of the listed categories
        var both = collection.Find(
            Builders<Product>.Filter.All(p => p.Categories, new[] { "Balls", "Outdoor" })).ToList();
        ```

        Shell equivalents use `$nin` (with `$elemMatch` semantics for arrays) and `$all`.

??? example "Exercise 3 – Embedded document and existence"
    **Task:** List products that **have** an embedded `VAT` document whose percentage is less than 27. Remember that a missing embedded document must not cause an error.

    ??? success "Solution (.NET Driver)"
        ```csharp
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Exists(p => p.VAT),
            Builders<Product>.Filter.Lt(p => p.VAT.Percentage, 27)
        );
        var result = collection.Find(filter).ToList();
        ```

??? example "Exercise 4 – Partial update with multiple operators"
    **Task:** For every product in the "Clearance" category: decrease the price by 10% **is not allowed via $inc**, so instead reduce the stock by 1, set a new field `OnSale = true`, and add "Discounted" to the `Categories` array (without creating duplicates). Do it in a single `UpdateMany`.

    ??? success "Solution (.NET Driver)"
        ```csharp
        var filter = Builders<Product>.Filter.AnyEq(p => p.Categories, "Clearance");
        var update = Builders<Product>.Update
            .Inc(p => p.Stock, -1)
            .Set("OnSale", true)
            .AddToSet(p => p.Categories, "Discounted");

        collection.UpdateMany(filter, update);
        ```

        `$inc` adds a (possibly negative) value; `$addToSet` avoids duplicate array entries (unlike `$push`).

??? example "Exercise 5 – Aggregation pipeline"
    **Task:** For products in the "Balls" category, group by VAT percentage and return, per group, the **number of products** and the **total stock**, sorted by total stock descending.

    ??? success "Solution (.NET Driver)"
        ```csharp
        var result = collection.Aggregate()
            .Match(Builders<Product>.Filter.AnyEq(p => p.Categories, "Balls"))
            .Group(
                p => p.VAT.Percentage,
                g => new
                {
                    Percentage = g.Key,
                    Count = g.Count(),
                    TotalStock = g.Sum(p => p.Stock)
                })
            .SortByDescending(x => x.TotalStock)
            .ToList();
        ```

    ??? success "Solution (MongoDB Shell)"
        ```javascript
        db.products.aggregate([
            { $match: { categories: "Balls" } },
            { $group: { _id: "$vat.percentage",
                        count: { $sum: 1 },
                        totalStock: { $sum: "$stock" } } },
            { $sort: { totalStock: -1 } }
        ])
        ```

??? example "Exercise 6 – Sorted paging"
    **Task:** Return the third page of 20 products ordered by name ascending, then by price descending.

    ??? success "Solution (.NET Driver)"
        ```csharp
        var page = collection.Find(Builders<Product>.Filter.Empty)
            .Sort(Builders<Product>.Sort.Combine(
                Builders<Product>.Sort.Ascending(p => p.Name),
                Builders<Product>.Sort.Descending(p => p.Price)))
            .Skip(40)   // pages 1 and 2 = 40 documents
            .Limit(20)
            .ToList();
        ```

        Without a `Sort`, `Skip`/`Limit` are non-deterministic.

??? example "Exercise 7 – Replace vs. update (conceptual)"
    **Task:** Explain the difference between `ReplaceOne` and `UpdateOne`/`$set`, and what happens to fields you do not mention in each case.

    ??? success "Solution"
        - `ReplaceOne` swaps the **entire** matched document for the new one (keeping the same `_id`). Any field not present in the replacement object is **lost**.
        - `UpdateOne` with `$set` (and other update operators) modifies **only** the specified fields and leaves all other fields untouched. This is the safe choice for partial modifications and is also more efficient on large documents.

---

### Transaction Management

The following exercises relate to the `Transactions` material, covering isolation problems, isolation levels, locking/deadlocks and transaction logging.

??? example "Exercise 1 – Identify the anomaly"
    **Task:** Two transactions run concurrently:

    - T1: reads the stock of product #5 (gets 10).
    - T2: reads the stock of product #5 (gets 10), sets it to 8, commits.
    - T1: sets the stock to 9 (10 − 1), commits.

    Which isolation anomaly occurred, what is the final stock, and what should it have been?

    ??? success "Solution"
        This is a **lost update**. The final stock is 9 (T1's write), but T2's decrement to 8 was overwritten as if it never happened. With both decrements applied the correct value should be 7 (or at least T2's committed 8 should not be silently lost). Prevent it with `REPEATABLE READ`/`SERIALIZABLE`, pessimistic locking, or optimistic concurrency (row version check on update).

??? example "Exercise 2 – Choose the isolation level"
    **Task:** A report sums the value of all orders, iterating the order items in several queries. During the report, money must not appear or disappear, i.e. **no new order items may become visible and no read row may change** while the report runs. Which isolation level is needed and why is the next-lower one insufficient?

    ??? success "Solution"
        Use **SERIALIZABLE**. `REPEATABLE READ` would prevent the already-read rows from changing (no dirty/non-repeatable read), but it still allows **phantom rows** – newly inserted order items matching the query could appear in a later query of the same transaction. Only `SERIALIZABLE` also prevents phantoms.

??? example "Exercise 3 – Optimistic vs. pessimistic concurrency"
    **Task:** Explain how you would prevent the lost update of Exercise 1 with (a) pessimistic and (b) optimistic concurrency control, and name a trade-off of each.

    ??? success "Solution"
        - **Pessimistic:** acquire a lock when reading the row (e.g. higher isolation level / `UPDLOCK` hint) so the other transaction blocks until the first finishes. Trade-off: reduced concurrency/throughput and risk of deadlocks.
        - **Optimistic:** do not lock; keep a version/timestamp column and, on update, verify the row has not changed since it was read (`WHERE Version = @readVersion`). If 0 rows are affected, someone else changed it – retry. Trade-off: wasted work and a retry loop under high contention.

??? example "Exercise 4 – Transaction logging trace"
    **Task:** A transaction decreases A by 2 (10→8) and increases B by 2 (20→22). Using **undo logging**, what is written to the log for the two writes, and in what order must the log flush and the database writes happen at commit?

    ??? success "Solution"
        Undo logging records the **original** values:

        | Operation | Log entry |
        |---|---|
        | Write(A) | `T1, A, 10` |
        | Write(B) | `T1, B, 20` |

        At commit the order is: **flush the log first**, then write A and B to the database files, then write the `Commit T1` mark. On recovery, any transaction **without** a commit mark is undone using the original values from the log. (Contrast: redo logging stores the *final* values 8 and 22 and writes the commit mark *before* the database files.)

??? example "Exercise 5 – Deadlock diagnosis in MSSQL"
    **Task:** Two transactions update tables `Lefty` and `Righty` in opposite order and deadlock. Which DMV queries let you see (a) the locks each session holds and (b) which session is blocking which? What does SQL Server do automatically?

    ??? success "Solution"
        (a) Current locks and the tables they sit on:

        ```sql
        SELECT OBJECT_NAME(P.object_id) AS TableName,
               Resource_type, request_status, request_session_id
        FROM sys.dm_tran_locks dtl
             JOIN sys.partitions P ON dtl.resource_associated_entity_id = p.hobt_id
        ```

        (b) Blocking relationships:

        ```sql
        SELECT blocking_session_id AS BlockingSessionID,
               session_id AS VictimSessionID,
               wait_time/1000 AS WaitDurationSecond
        FROM sys.dm_exec_requests
        WHERE blocking_session_id > 0
        ```

        SQL Server's deadlock monitor detects the cycle and **aborts one transaction (the deadlock victim)**, rolling back all its changes. The application must catch this and **retry**. Deadlocks can be made less likely by always acquiring locks in the **same order**.

---

## Self-Test Questions – AI Prompts by Topic

Use the prompts below mainly for solo study, to simulate oral exam questions. It is adviced to provide the slides for the agent, so it can work from a richer context.

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
