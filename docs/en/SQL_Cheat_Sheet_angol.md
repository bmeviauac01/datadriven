# Query Syntax Cheat Sheet

In this cheat sheet, the query syntax of various programming languages are compared.

**Important:** When using C# LINQ do not forget about creating the database context in the using block!

```csharp
using (var db = new AdatvezDbContext())
{
    //queries, etc.
}
```

## SELECT

**SQL:**

```SQL
SELECT column1, column2
FROM table
WHERE condition;
```

**C# LINQ:**

**Note:** In the Select() method, an anonymous object is created. Its attributes can be freely chosen to perform the projection operation.
```csharp
// C#-like syntax
var query = context.Table
            .Where(item => condition)
            .Select(item => new { item.Column1, item.Column2 });

 // SQL-like syntax       
var query = from item in context.Table
            where condition
            select new { item.Column1, item.Column2 };
```
**Important:**  If we need entities referenced by navigation properties, the developer specifies this in the code using Include(). The system loads the requested references. **DO NOT** return individual attributes using Select in this case.
```csharp
var query = products
            .Include(p => p.VAT)
            .Where(p => p.ID==23)
            .SingleOrDefault();
// Navigation properties' attributes are accessible externally without lazy loading
if(query!=null){
    Console.WriteLine(query.VAT.ID);
}
```
**C# MongoDb:**

Similar to Select in LINQ, use it when we want to project specific attributes.

```csharp
collection.Find(item=>item.Column1 == value)
        .Project(item => new
        {
            Attr1=item.Coloumn1,
            Attr2=item.Coloumn2
        }).ToList();
```
Projection similar to Select can also be specified in the Group() function, as shown below.

## WHERE
**SQL:**

```SQL
SELECT * FROM table_name WHERE column_name="Example";
```
Query where a field is NULL:
```SQL
SELECT * FROM table_name WHERE column_name IS NULL;
To check the opposite, use IS NOT NULL
```
**ISNULL** function: If there is a value in the salary column, the query continues with that value. Otherwise, it uses the value provided as the second parameter (in this case, 0).
```SQL

SELECT employee_id, employee_name, ISNULL(salary, 0) AS modified_salary
FROM employees;

```

Subqueries can be named, and their results can be referenced. The example below (from the Microsoft SQL seminar) finds the product category with the most items.

```SQL
select top 1 Name, (select count(*) from Product where Product.CategoryID = c.ID) as cnt
from Category c
order by cnt desc
```

**C# LINQ:**
Where price is less than 1000:
```csharp
// C#-like syntax
products.Where(p => p.Price < 1000)
// SQL-like syntax
from p in products
where p.Price < 1000
```
**C# MongoDb:**

**Important:** In MongoDB's .Find() operation, only conditions that apply to the document itself can be used; it is not possible to reference or join with another collection!

**Important:** In MongoDb, you must always include a .Find() in the commands (unless using Aggregate()). A common parameterization because od this constraint is .Find(_ => true) of the Find() command.

**Note:** The result of the Find() function is not the result set; it is just a descriptor for executing the query. To fetch the entire result set as a list, use .ToList(). (If you want to return a single element of a query you should use an other command, see below.)
```csharp
collection.Find(item=>item.Column == value).ToList();
```

## Limiting the result set count
**SQL:**

In cases where you want to retrieve only one or just a few row from your queries.
```SQL
select top 1 * --you can change the number parameter
from Product
order by Name
```
**C#** (LINQ és MongoDb):

If you need only the first element or know there will be only one element, you can use .First(), .FirstOrDefault(), .Single(), or .SingleOrDefault(). Ensure the preceding query indeed returns a single data element when using .Single() or .SingleOrDefault().

Use the .Limit(num) method to limit the number of elements the query returns to the num parameter.

We can also navigate through the results using Take and Skip.

```csharp
// Retrieves 10 items
products.Take(10)

// Skips 10, then retrieves 10
products.Skip(10).Take(10)
```

## Group By
**SQL:**

List the names of products starting with 'M' and their ordered quantities, including products with zero orders.
```SQL
select p.Name, Sum(oi.Amount)
from Product p
     left outer join OrderItem oi on p.id=oi.ProductID
where p.Name like 'M%'
group by p.Name
```

