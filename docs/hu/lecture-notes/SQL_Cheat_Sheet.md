# Lekérdezés szintaktitka cheat-sheet

Ebben a cheat-sheet-ben a különböző tanult nyelvek lekérdezési szintaktikája kerül összehasonlításra.

!!! warning Fontos
     C# LINQ használatakor ne felejtsük el példányosítani az adatbázis kontextust egy `using` block-ban.

    ```csharp
    using (var db = new AdatvezDbContext())
    {
        //lekérdezések, stb...
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

```csharp
// fluent szintaktika
var query = context.Table
    .Where(item => condition)
    .Select(item => new { item.Column1, item.Column2 });

// Query szintaktika           
var query = from item in context.Table
            where condition
            select new { item.Column1, item.Column2 };
```

!!! tip "Megjegyzés"
    `Select()`-ben névtelen objektumot hoztunk létre, ennek property-jei szabadon választhatóak, a vetítési művelet elvégzéséhez.

!!! warning Fontos
    Ha szükségünk van a navigation propertykkel hivatkozott entitásokra, akkor a fejlesztő a kódban ezt specifikálja az `Include` használatával, és a rendszer betölti a kért hivatkozásokat is.

```csharp
var prod = db.Products
    .Include(p => p.VAT)
    .Where(p => p.ID == 23)
    .SingleOrDefault();
if(prod is not null)
{
    Console.WriteLine(prod.VAT.ID);
}
```

**C# MongoDb:**

MongoDb-beli `Project()`-et hasonlóképpen használjuk a LINQ-beli `Select()`-hez, amennyiben vagy vetíteni akarunk bizonyos attribútumokra.

```csharp
collection
    .Find(item=>item.Column1 == value)
    .Project(item => new
    {
        Attr1 = item.Coloumn1,
        Attr2 = item.Coloumn2
    })
    .ToList();
```

## WHERE

**SQL:**

```SQL
SELECT *
FROM table_name
WHERE column_name = "Example";
```

Lekérdezés melyben egy mező `NULL` értékű:

```SQL
SELECT *
FROM table_name
WHERE column_name IS NULL;
-- Ha ellenkezőjére lennénk kíváncsiak, IS NOT NULL kellene
```

`ISNULL` függvény: Ha van érték a `salary` oszlopban, akkor annak az értékével dolgozik tovább a lekérdezés, ha nem akkor a második paraméterként kapott értékkel (ebben az esetben 0).

```SQL
SELECT employee_id, employee_name, ISNULL(salary, 0) AS modified_salary
FROM employees;
```

Allekérdezést el lehet nevezni, eredményeire lehet hivatkozni. Látható az alábbi feladatbeli példában (feladat a Microsoft SQL gyakorlatról).

**Feladat:** Melyik termék kategóriában van a legtöbb termék?

```SQL
SELECT TOP 1 Name, (SELECT COUNT(*) FROM Product WHERE Product.CategoryID = c.ID) AS cnt
FROM Category c
ORDER BY cnt DESC
```

**C# LINQ:**

**Feladat:** Hol kisebb az ár mint 1000?

```csharp
//Fluent szintaktika
db.Products.Where(p => p.Price < 1000)

