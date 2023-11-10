# Lekérdezés szintaktitka cheat-sheet

Ebben a cheat-sheet-ben a különböző tanult nyelvek lekérdezési szintaktikája kerül összehasonlításra.

**Fontos:** C# LINQ használatakor ne felejtsük el példányosítani az adatbázis kontextust egy  using block-ban.

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

**Megjegyzés:** Select()-ben névtelen objektumot hoztunk létre, ennek attribútumai szabadon választhatóak, a vetítési művelet elvégzéséhez.
```csharp
//c# jellegű szintaktika
var query = context.Table
            .Where(item => condition)
            .Select(item => new { item.Column1, item.Column2 });

 //SQL szerű szintaktika           
var query = from item in context.Table
            where condition
            select new { item.Column1, item.Column2 };
```
**FONTOS:** Ha szükségünk van a navigation propertykkel hivatkozott entitásokra, akkor a fejlesztő a kódban ezt specifikálja az Include használatával, és a rendszer betölti a kért hivatkozásokat is. Ezek attribútumait **NE** egyesével select segítségével adjuk vissza.
```csharp
var query = products
            .Include(p => p.VAT)
            .Where(p => p.ID==23)
            .SingleOrDefault();
//Elérhető a navigation property összes attribútuma kívülről is, lazy loading nélkül
if(query!=null){
    Console.WriteLine(query.VAT.ID);
}
```
**C# MongoDb:**

Hasonlóképpen használjuk a LINQ-beli selecthez, amennyiben vagy vetíteni akarunk bizonyos attribútumokra.
```csharp
collection.Find(item=>item.Column1 == value)
        .Project(item => new
        {
            Attr1=item.Coloumn1,
            Attr2=item.Coloumn2
        }).ToList();
```
Group() függvényben van lehetőség ehhez hasonló vetítést megadni, lásd lentebb.

## WHERE
**SQL:**

```SQL
SELECT * FROM table_name WHERE column_name="Example";
```
Lekérdezés melyben egy mező NULL értékű:
```SQL
SELECT * FROM table_name WHERE column_name IS NULL;
--Ha ellenkezőjére lennénk kíváncsiak, IS NOT NULL kellene
```
**ISNULL** függvény: Ha van érték a salary oszlopban, akkor annak az értékével dolgozik tovább a lekérdezés, ha nem akkor a második paraméterként kapott értékkel (ebben az esetben 0).
```SQL

SELECT employee_id, employee_name, ISNULL(salary, 0) AS modified_salary
FROM employees;

```

Allekérdezést el lehet nevezni, eredményeire lehet hivatkozni. Látható az alábbi feladatbeli példában (feladat a Microsoft SQL gyakorlatról).

Feladat: Melyik termék kategóriában van a legtöbb termék?
```SQL
select top 1 Name, (select count(*) from Product where Product.CategoryID = c.ID) as cnt
from Category c
order by cnt desc
```

**C# LINQ:**
Hol kisebb az ár mint 1000?
```csharp
//c# jellegű szintaktika
products.Where(p => p.Price < 1000)
//SQL szerű szintaktika
from p in products
where p.Price < 1000
```
**C# MongoDb:**


**Fontos:** MongoDb-nél a Find-ban olyan feltétel lehet csak, ami a dokumentumra vonatkozik, nem lehet csatolt (joinolt) másik gyűjteményre hivatkozni!

**Fontos:** MongoDb-nél mindig oda kell írni egy .Find()-ot a parancsokba (amennyiben nem használunk Aggregate()-et), így gyakori a .Find(_=>true) paraméterezésű Find() parancs.

**Megjegyzés:** A Find() függvény eredménye még nem az eredményhalmaz, hanem csak egy leíró a lekérdezés végrehajtásához. A toList() kéri le a teljes eredményhalmazt listaként. (Amennyiben egyetlen elemet szeretnénk visszaadni nem ezt a függvényt kell használni, lásd lentebb)
```csharp
collection.Find(item=>item.Column == value).ToList();
```

## Egy elemű eredményhalmaz és eredményhalmaz számossága
**SQL:**

Azokban az esetekben, amikor csak egyetlen sort szeretnénk visszakapni lekérdezéseinkből.
```SQL
select top 1 *
from Product
order by Name
```
**C#** (LINQ és MongoDb):