**C# LINQ:**
Group elements by VATID.
```csharp
//c# jellegű szintaktika
products.GroupBy(p => p.VATID)
//SQL szerű szintaktika
from p in products
group p by p.VATID
```
**C# MongoDb:**
Filter elements which are in the "Balls" category and group them by the Vat percentage.
```csharp
collection.Aggregate()
                    .Match(Builders<Product>.Filter.AnyEq(x => x.Categories, "Balls")) // filter
                    .Group(x => x.VAT.Percentage, x => x) // grouping
                    .ToList()
```
Within the Match(), an alternative approach to lambda can be seen. Multiple different keywords and nested Filters can be written here, such as:
```csharp
collection.Find(x => x.Price == 123);
collection.Find(Builders<Product>.Filter.Eq(x => x.Price, 123)); //Eq, mint equals

collection.Find(x => x.Price != 123);
collection.Find(Builders<Product>.Filter.Ne(x => x.Price, 123)); // Ne, mint not equals

collection.Find(x => x.Price >= 123);
collection.Find(Builders<Product>.Filter.Gte(x => x.Price, 123)); // Gte, mint greater than or equal to

collection.Find(x => x.Price < 123);
collection.Find(Builders<Product>.Filter.Lt(x => x.Price, 123)); // Lt, mint less than

collection.Find(x => x.Price < 500 || x.Stock < 10);
collection.Find(
    Builders<Product>.Filter.Or(
        Builders<Product>.Filter.Lt(x => x.Price, 500),
        Builders<Product>.Filter.Lt(x => x.Stock, 10)
    )
);

collection.Find(x => !(x.Price < 500 || x.Stock < 10));
collection.Find(
    Builders<Product>.Filter.Not(
        Builders<Product>.Filter.Or(
            Builders<Product>.Filter.Lt(x => x.Price, 500),
            Builders<Product>.Filter.Lt(x => x.Stock, 10)
        )
    )
);
```

In .Group(), you can specify projections similarly to the .Project() function.
```csharp
var r=productsCollection.Aggregate()
    .Group(
        // grouping section
        p=>p.VAT.Percentage,
        // projection section
        p=>new{
        vatp=p.Key
        sumPrice=p.Sum(s=>s.Price)
    })
    .ToList();

```

## Order By
**SQL:**

```SQL
ORDER BY column1 ASC, column2 DESC;
```

**C# LINQ:**

Two-level sorting
```csharp
.OrderBy(item => item.Column1)
.ThenByDescending(item => item.Column2)
```
**C# MongoDb:**

```csharp
.Sort(Builders<CollectionItem>.Sort.Ascending(item=>item.Coloumn))
```

## JOIN

**SQL:**

```SQL
SELECT table1.column, table2.column
FROM table1
JOIN table2 ON table1.column = table2.column;
```

**C# LINQ:**

Generally, using navigation properties, associated classes can be reached. Explicit joining is necessary only if the task logic requires it or if there's no navigation property in one class pointing to another.
```csharp
var query = context.Table1
            .Join(context.Table2,
                  item1 => item1.Column,
                  item2 => item2.Column,
                  (item1, item2) => new { item1.Column, item2.Column });
// SQL-like LINQ syntax:
var query = from item1 in context.Table1
            join item2 in context.Table2
            on item1.Column equals item2.Column
            select new { item1.Column, item2.Column };
```
**Note:** From this point on, in C# LINQ, only the more convenient query syntax will be used.

**C# MongoDb:**

**A method for server-side joins in MongoDb was not covered. It's done via LINQ after retrieving the entire datasets from the database.** After querying the data, typically, the matching is done client-side using dictionaries with .ToHashSet() and .Contains() methods (See MongoDB seminar excercise 1.5).

## Distinct

**SQL:**

```SQL
select distinct p.Name
from Product p
```

**C# LINQ:**

```csharp
products.Select(p => p.Name).Distinct();
```
**C# MongoDb:**
```csharp
db.orders.distinct("cust_id").ToList();
```

## Oszlopfüggvények

**SQL:**

```SQL
--What is the cost of the most expensive product?
select max(Price)
from Product
--What are the most expensive products?
select *
from Product
where Price=(select max(Price) from Product)
```

**C# LINQ:**

```csharp
products.Count()
// For example, for Max, Min, Sum, similar approaches can be used
products.Select(p => p.Price).Average()
```

**C# MongoDb:**

General aggregations can be constructed using pipelines. A pipeline can return multiple results from a document set (e.g., total, average, maximum, or minimum values)

Max function:

**Note:** Observe the constant grouping inside Group, which is done to calculate the column function over the entire collection.
```csharp
collection.Aggregate().Group(p=>1,p=>p.Max(x=>x.Stock)).Single();
```

Let's see this through the example of grouping. The following query lists the total values of OrderItem records related to an order if the total value exceeds 30,000.
```csharp
var q=ordersCollection.Aggregate()
                .Project(order => new
                {
                    CustomerID = order.CustomerID,
                    Total = order.OrderItems.Sum(oi => oi.Amount * oi.Price)
                })
                .Match(order => order.Total > 30000)
                .ToList();
```