//Query szintaktika
from p in db.Products
where p.Price < 1000
```

**C# MongoDb:**

!!! warning Fontos
     MongoDb-nél a `Find`-ban olyan feltétel lehet csak, ami a dokumentumra vonatkozik, nem lehet csatolt (joinolt) másik gyűjteményre hivatkozni!

!!! warning Fontos
    MongoDb-nél mindig oda kell írni egy `.Find()`-ot a parancsokba (amennyiben nem használunk `Aggregate()`-et), így gyakori a `.Find(_=>true)` paraméterezésű `Find()` parancs.

!!! note Megjegyzés
    A `Find()` függvény eredménye még nem az eredményhalmaz, hanem csak egy leíró a lekérdezés végrehajtásához.
    A `ToList()` kéri le a teljes eredményhalmazt listaként. (Amennyiben egyetlen elemet szeretnénk visszaadni nem ezt a függvényt kell használni, lásd lentebb)

```csharp
collection.Find(item => item.Column == value).ToList();
```

## Egy elemű eredményhalmaz és eredményhalmaz számossága

**SQL:**

Azokban az esetekben, amikor csak egyetlen sort szeretnénk visszakapni lekérdezéseinkből.

```SQL
SELECT TOP 1 *
FROM Product
ORDER BY Name
```

**C# (LINQ és MongoDb):**

Amennyiben csak az első elemre van szükségünk, vagy tudjuk, hogy csak egy elem lesz, akkor használhatjuk a `.First()`, `.FirstOrDefault()`, vagy `.Single()`, `.SingleOrDefault()` függvényeket. Fontos, hogy `.Single()` vagy `.SingleOrDefault()` függvények használata esetén gondoskodjunk arról, hogy a megelőző lekérdezés valóban egyetlen adatelemet adjon vissza. (Különben kivételt fog dobni)

Lehetőségünk nyílik lapozás használatára is `.Skip()` használatával

**C# LINQ:**

`Take()` használatával korlátozhatjuk hány eredményt ad vissza a lekérdezés.

```csharp
// 10-et olvas ki
db.Products.Take(10)

// Skippel 10-et majd kiolvas 10-et
db.Products.Skip(10).Take(10)
```

**C# MongoDb:**

`Limit()` használatával korlátozhatjuk hány eredményt ad vissza a lekérdezés.

```csharp
//10-et olvas ki
collection.Find(...).Limit(10);
//Skippel 10et majd kiolvas 10-et
collection.Find(...).Skip(10).Limit(10);
```

## Group By

**SQL:**

Listázza ki az M betűvel kezdődő termékek nevét és a megrendelt mennyiségeket úgy, hogy azok a termékek is benne legyenek a listában melyekből nem rendeltek meg semmit

```SQL
SELECT p.Name, SUM(oi.Amount)
FROM Product p
     LEFT OUTER JOIN OrderItem oi ON p.id = oi.ProductID
WHERE p.Name LIKE 'M%'
GROUP BY p.Name
```

**C# LINQ:**

Listázzuk ki a termékeket `VATID` szerint csoportosítva.

```csharp
// Fluent szintaktika
db.Products.GroupBy(p => p.VATID)

// Query szintaktika
from p in db.Products
group p by p.VATID
```

**C# MongoDb:**

```csharp
collection.Aggregate()
    .Match(Builders<Product>.Filter.AnyEq(x => x.Categories, "Labdák")) // szűrés
    .Group(x => x.VAT.Percentage, x => x) // csoportosítás
    .ToList()
```

A `Match()`-en belül lambda helyett a feltételnek, egy alternatív megadását láthatjuk. Ide több különböző kulcsszót lehet írni és egymásba lehet ágyazni a Filtereket.

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

Példa `.Group()` második paraméterében lehet vetítéseket megadni hasonlóan a `.Project()` függvényhez.

```csharp
var r = productsCollection
    .Aggregate()
    .Group(
        // csoportosítás rész
        p => p.VAT.Percentage,
        // projekcios rész
        p => new
        {
            VatPercentage = p.Key
            SumPrice = p.Sum(s => s.Price)
        })
    .ToList();
```

## Order By

**SQL:**

```SQL
ORDER BY column1 ASC, column2 DESC;
```

**C# LINQ:**

Kétszintű rendezés

```csharp
items.OrderBy(item => item.Column1)
     .ThenByDescending(item => item.Column2)
```

**C# MongoDb:**

```csharp
itemcollection.Sort(
    Builders<CollectionItem>.Sort.Ascending(item => item.Coloumn))
```

## JOIN

**SQL:**

```SQL
SELECT table1.column, table2.column
FROM table1
JOIN table2 ON table1.column = table2.column;
```

**C# LINQ:**

Mivel navigation propertyk segítségével általában elérhetőek az asszociált osztályok, explicit illeszteni csak akkor kell, ha a feladat megoldási logikája megkívánja vagy nincs navigation property az egyik osztályban a másikra.

```csharp
// Fluent szintaktika
var query = context
    .Table1
    .Join(context.Table2,
        item1 => item1.Column,
        item2 => item2.Column,
        (item1, item2) => new { item1.Column, item2.Column });