Amennyiben csak az első elemre van szükségünk, vagy tudjuk, hogy csak egy elem lesz, akkor használhatjuk a .First(), .FirstOrDefault(), vagy .Single(), .SingleOrDefault() függvényeket. Fontos, hogy .Single() vagy .SingleOrDefault() függvények használata esetén gondoskodjunk arról, hogy a megelőző lekérdezés valóban egyetlen adatelemet adjon vissza.

Azt, hogy hány elemet ad vissza egy lekérdezés a .Limit(num) függvénnyel lehet megtenni, ahol a num paraméter adja meg az elemek számát.

Lehetőségünk nyílik lapozás használatára is Take és Skip használatával

```csharp
//10-et olvas ki
products.Take(10)

//Skippel 10et majd kiolvas 10-et
products.Skip(10).Take(10)
```

## Group By
**SQL:**

Listázza ki az M betűvel kezdődő termékek nevét és a megrendelt mennyiségeket úgy, hogy azok a termékek is benne legyenek a listában melyekből nem rendeltek meg semmit
```SQL
select p.Name, Sum(oi.Amount)
from Product p
     left outer join OrderItem oi on p.id=oi.ProductID
where p.Name like 'M%'
group by p.Name
```

**C# LINQ:**

Listázzuk ki a termékeket VATID szerint csoportosítva.
```csharp
//c# jellegű szintaktika
products.GroupBy(p => p.VATID)
//SQL szerű szintaktika
from p in products
group p by p.VATID
```
**C# MongoDb:**

```csharp
collection.Aggregate()
                    .Match(Builders<Product>.Filter.AnyEq(x => x.Categories, "Labdák")) // szűrés
                    .Group(x => x.VAT.Percentage, x => x) // csoportosítás
                    .ToList()
```
A Match()-en belül lambda helyett a feltételnek, egy alternatív megadását láthatjuk. Ide több különböző kulcsszót lehet írni és egymásba lehet ágyazni a Filtereket.

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

Példa .Group() második paraméterében lehet vetítéseket megadni hasonlóan a .Project() függvényhez.
```csharp
var r=productsCollection.Aggregate()
    .Group(
        //csoportosítás rész
        p=>p.VAT.Percentage,
        //projekcios rész
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

Kétszintű rendezés
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

Mivel Navigation propertyk segítségével általában elérhetőek az asszociált osztályok, explicit illeszteni csak akkor kell, ha a feladat megoldási logikája megkívánja vagy nincs navigation property az egyik osztályban a másikra.
```csharp
var query = context.Table1
            .Join(context.Table2,
                  item1 => item1.Column,
                  item2 => item2.Column,
                  (item1, item2) => new { item1.Column, item2.Column });
//SQL szerű LINQ szintaktikával:
var query = from item1 in context.Table1
            join item2 in context.Table2
            on item1.Column equals item2.Column
            select new { item1.Column, item2.Column };
```
**Megjegyzés:** Innentől C# LINQ esetén csak a kényelmesebb, query szintatkiai jelölés lesz látható.

**C# MongoDb:**

**MongoDb-ben nem tanultuk szerveri oldari joinra módszert. LINQ segítségével van az után, hogy a teljes illesztendő adathalmazokat kiolvastuk az adatbázisból.** Az adatok lekérdezése után általában a .ToHashSet() és a .Contains() metódusok segítségével, kliens oldali dictionary készítéssel végezzük el az illesztést. (Lásd MongoDb gyakorlat 1. Feladat 5. pontja)

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
--Mennyibe kerül a legdrágább termék?
select max(Price)
from Product
--Melyek a legdrágább termékek?
select *
from Product
where Price=(select max(Price) from Product)
```

**C# LINQ:**

```csharp
products.Count()
//Alábbihoz hasonló képpen Max, Min, Sum
products.Select(p => p.Price).Average()
```

**C# MongoDb:**

Az általános aggregációkhoz készíthetünk pipeline-okat. Egy pipeline képes több eredményt is visszaadni egy dokumentumhalmazról (pl. total, average, maximum vagy minimum értékeket).

Maximum függvény:

**Megjegyzés:** Figyeljük meg a Group-on belüli konstans szerinti csoportosítást, mely azért van hogy a teljes collectionre számítsuk ki az oszlopfüggvény értékét.
```csharp
collection.Aggregate().Group(p=>1,p=>p.Max(x=>x.Stock)).Single();
```

