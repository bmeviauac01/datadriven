# LINQ: Language Integrated Query

Adottak a következő osztályok, és listák ilyen objektumokból.

```csharp
class Termek
{
    public int ID;
    public string Nev;
    public int Ar;
    public int AfaId;
}

class Afa
{
    public int ID;
    public int Kulcs;
}

List<Termek> termekek = ...
List<Afa> afa = ...
```

## LINQ kifejezések és az IQueryable

Vegyünk egy egyszerű kifejezést: `termekek.Where(t => t.Ar < 1000)`. Ezen kifejezés nem teljes abban az értelemben, hogy a szűrés **nem került végrehajtásra**. A LINQ kifejezések eredménye egy `IQueryable<T>` generikus interfész, amely nem tartalmazza az eredményeket, csupán egy leírót, hogy mi a kifejezés.

Ezt _késői kiértékelésnek_ (deferred execution) hívjuk, ugyanis a leírt művelet csak akkor fog végrehajtódni, amikor az eredményekre ténylegesen is szükség van:

- amikor elkezdünk iterálni az eredményhalmazon (pl. foreach),
- amikor elkérjük az első elemet (lásd később, pl. `.First()`),
- amikor listát kérünk az eredményhalmazból (`.ToList()`).

Ez a működés azért praktikus, mert így tudjuk szintaktikailag egymás után fűzni a LINQ műveleteket, mint például:

```csharp
var l = termekek.Where(t => t.Ar < 1000)
                .Where(t => t.Nev.Contains('s'))
                .OrderBy(t => t.Nev)
                .Select(t => t.Nev)
...

// az l változó nem tartalmazza az eredményhalmazt

foreach(var x in l) // itt fog lefutni a tényleges kiértékelés
   { ... }
```

> Ha mindenképpen szeretnénk kérni a lefuttatást, akkor tipikusan a `.ToList()`-et használjuk. Ezzel azonban vigyázzunk, fontoljuk meg, tényleg erre van-e szükségünk.

## LINQ műveletek

Az alábbi példáknál, ahol elérhető, mindkét szintaktikát mutatjuk. A két féle szintaktika teljesen egyenértékű.

### Szűrés

```csharp
termekek.Where(t => t.Ar < 1000)

from t in termekek
where t.Ar < 1000
```

### Projekció

```csharp
termekek.Select(t => t.Nev)

from t in termekek
select t.Nev
```

### Join

```csharp
from t in termekek
join a in afa on t.AfaId equals a.Id
select t.NettoAr * a.Kulcs

termekek.Join(afa, t => t.AfaId, a => a.Id, (t, a) => t.NettoAr * a.Kulcs)
```

### Sorrendezés

```csharp
termekek.OrderBy[Descending](t => t.Nev)
.ThenBy[Descending](t => t.Ar)

from t in termekek
orderby t.Nev, t.Ar [descending]
```

### Halmaz műveletek

```csharp
termekek.Select(t => t.Nev).Distinct()

termekek.Where(t => t.Ar < 1000)
.Union( termekek.Where(t => t.Ar > 100000) )

// hasonlóan Except, Intersect
```

### Aggregáció

```csharp
termekek.Count()

termekek.Select(t => t.NettoAr).Average()

// hasonlóan Sum, Min, Max
```

### Első, utolsó

```csharp
termekek.First()

termekek.Last()

termekek.Where(t => t.Id==12).FirstOrDefault()

termekek.Where(t => t.Id==12).SingleOrDefault()
```

### Lapozás

```csharp
termekek.Take(10)

termekek.Skip(10).Take(10)
```

### Tartalmazás (létezik-e)

```csharp
termekek.Any(t => t.Ar == 1234)

termekek.Where(t => t.Ar == 1234).Any()
```

### Csoportosítás

```csharp
from t in termekek
group t by t.AfaId

termekek.GroupBy(t => t.AfaId)
```

### Bonyolultabb projekció

A projekció során több féle módon kérhetjük az eredményeket.

### Egész objektum

```csharp
from t in termekek
...
select t
```

Ilyenkor az eredmény `IQueryable<Termek>`, azaz Termék osztály példányokat kapunk.

### Csak bizonyos mező

```csharp
from t in termekek
...
select t.Nev
```

Ilyenkor az eredmény `IQueryable<string>`, azaz csak a neveket kapjuk.

### Nevesített típusok

```csharp
from t in termekek
...
select new MyType(t.Nev, t.NettoAr)
```

Ilyenkor az eredmény `IQueryable<MyType>`, ahol a _MyType_ osztályt deklarálnunk kell, és a select-ben a konstruktorát hívjuk meg.

### Névtelen típusok

```csharp
from t in termek
where t.Ar > 1000
select new { ID = t.ID, Nev = t.Nev };
```

Névtelen típust a `new { }` szintaktikával hozhatunk létre. Ebből a fordító egy osztály definíciót készít a megadott nevű property-kkel. Ezt tipikusan akkor érdemes használni, ha egy-két tulajdonságot szeretnénk csak lekérdezni, és nincs szükségünk az egész objektumra.

Egy másik gyakori használati esete a névtelen típusnak, amikor nem egy rekord pár tulajdonságára vagyunk kíváncsiak, hanem számított értéket kérdezünk le, pl. a termékek neve és bruttó ára:

```csharp
from t in termekek
join a in afa on t.AfaId equals a.Id
select new { Nev = t.Nev, BruttoAr = t.NettoAr * a.Kulcs }
```

## További információk és példák

Lambda kifejezések: <https://www.tutorialsteacher.com/linq/linq-lambda-expression>

Linq: <https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/>
