# LINQ: Language Integrated Query

Adottak a következő osztályok, és listák ilyen objektumokból.

```csharp
class Product
{
    public int ID;
    public string Name;
    public int Price;
    public int VATID;
}

class VAT
{
    public int ID;
    public int Percentage;
}

List<Product> products = ...
List<VAT> vat = ...
```

!!! important "`System.Linq`"
    A Linq használatához a `System.Linq` névteret kell használnunk:

    ```csharp
    using System.Linq;
    ```

## LINQ műveletek

Az alábbi példáknál, ahol elérhető, mindkét szintaktikát mutatjuk. A két féle szintaktika teljesen egyenértékű.

### Szűrés

```csharp
products.Where(p => p.Price < 1000)

from p in products
where p.Price < 1000
```

### Projekció

```csharp
products.Select(p => p.Name)

from p in products
select p.Name
```

### Join

```csharp
from p in products
join v in vat on p.VATID equals v.Id
select p.Price * v.Percentage

products.Join(vat, p => p.VATID, v => v.Id, (p, v) => p.Price * v.Percentage)
```

### Sorrendezés

```csharp
products.OrderBy[Descending](p => p.Name)
.ThenBy[Descending](p => p.Price)

from p in products
orderby p.Name, p.Price [descending]
```

### Halmaz műveletek

```csharp
products.Select(p => p.Name).Distinct()

products.Where(p => p.Price < 1000)
.Union( products.Where(p => p.Price > 100000) )

// hasonlóan Except, Intersect
```

### Aggregáció

```csharp
products.Count()

products.Select(p => p.Price).Average()

// hasonlóan Sum, Min, Max
```

### Első, utolsó

```csharp
products.First()

products.Last()

products.Where(p => p.Id==12).FirstOrDefault()

products.Where(p => p.Id==12).SingleOrDefault()
```

### Lapozás

```csharp
products.Take(10)

products.Skip(10).Take(10)
```

### Tartalmazás (létezik-e)

```csharp
products.Any(p => p.Price == 1234)

products.Where(p => p.Price == 1234).Any()
```

### Csoportosítás

```csharp
from p in products
group p by p.VATID

products.GroupBy(p => p.VATID)
```

### Bonyolultabb projekció

A projekció során több féle módon kérhetjük az eredményeket.

#### Egész objektum

```csharp
from p in products
...
select p
```

Ilyenkor az eredmény `IQueryable<Product>`, azaz Product osztály példányokat kapunk.

#### Csak bizonyos mező

```csharp
from p in products
...
select p.Name
```

Ilyenkor az eredmény `IQueryable<string>`, azaz csak a neveket kapjuk.

#### Nevesített típusok

```csharp
from p in products
...
select new MyType(p.Name, p.Price)
```

Ilyenkor az eredmény `IQueryable<MyType>`, ahol a _MyType_ osztályt deklarálnunk kell, és a select-ben a konstruktorát hívjuk meg.

#### Névtelen típusok

```csharp
from p in products
where p.Price > 1000
select new { ID = p.ID, Name = p.Name };
```

Névtelen típust a `new { }` szintaktikával hozhatunk létre. Ebből a fordító egy osztály definíciót készít a megadott nevű property-kkel. Ezt tipikusan akkor érdemes használni, ha egy-két tulajdonságot szeretnénk csak lekérdezni, és nincs szükségünk az egész objektumra.

Egy másik gyakori használati esete a névtelen típusnak, amikor nem egy rekord pár tulajdonságára vagyunk kíváncsiak, hanem számított értéket kérdezünk le, pl. a termékek neve és bruttó ára:

```csharp
from p in products
join v in vat on p.VATID equals v.Id
select new { Name = p.Name, FullPrice = p.Price * v.Percentage }
```

## LINQ kifejezések és az IQueryable

Vegyünk egy egyszerű kifejezést: `products.Where(p => p.Price < 1000)`. Ezen kifejezés nem teljes abban az értelemben, hogy a szűrés **nem került végrehajtásra**. A LINQ kifejezések eredménye egy `IQueryable<T>` generikus interfész, amely nem tartalmazza az eredményeket, csupán egy leírót, hogy mi a kifejezés.

Ezt _késői kiértékelésnek_ (deferred execution) hívjuk, ugyanis a leírt művelet csak akkor fog végrehajtódni, amikor az eredményekre ténylegesen is szükség van:

- amikor elkezdünk iterálni az eredményhalmazon (pl. `foreach`),
- amikor elkérjük az első elemet (lásd később, pl. `.First()`),
- amikor listát kérünk az eredményhalmazból (`.ToList()`).

Ez a működés azért praktikus, mert így tudjuk szintaktikailag egymás után fűzni a LINQ műveleteket, mint például:

```csharp
var l = products.Where(p => p.Price < 1000)
                .Where(p => p.Name.Contains('s'))
                .OrderBy(p => p.Name)
                .Select(p => p.Name)
...

// az l változó nem tartalmazza az eredményhalmazt

foreach(var x in l) // itt fog lefutni a tényleges kiértékelés
   { ... }
```

!!! note "Kiértékelés"
    Ha mindenképpen szeretnénk kérni a lefuttatást, akkor tipikusan a `.ToList()`-et használjuk. Ezzel azonban vigyázzunk, fontoljuk meg, tényleg erre van-e szükségünk.

## További információk és példák

Lambda kifejezések: <https://www.tutorialsteacher.com/linq/linq-lambda-expression>

Linq: <https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/>