Nézzük ezt a csoportosítás példáján keresztül.
Alábbi lekérdezés kilistázza, hogy mely rendeléshez, mekkora összértékben tartoznak OrderItem rekordok ha azok összértéke meghaladja a 30000-et.
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
Használd a DeleteMany parancsot ha több rekordot szeretnél törölni.

## Insert

**SQL:**

```SQL
insert into Product
values ('aa', 100, 0, 3, 2, null)
--Amikor másik lekérdezés eredményét szeretnénk beilleszteni:
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
    Name = "Alma",
    Price = 890,
    Categories = new[] { "Gyümölcsök" }
};
collection.InsertOne(newProduct);
```

## Update

**SQL:**

Feladat: Emelje meg azon termékek árát 10%-al, melyek nevében szerepel a "Lego" szó!
```SQL
update Product
set Price=1.1*Price
where Name like '%Lego%'
```
Ha olyan értékeket szeretnénk adni a SET parancsban, melyek másik táblákból nyeretők ki, az alábbi képpen lehetséges. A példa az MS SQL gyakorlat anyagából van.
Feladat: A 9-es azonosítójú számú order státusz állapotát másoljuk be minden olyan OrderItem-be, mely hozzá tartozik.
```SQL
UPDATE OrderItem
SET StatusID = o.StatusID
FROM OrderItem oi
INNER JOIN Order o ON o.Id = oi.OrderID
WHERE o.ID=9;
```

**C# LINQ:**

(Spoiler az Entity Framework gyakorlat 3. feladatának megoldására.)

Feladat: Írj olyan LINQ-ra épülő C# kódot, amely az "LEGO" kategóriás termékek árát megemeli 10 százalékkal!
```csharp
using (var db = new AdatvezDbContext())
{
    var legoProductsQuery = db.Products.Where(p => p.Category.Name == "LEGO");
        // A ToList, de simán a foreach is adatbázis kérést indukál
        foreach (var p in legoProductsQuery.ToList())
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
Jól látható, hogy az Update után a Filterhez hasonlóan több különböző operátor írható. Ilyenek a: Set, UnSet, SetOnInsert, CurrentDate, Mul, Min, Max, AddToSet (Teljes, részletes leírás a jegyzetben)

Minden 13-as kategória Id-jú kategória beli elem frissítése:
```csharp
productsCollection.UpdateMany(
    filter: p => p.CategoryID == 13,
    update: Builders<Product>.Update.Mul(p => p.Price, 1.1));
```
Amennyiben úgy akarunk keresni, hogy vagy updatelunk egy bizonyos filterre illeszkedő elemet vagy ha nincs ilyen beillesztjük, használhatjuk az alábbi IsUpsert függvényt.
```csharp
var catExpensiveToys = categoriesCollection.FindOneAndUpdate<Category>(
    filter: c => c.Name == "Expensive toys",
    update: Builders<Category>.Update.SetOnInsert(c => c.Name, "Expensive toys"),
    options: new FindOneAndUpdateOptions<Category, Category> { IsUpsert = true, ReturnDocument = ReturnDocument.After });
```

##Összefoglaló táblázat##
##Summary Table##
| SQL               | C# LINQ                 | C# MongoDb              |
|-------------------|-------------------------|-------------------------|
| **SELECT**        | **Select()**            | **Project()**              |
| **WHERE**         | **Where()**             | **Find()**            |
| **GROUP BY**      | **GroupBy()**           | **Group()**             |
| **ORDER BY**      | **OrderBy()**           | **Sort()**              |
| **JOIN**          | Használj navigációs propertyket, ha lehetséges, különben: **Join()**              | **Join()**              |
| **DISTINCT**      | **Distinct()**          | **Distinct()**          |
| **Count()**, **Max()**, **Average()** | **Count()**, **Max()**, **Average()** | Először **.Aggregate()**, majd: **Count()**, **Max()**, **Average()** |
| **DELETE FROM**        |**.Remove()**, és **db.SaveChanges()** mentéshez|**.DeleteOne()**, **.DeleteMany()**|
**UPDATE ... SET**| Módosítsd az adatokat, majd **db.SaveChanges()**|**.UpdateOne()**, **.UpdateMany()**|
**INSERT INTO**|**.Add()** és aztán **db.SaveChanges()**| **.InsertOne()**, **.InsertMany()**
