# Kapcsolatok definiálása Entity Framework-ben

Relációs adatbázisokban entitásokat és az őket összekötő kapcsolatokat tárolunk. Így egyes egyedeken keresztül elérhetünk más, hozzájuk valamilyen módon kapcsolódó egyedeket is. A relációs modellben ezt a táblák összekapcsolásával tesszük, amelyet SQL nyelven `join`-nak hívunk. Az Entity Framework, mint ORM keretrendszer, a kapcsolatok kezelésére is ad kész és kényelmes megoldást számunkra.

## Kapcsolatok definiálása

!!! note "Konvenciók kapcsolatok leképezésére"
    Az Entity Framework rendelkezik alapvető [konvenciókkal](https://docs.microsoft.com/en-us/ef/core/modeling/relationships#conventions), amelyekkel a rendszer automatikusan, explicit konfiguráció nélkül képes kapcsolatokat leképezni. Az alábbiakban erre nem térünk ki, helyette explicit definiáljuk a kapcsolatokat. A manuális felkonfigurálással nem mellesleg láthatóbbá válnak a szándékaink is.

Nézzük is tehát a példa objektumainkat, majd vizsgáljuk meg a rajtuk megjelenő kapcsolatot:

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

A konfigurálást a `DBContext` leszármazott, `OnModelCreating` függvényében tehetjük meg, ahol egy entitásra a következő függvényeket használhatjuk:

```csharp
modelBuilder.Entity<Példa>
    .HasOne()/.HasMany()
    .WithOne()/.WithMany()
```

A példánkban egy egy-több kapcsolatot láthatunk, amit a következő módon írhatunk le:

```csharp
modelBuilder.Entity<Product>()
            .HasOne(d => d.VAT)
            .WithMany(p => p.Product)
            .HasForeignKey(d => d.VatId);
```

Így a két entitás között létrejövő kapcsolatot a `Product` tábla `VAT`-ra mutató külső kulcsa adja (ami természetesen az adatbázisban is megjelenik egy oszlop formájában), de emellett kód szinten a konkrét `VAT` referencia is megjelenik a `Product` osztályban, ahogy a `VAT` objektumhoz tartozó `Product` lista is elérhetővé válik számunkra egy C# property formájában. Ezeket hívjuk _navigation propertynek_.

## Explicit join

A `DBContext`-ünk az entitások lekérdezéséhez `DBSet`-eket kínál, amiken LINQ műveleteket hajthatunk végre. Az egyik ilyen művelet a `join` összekapcsolás. A két `DBSet` összekapcsolása a külső kulcson keresztül történhet. Az alábbi LINQ kifejezés az SQL megfelelőjéhez hasonlóan deklaratívan leírja, hogy mit szeretnénk megkapni.

```csharp
var query = 
    from p in dbContext.Product
    join v in dbContext.Vat on p.VatId equals v.Id
    where p.Name.Contains("teszt")
    select v.Percentage;

// Megmutatja a generált SQL utasítást
Console.WriteLine(query.ToQueryString());    
```

A háttérben az alábbi lesz a generált SQL utasítás:

```sql
SELECT [v].[Percentage]
FROM [Product] AS [p]
INNER JOIN [VAT] AS [v] ON [p].[VatId] = [v].[ID]
WHERE [p].[Name] LIKE N'%teszt%'
```

Az ilyen módú kapcsolásra azonban ritkán van szükségünk - sőt, kerülendő is, amikor rendelkezésünkre állnak a _navigation propertyk_.

## Navigation property

Mivel a `DbContext`-ben pontosan felkonfiguráltuk, hogy a `Product` és `VAT` entitások között milyen kapcsolat van, használhatjuk a `Product` osztályban található `VAT` property-t: ezt nevezzük _navigation propertynek_. A _navigation property_ "mögötti" kapcsolatot az EF automatikusan kezeli, és végrehajtja az (általunk explicit le nem írt) összekapcsolást. Így az előző lekérdezésünk a következőre egyszerűsödik:

```csharp
var query =
    from p in dbContext.Product
    where p.Name.Contains("teszt")
    select p.VAT.Percentage;

// Megmutatja a generált SQL utasítást
Console.WriteLine(query.ToQueryString());
```

Alább láthatjuk a generált lekérdezés, ami csak a join típusában tér el a korábbitól, de egyébként érdemben ugyan arra a megoldásra jutunk.

```sql
SELECT [v].[Percentage]
FROM [Product] AS [p]
LEFT JOIN [VAT] AS [v] ON [p].[VatId] = [v].[ID]
WHERE [p].[Name] LIKE N'%teszt%'
```

!!! important "Használjuk a _navigation property_-ket"
    EF-ben mindig használjuk a _navigation propertyket_ ott, ahol rendelkezésre állnak. Kerüljük az explicit `join` végrehajtását.

### Include

Az előző példákban csak egy-egy skalár eredményt kérdeztünk le. De mi történik a _navigation property_-kkel, amikor egy egész entitást kérdezünk le? Például:

```csharp
var prod = dbContext.Product.Where(p => p.Name.Contains("teszt")).First();

Console.WriteLine(prod.Name); // ez működik, kiírja a nevet
Console.WriteLine(prod.VAT.Percentage); // a navigation property elérése
```

A példában az utolsó sorban futási idejű hibát kapnánk. Miért is? Ugyan a _navigation property_-t felkonfiguráltuk, de alapesetben az EF nem tölti be a hivatkozott entitásokat. Tehát a lekérdezésekben dolgozhatunk velük (ahogy korábban tettük a hivatkozott `v.Percentage` lekérdezésével), viszont amennyiben egy `Product` entitást kérünk a rendszertől, abban nem szerepel a hivatkozott `VAT` entitás. A rendszer képes lenne a hivatkozott rekordo(ka)t is letölteni, de nem teszi. A fejlesztő feladata eldönteni, hogy szüksége van-e a hivatkozott entitásokra. Gondoljunk csak bele, ha minden hivatkozott entitást automatikusan letöltene a rendszer (tranzitívan is), akkor egyetlen rekord elérésével akár százakat, ezreket kellene megkeresnie az adatbázisnak. Ez a legtöbb esetben felesleges.

Ha mégis szükségünk van a hivatkozott entitásokra, akkor a fejlesztő a kódban ezt specifikálja az `Include` használatával, és a rendszer betölti a kért hivatkozásokat is.

```csharp
var query =
    from p in dbContext.Products.Include(p => p.VAT)
    where p.Name.Contains("teszt")
    select p;

// vagy ebben az esetben talán egy jobban látható de teljesen ekvivalens megoldás:
// var query = products
//               .Include(p => p.VAT)
//               .Where(p => p.Name.Contains("teszt"));

Console.WriteLine(query.ToQueryString());
```

Ha megnézzük a generált SQL utasítást, látható benne a `join` és az is, hogy a `select` minden szükséges adatot lekérdez.

```sql
SELECT [p].[Id], [p].[CategoryId], [p].[Description], [p].[Name], [p].[Price], [p].[Stock], [p].[VatId], [v].[ID], [v].[Percentage]
FROM [Product] AS [p]
LEFT JOIN [VAT] AS [v] ON [p].[VatId] = [v].[ID]
WHERE [p].[Name] LIKE N'%teszt%'
```

!!! note "Hivatkozott entitások automatikus _lazy_ betöltése"
    Entity Frameworkben lehetőség van a [_lazy loading_](https://docs.microsoft.com/en-us/ef/core/querying/related-data/lazy) bekapcsolására, aminek hatására a _navigation property_-ken keresztül hivatkozott entitásokat a rendszer _lazy_ módon (azaz: amikor szükség van rá) betölti külön `Include` nélkül is. Ez a megoldás ugyan kényelmes a fejlesztő szempontjából, de ára van: a betöltés akkor történik meg, amikor szükség lesz rá (amikor a property-t eléri a kód), ami tipikusan több külön adatbázis lekérdezést fog eredményezni. Az `Include` megoldásban fentebb látható, hogy egyetlen lekérdezés betölti a `Product` és `VAT` adatokat is. Ha ezt _lazy loading_-gal csinálnánk, akkor lenne egy lekérdezés a `Product` adataihoz, majd időben később még egy a hivatkozott `VAT` lekérdezéséhez. Ez tehát teljesítményben jelentősen rosszabb.
