# Kapcsolatok definiálása EF Core-ban, Navigation property

SQL adatbázisokban entitásokat és az őket összekötő kapcsolatokat tárolunk. Így egyes egyedeken keresztül elérhetünk más hozzájuk valamilyen módon kapcsolodó egyedeket is.
Alapvetően ezt a táblák összekapcsolásával tesszük amelyet SQL nyelven join-nak hívunk. 
Entitások között több féle kapcsolat lehet, az egyik ilyen jellemző a kapcsolat kardinalitása. Ez lehet egy-egy, egy-több, és több-több kapcsolat.

## EF Core

Az adatbázist a kódunkal összeköttetni Object Relation Mapping (ORM)-el lehet, amelyekre más jegyzetek bővebben kitérnek.
Entity Framework Core az egyik legújabb ORM eszköz. A mappelést Code First modellel oldja meg, ami annyit tesz, hogy az entitásokat kódban definiáljuk, és onnan szinkronizálhatjuk (migráljuk) az adatbázisba.

### Kapcsolatok definiálása

!!! note "" EF Coreban-ban vannak alapvető konvenciók amelyekket a rendszer le tud kezelni és autómatikusan létrehoz, de erre most nem térünk ki, hiszen megérteni szeretnénk a kapcsolatokat és a manuális felkonfigurálással sokkal láthatóbbá válnak azok.

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

A konfigurálást a DBContext, OnModelCreating függvényében telhetjük meg ahol egy entitásra a következő függvényeket definiálhatjuk:

```csharp
modelBuilder.Entity<Példa>
	.HasOne()/.HasMany()
	.WithOne()/.WithMany()
```

A példánkban egy egy-több kacsolattot láthatunk amit a következő módon írhatunk le:

```csharp
modelBuilder.Entity<Product>()
            .HasOne(d => d.VAT)
            .WithMany(p => p.Product)
            .HasForeignKey(d => d.VatId);
```
Így a két entitás között létrejövő navigációt a Product VAT-ra mutató külső kulcsa adja, ami az adatbázisban is megjelenik, de kód szinten a konkét VAT objektum is megjelenik a Product objektumon, ahogy a VAt objektum-hoz tartozó Product lista is elérhetővé válik számunkra.

Ezzel el is érkeztünk a köveztkező témánkhoz:

### Navigation property

A DBContext-ünk az Entitások lekérdezéséhez DBSet-eket (IQueryable) kinál amiken végrehajthatünk LINQ- műveleteket. A LINQ részletes ismertetését egy másik fejezet tartalmazza, így itt arra építhetünk. Nézzük meg hogy, eddig hogyan kapcsoltunk össze két relációt és, hogy milyen más lehetőségünk van még erre:

#### Join:

A két DBSet összekapcsolása joinnal a külső kulcson keresztül történik. A linq kifejezés az sql-hez hasonlóan deklerativan leírja, hogy mint szeretnénk megkapni és még kiérékelés előtt el is kérhetjük tőle, hogy milyen query fog lefutni az adatbázisban.

```csharp
var products = _dbContext.Product;
var vat = _dbContext.Vat;

var query = 
	from p in products
	join v in vat on p.VatId equals v.Id
	where p.Name.Contains("teszt")
	select v.Percentage;

Console.WriteLine(query.ToQueryString());	
```

A kiírt eredmény: 

```sql
SELECT [v].[Percentage]
FROM [Product] AS [p]
INNER JOIN [VAT] AS [v] ON [p].[VatId] = [v].[ID]
WHERE [p].[Name] LIKE N'%teszt%'
```

#### Navigáció:

Miután az első részben pontosan felkonfiguráltuk, hogy a Product és VAT entitások között milyen kapcsolat van használhatjuk a Product osztályban felvet VAT property-t amit ezesetben navigation propertynek is nevezhetünk. Így az előző lekérdezésünk a köveztekőre egyszerűsödik:

```csharp
var query2 =
	from p in products
	where p.Name.Contains("teszt")
	select p.VAT.Percentage;

Console.WriteLine(query2.ToQueryString());
```

Láthatjuk, hogy a kiírt eredmény csak a join típusában tér el, de egyebként ugyan arra a megoldásra jutunk.

```sql
SELECT [v].[Percentage]
FROM [Product] AS [p]
LEFT JOIN [VAT] AS [v] ON [p].[VatId] = [v].[ID]
WHERE [p].[Name] LIKE N'%teszt%'
```

#### Include

A jól felvett Navigation property konkrét objektum ként is megjelenhet a tartalmazó objektumban, ehhez egy egyszerű include utasítást kell meghívnunk:

```csharp
var query3 =
	from p in products.Include(p => p.VAT)
	where p.Name.Contains("teszt")
	select p;

// vagy ebben az esetben talán egy jobban látható de teljesen ekvivalens megoldás:

var query3 = products
                .Include(p => p.VAT)
                .Where(p => p.Name.Contains("teszt"));

Console.WriteLine(query3.ToQueryString());
```

```sql
SELECT [p].[Id], [p].[CategoryId], [p].[Description], [p].[Name], [p].[Price], [p].[Stock], [p].[VatId], [v].[ID], [v].[Percentage]
FROM [Product] AS [p]
LEFT JOIN [VAT] AS [v] ON [p].[VatId] = [v].[ID]
WHERE [p].[Name] LIKE N'%teszt%'
```
Látható, hogy minden szükséges értéket lekérdez az utasítás, és a legjobb, hogy azok helyrerakásával nekünk nem is kell foglalkoznunk.