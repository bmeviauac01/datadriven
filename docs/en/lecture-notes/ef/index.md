# Defining relationships in Entity Framework

We store both entities and the relationships that connect them in relational databases. This allows us to query related entities through joining tables, expressed with the `join` SQL command. Entity Framework, which is an ORM framework, provides us with built-in support for conveniently managing these relationships.

## Defining relationships

!!! note "Convention-based mapping"
    Entity Framework has [conventions](https://docs.microsoft.com/en-us/ef/core/modeling/relationships#conventions) that enable mapping relationships automatically without explicit configuration. We will not rely on this feature here; instead will define the relationships explicitly.

Let us look at our example classes and the relationship among them:

```csharp
public class Product
{
    public int ID;
    public string Name;
    public int Price;
    public int VATID;

    public VAT VAT { get; set; }
}

public class VAT
{
    public int ID;
    public int Percentage;
    public ICollection<Product> Product { get; set; }
}

```

We can set up the configuration in the `OnModelCreating` function inherited from the `DBContext` base class. We can use the following functions for configuring an entity:

```csharp
modelBuilder.Entity<Example>
    .HasOne()/.HasMany()
    .WithOne()/.WithMany()
```

In the example above, we can see a one-to-many connections, which can be described as follows:

```csharp
modelBuilder.Entity<Product>()
            .HasOne(d => d.VAT)
            .WithMany(p => p.Product)
            .HasForeignKey(d => d.VatId);
```

The connection between the two entities is provided by the foreign key of the `Product` table pointing to `VAT` table (which foreign key, naturally, also appears in the database as a column). In the C# code, a `VAT` object reference appears in class `Product`. Similarly, `VAT` objects have a list of connected `Product` instances. These C# properties are called _navigation properties_.

## Explicit joining

The `DBContext` offers the tables as `DBSets`, on which we can perform LINQ operations. One such operation is the `join` function. Two `DBSets` can be joined via the appropriate foreign key. Similar to it's SQL equivalent, the following LINQ expression declaratively describes what we want to get.

```csharp
var query = 
    from p in dbContext.Product
    join v in dbContext.Vat on p.VatId equals v.Id
    where p.Name.Contains("test")
    select v.Percentage;

// Displays the generated SQL query
Console.WriteLine(query.ToQueryString());    
```

The generated SQL query will look similar to this:

```sql
SELECT [v].[Percentage]
FROM [Product] AS [p]
INNER JOIN [VAT] AS [v] ON [p].[VatId] = [v].[ID]
WHERE [p].[Name] LIKE N'%test%'
```

We rarely need to use explicit joins. As a matter of fact, we should avoid using them when _navigation properties_ are available.

## Navigation property

Since we have configured the relationship between the `Product` and `VAT` EF entities in our `DbContext`, we can use the `VAT` property in the `Product` class: this is the _navigation property_. The joining "behind" the _navigation property_ is handled automatically by EF without us having to define it in the query. This simplifies our previous query to:

```csharp
var query =
    from p in dbContext.Product
    where p.Name.Contains("test")
    select p.VAT.Percentage;

// Displays the generated SQL query
Console.WriteLine(query.ToQueryString());
```

Below we can see the generated SQL query, which differs from the previous one only in the type of join, but otherwise, we get to the same solution.

```sql
SELECT [v].[Percentage]
FROM [Product] AS [p]
LEFT JOIN [VAT] AS [v] ON [p].[VatId] = [v].[ID]
WHERE [p].[Name] LIKE N'%test%'
```

!!! important "Prefer _navigation properties_"
    In EF, we should always strive to use the _navigation properties_ when possible. We should avoid performing explicit joins.

### Include

In the previous example, only one scalar result was queried. But what happens to the _navigation properties_ when we query an entire entity? For example:

```csharp
var prod = dbContext.Product.Where(p => p.Name.Contains("test")).First();

Console.WriteLine(prod.Name); // this works, it will print the name
Console.WriteLine(prod.VAT.Percentage); // accessing the referenced entity via the navigation property
```

In this example, we would get a runtime error in the last line. Why is that? Despite the _navigation property_ being configured, EF does not load referenced entities by default. We can work with them in queries (as we wrote `p.VAT.Percentage` in a previous query), but if we query a `Product` entity, it does not include the referenced `VAT` entity. The referenced record(s) could be fetched. But it is up to the developer to decide if they really need them. Just consider, if all the referenced entities were fetched automatically (even transitively), the database would have to look up hundreds or thousands or records to get a single entity and all of it's referenced data via navigation properties. This is unnecessary in most cases.

If we really need the referenced entities, then we need to specify this in the code using `Include` as follows:

```csharp
var query =
    from p in dbContext.Products.Include(p => p.VAT)
    where p.Name.Contains("test")
    select p;

// or an alternative syntax for the same:
// var query = products
//               .Include(p => p.VAT)
//               .Where(p => p.Name.Contains("test"));

Console.WriteLine(query.ToQueryString());
```

If we look at the generated SQL statement, it shows both the appropriate `join` and the required data appearing within the `select` statement.

```sql
SELECT [p].[Id], [p].[CategoryId], [p].[Description], [p].[Name], [p].[Price], [p].[Stock], [p].[VatId], [v].[ID], [v].[Percentage]
FROM [Product] AS [p]
LEFT JOIN [VAT] AS [v] ON [p].[VatId] = [v].[ID]
WHERE [p].[Name] LIKE N'%test%'
```

!!! note "Automatic _lazy loading_ of referenced entities"
    In Entity Framework, it is possible to turn on [_lazy loading_](https://docs.microsoft.com/en-us/ef/core/querying/related-data/lazy), which causes entities to be loaded through _navigation properties_ on demand. The loading is performed in a _lazy_ way (that is, only when needed) without an explicit `Include`. While this solution is convenient for the developer, it comes at a price: loading data when needed (when the code reaches a statement referencing the property) will typically result in several separate database queries. In the `Include` solution, you can see above that a single query loads both the `Product` and `VAT` data. If we used _lazy loading_, there would be a query for the `Product` data and another one for the referenced `VAT` properties at a later time. Thus, lazy loading is usually worse in terms of performance.