## Delete

**SQL:**

```SQL
delete
from Product
where ID=23
```

**C# LINQ:**

```csharp
using (var db = new AdatvezDbContext())
{
    var deleteThis=db.Products.Select(p=>p.ID=="23").SingleOrDefault();
    if(deleteThis!=null){
        db.Products.Remove(deleteThis);
        db.SaveChanges();
    }
}
```
**C# MongoDb:**


```csharp
var deleteResult = collection.DeleteOne(x => x.Id == new ObjectId("..."));
```
Use DeleteMany if you want to delete multiple records.

## Insert

**SQL:**

```SQL
insert into Product
values ('aa', 100, 0, 3, 2, null)
--When inserting results from another query:
insert into Product (Name, Price)
select Name, Price
from InvoiceItem
where Amount>2
```

**C# LINQ:**

```csharp
using (var db = new AdatvezDbContext())
{
    db.Table.Add(new dataItem { Name = "Example" });
    db.SaveChanges();
}
```
**C# MongoDb:**

```csharp
var newProduct = new Product
{
    Name = "Apple",
    Price = 890,
    Categories = new[] { "Fruits" }
};
collection.InsertOne(newProduct);
```

## Update

**SQL:**

Task: Increase the prices of products containing the word "Lego" in their names by 10%.
```SQL
update Product
set Price=1.1*Price
where Name like '%Lego%'
```
If you want to assign values from another table in the SET command, as shown in the example below (from the Microsoft SQL seminar):
Task: Copy the status of order ID 9 to all corresponding OrderItems.
```SQL
UPDATE OrderItem
SET StatusID = o.StatusID
FROM OrderItem oi
INNER JOIN Order o ON o.Id = oi.OrderID
WHERE o.ID=9;
```

**C# LINQ:**

(Spoiler for the solution to Entity Framework seminar's task 3.)

Task: Write C# code based on LINQ that increases the prices of products in the "LEGO" category by 10%.
```csharp
using (var db = new AdatvezDbContext())
{
    var legoProductsQuery = db.Products.Where(p => p.Category.Name == "LEGO");
        // ToList is used here, but a simple foreach loop also triggers a database query
        foreach (var p in legoProductsQuery.ToList())
        {
            p.Price = 1.1m * p.Price;
        }
    db.SaveChanges();
}
```

**C# MongoDb:**

Updating a single element:
```csharp
collection.UpdateOne(
    filter: x => x.Id == new ObjectId("..."),
    update: Builders<Product>.Update.Set(x => x.Stock, 5));
```
It's evident that after Update, different operators can be written similar to Filter. These operators include: Set, UnSet, SetOnInsert, CurrentDate, Mul, Min, Max, AddToSet (Detailed descriptions can be found in the lecture notes).

Updating all items within category ID 13:
```csharp
productsCollection.UpdateMany(
    filter: p => p.CategoryID == 13,
    update: Builders<Product>.Update.Mul(p => p.Price, 1.1));
```
If you want to search and update a matching element or insert one if there's none, you can use the IsUpsert function as below.
```csharp
var catExpensiveToys = categoriesCollection.FindOneAndUpdate<Category>(
    filter: c => c.Name == "Expensive toys",
    update: Builders<Category>.Update.SetOnInsert(c => c.Name, "Expensive toys"),
    options: new FindOneAndUpdateOptions<Category, Category> { IsUpsert = true, ReturnDocument = ReturnDocument.After });
```

##Summary Table##
| SQL               | C# LINQ                 | C# MongoDb              |
|-------------------|-------------------------|-------------------------|
| **SELECT**        | **Select()**            | **Project()**              |
| **WHERE**         | **Where()**             | **Find()**            |
| **GROUP BY**      | **GroupBy()**           | **Group()**             |
| **ORDER BY**      | **OrderBy()**           | **Sort()**              |
| **JOIN**          | Use navigation properties if possible, else: **Join()**              | **Join()**              |
| **DISTINCT**      | **Distinct()**          | **Distinct()**          |
| **Count()**, **Max()**, **Average()** | **Count()**, **Max()**, **Average()** | Use after **Aggregate()**: **Count()**, **Max()**, **Average()** |
| **DELETE FROM**        |**.Remove()**, and **db.SaveChanges()** to save|**.DeleteOne()**, **.DeleteMany()**|
**UPDATE ... SET**| Modify the data and then **db.SaveChanges()**|**.UpdateOne()**, **.UpdateMany()**|
**INSERT INTO**|**.Add()** and then **db.SaveChanges()**| **.InsertOne()**, **.InsertMany()**