// Query szintaktika
var query = from item1 in context.Table1
            join item2 in context.Table2
            on item1.Column equals item2.Column
            select new { item1.Column, item2.Column };
```

**C# MongoDb:**

**MongoDb-ben nem tanultuk szerveri oldari joinra módszert. LINQ segítségével van az után, hogy a teljes illesztendő adathalmazokat kiolvastuk az adatbázisból.** Az adatok lekérdezése után általában a `.ToHashSet()` és a `.Contains()` metódusok segítségével, kliens oldali dictionary készítéssel végezzük el az illesztést. (Lásd MongoDb gyakorlat 1. Feladat 5. pontja)

## Distinct

**SQL:**

```SQL
SELECT DISTINCT p.Name
FROM Product p
```

**C# LINQ:**

**Feladat:** Minden különböző termék név

```csharp
db.Products.Select(p => p.Name).Distinct();
```

**C# MongoDb:**
**Feladat:** Minden `CategoryID`, melyhez tartozik termék, melynek ára nagyobb mint 3000.

```csharp
var xd = productsCollection
    .Distinct(p => p.CategoryID, p => p.Price > 3000)
    .ToList();
```

## Oszlopfüggvények

**SQL:**

**Feladat:** Mennyibe kerül a legdrágább termék?

```SQL
SELECT MAX(Price)
FROM Product
```

**Feladat:** Melyek a legdrágább termékek?

```SQL
SELECT *
FROM Product
WHERE Price = (SELECT MAX(Price) FROM Product)
```

**C# LINQ:**

```csharp
db.Products.Count()

// Alábbihoz hasonló képpen Max, Min, Sum
db.Products.Select(p => p.Price).Average()
```

**C# MongoDb:**

Az általános aggregációkhoz készíthetünk pipeline-okat. Egy pipeline képes több eredményt is visszaadni egy dokumentumhalmazról (pl. total, average, maximum vagy minimum értékeket).

Maximum függvény:

!!! tip Megjegyzés
    Figyeljük meg a `Group`-on belüli konstans szerinti csoportosítást, mely azért van hogy a teljes collectionre számítsuk ki az oszlopfüggvény értékét.

```csharp
collection
    .Aggregate()
    .Group(p => 1, p => p.Max(x => x.Stock))
    .Single();
```

Nézzük ezt a csoportosítás példáján keresztül.
Alábbi lekérdezés kilistázza, hogy mely rendeléshez, mekkora összértékben tartoznak `OrderItem` rekordok ha azok összértéke meghaladja a 30000-et.

```csharp
var q = ordersCollection
    .Aggregate()
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
DELETE
FROM Product
WHERE ID = 23
```

**C# LINQ:**

```csharp
using (var db = new AdatvezDbContext())
{
    var deleteThis = db.Products
        .Select(p => p.ID == 23)
        .SingleOrDefault();
    if(deleteThis is not null)
    {
        db.Products.Remove(deleteThis);
        db.SaveChanges();
    }
}
```

**C# MongoDb:**

```csharp
var deleteResult = collection.DeleteOne(x => x.Id == new ObjectId("..."));
```

Használd a `DeleteMany` parancsot ha több rekordot szeretnél törölni.

## Insert

**SQL:**

```SQL
INSERT INTO Product
VALUES ('aa', 100, 0, 3, 2, null)
```

Amikor másik lekérdezés eredményét szeretnénk beilleszteni:

```SQL
INSERT INTO Product (Name, Price)
SELECT Name, Price
FROM InvoiceItem
WHERE Amount>2
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
    Name = "Alma",
    Price = 890,
    Categories = new[] { "Gyümölcsök" }
};
collection.InsertOne(newProduct);
```

## Update

**SQL:**

**Feladat:** Emelje meg azon termékek árát 10%-al, melyek nevében szerepel a "Lego" szó!

```SQL
UPDATE Product
SET Price=1.1 * Price
WHERE Name LIKE '%Lego%'
```

Ha olyan értékeket szeretnénk adni a `SET` parancsban, melyek másik táblákból nyeretők ki, az alábbi képpen lehetséges. A példa az MS SQL gyakorlat anyagából van.

**Feladat:** A 9-es azonosítójú számú megrendelés státusz állapotát másoljuk be minden olyan `OrderItem`-be, mely hozzá tartozik.

```SQL
UPDATE OrderItem
SET StatusID = o.StatusID
FROM OrderItem oi
INNER JOIN Order o ON o.Id = oi.OrderID
WHERE o.ID = 9;
```

**C# LINQ:**

!!! warning Spoiler
    Az Entity Framework gyakorlat 3. feladatának megoldása

**Feladat:** Írj olyan LINQ-ra épülő C# kódot, amely az "LEGO" kategóriás termékek árát megemeli 10 százalékkal!

```csharp
using (var db = new AdatvezDbContext())
{
    var legoProductsQuery = db.Products
        .Where(p => p.Category.Name == "LEGO")
        .ToList();
    foreach (var p in legoProductsQuery)
    {
        p.Price = 1.1m * p.Price;
    }
    db.SaveChanges();
}
```

**C# MongoDb:**

Egy elem frissítése:

```csharp
collection.UpdateOne(
    filter: x => x.Id == new ObjectId("..."),
    update: Builders<Product>.Update.Set(x => x.Stock, 5));
```

Jól látható, hogy az `Update` után a Filterhez hasonlóan több különböző operátor írható. Ilyenek a: `Set`, `UnSet`, `SetOnInsert`, `CurrentDate`, `Mul`, `Min`, `Max`, `AddToSet` (Teljes, részletes leírás a jegyzetben)

Minden 13-as kategória `Id`-jú kategória beli elem frissítése:

```csharp
productsCollection.UpdateMany(
    filter: p => p.CategoryID == 13,
    update: Builders<Product>.Update.Mul(p => p.Price, 1.1));
```

Amennyiben úgy akarunk keresni, hogy vagy updatelunk egy bizonyos filterre illeszkedő elemet vagy ha nincs ilyen beillesztjük, használhatjuk az alábbi `IsUpsert` függvényt.

```csharp
var catExpensiveToys = categoriesCollection.FindOneAndUpdate<Category>(
    filter: c => c.Name == "Expensive toys",
    update: Builders<Category>.Update.SetOnInsert(c => c.Name, "Expensive toys"),
    options: new FindOneAndUpdateOptions<Category, Category> { IsUpsert = true, ReturnDocument = ReturnDocument.After });
```

## Összefoglaló táblázat

| SQL               | C# LINQ                 | C# MongoDb              |
|-------------------|-------------------------|-------------------------|
| `SELECT`        | `Select()`            | `Project()`              |
| `WHERE`         | `Where()`             | `Find()`            |
| `GROUP BY`      | `GroupBy()`           | `Group()`             |
| `ORDER BY`      | `OrderBy()`           | `Sort()`              |
| `JOIN`          | Használj navigációs propertyket, ha lehetséges, különben: `Join()`              | `Join()`              |
| `DISTINCT`      | `Distinct()`          | `Distinct()`          |
| `Count()`, `Max()`, `Average()` | `Count()`, `Max()`, `Average()` | Először `.Aggregate()`, majd: `Count()`, `Max()`, `Average()` |
| `DELETE FROM`        |`.Remove()`, és `db.SaveChanges()` mentéshez|`.DeleteOne()`, `.DeleteMany()`|
`UPDATE ... SET`| Módosítsd az adatokat, majd `db.SaveChanges()`|`.UpdateOne()`, `.UpdateMany()`|
`INSERT INTO`|`.Add()` és aztán `db.SaveChanges()`| `.InsertOne()`, `.InsertMany()`
