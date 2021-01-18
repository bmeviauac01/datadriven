# Redis, másik megközelítés az adat kezelésre

![A Redis logója](images/Redis_Logo.png)

## Redis alap koncepciói

A Redis célja a kiszolgálási sebesség maximalizálása. A cél megvalósításához az egyik legfontosabb érdekessége, hogy alapvetően nem egy perzisztens adatbázis. Idézve a Redis weboldaláról: [Redis is an open source (BSD licensed), in-memory data structure store, used as a database, cache, and message broker.](https://redis.io/topics/introduction)

### Adattárolás

A Redis által támogatott adattárolás is egy sajátos megoldás. Az új verziókban már nincsenek adatbázisok, tehát egy szerveren egy adatbázis van. Ha több adatbázist szeretnénk futtatni, akkor egyszerűen több szervert indítunk el, melyeken lehet különböző adat.

#### Adattípusok

- A legegyszerűbb adat a **string**, mely bármilyen karaktersorozatot jelenthet. Ez lehet akár egy bináris sorozat is, mint például egy JPEG tartalma.
    - Minden másik adat ebből az alap építőelemből készül.
- **List**-ek, melyek valójában úgy viselkednek mint egy **linked list**
- **Set**-ek, melyek minden elemből csak egyet tartalmaznak
- **Sorted set**-ek, melyek valójában rendezett set-ek.
    - Egy lényeges eltérés van a set és a sorted set között, hogy a sorted set-ben lehet egy listát visszakérni megadott elemekből, pl.: az első 10 elemet, vagy az utolsó 5-öt
- **Hash**-ek, melyek mező-érték párokat tárolnak.
- **Bit array (v. bitmap)**-ek, melyek valójában a string-ként tárolt adatokat kezelik úgy, mint egy bitsorozat.
- **HyperLogLog**-ok, melyek komplikáltabbnak hangzanak, mint amik valójában. Lényegük, hogy egyedi elemeket számláljanak. Elnevezése azért komplikáltabb, mert az érték algoritmikusan számított, nem pedig a valódi elemeket komparálja egymással (mely logikusan adatbázis méretétől függően kisebb, vagy nagyobb erőforrás pazarlás lenne).
- **Stream**-ek, melyek komplex adatstruktúrát jelképeznek. A Redis ezt *log data structure*-nek nevezi. Egy stream-hez csak hozzáadni lehet. Lényege, hogy valamilyen folyamatosan változó adatot lehessen nyomon követni.

!!!A HyperLogLog, valamint a Stream adatfajtával nem foglalkozunk mélyebben.

#### Perzisztencia

A perzisztencia nem egy elsődleges része a rendszernek. Ezt az is mutatja, hogy ha szeretnénk, ki is lehet kapcsolni a perzisztens tárolást, így csak egy memóriában tárolt cache-ként fog működni. Ha azonban szükségünk van a perzisztens tárolásra, erre két megoldás támogatott.

Az első megoldás kevésbé hibatűrő, ezt **snapshotting**-nak nevezi a Redis. Lényegében az adatbázist egy időpillanatban menti egy fájlba. Az idő, vagy más faktor, állítható a fejlesztő által. Működéséből következik a gyengesége, mivel ha az előző és az új mentés között bármilyen hiba miatt leáll a rendszer, akkor a köztes adat mind elveszett. Tehát leginkább biztonsági mentések készítésére érdemes használni.

A második már sokkal fontosabb szerepet tölt be, ez valójában a tényleges perzisztencia megoldása a Redis rendszerében. Ezt **append only** módnak nevezik, melyben egy *append-only file*-ba helyeznek minden adatot módosító parancsot. Tehát, bármilyen parancs, mely a memóriában létező adatokat módosítja, az hozzáadódik ehhez a fájlhoz. Ennek szintén látni egy problémáját, mely szerint a fájl, az adatbázis méretével aránytalanul, folyamatosan nő. Ugyanis, ha az adatbázishoz 2 elemet hozzáadunk, majd 1-et elveszünk, akkor ez az append-only file-ban 3 tranzakcióként lesz reprezentálva. Erre a Redis beépített megoldása az, hogy amikor a fájl elér egy bizonyos méretet, akkor újra fog generálódni, mostmár az adatbázis jelenlegi tartalmával, nem pedig az append-only file-ból.

!!!A Redis ajánlása, hogy mindkettő perzisztencia módszert használjuk.

### A Redis használata

A Redis alap funkcionalitása inkább egy cache tároló, mint egy adatbázis. Így tehát leginkább arra is használatos. A legjobb felépítés az, ha egy perzisztens adatbázist használunk az adatok tárolására, és a kiszolgálás gyorsítására igénybe vesszük a Redis szolgáltatásait. Ezzel a fontosabb, valamint a sűrűn lekérdezett adatokat gyorsabban elérhetővé tesszük.

Egy lehetséges megoldás, például MongoDB a perzisztens tárolás érdekében, és Redis mint gyorsítótár, hogy a sűrűn lekérdezett dokumentumokat egy gyorsabb tárhelyen tároljuk. Ilyenkor azt kell megoldanunk, hogy a két rendszer konzisztens legyen. Egy lehetséges megoldás, hogy a kiolvasást engedélyezzük a gyorsítótárból, azonban az írást csak a perzisztens tárhely frissítésével együtt engedélyezzük. Ezzel a megoldással lassítunk a rendszeren, mivel meg kell várnunk a MongoDB, és a Redis frissülését az új adatokkal. A másik megoldás, ha írást is és olvasást is a gyorsítótárban végezzük el, és a gyorsítótár tartalmát megadott időpontonként perzisztáljuk az adatbázisba. Ezzel azt a kockázatot vállaljuk, hogy az új adatot nem feltétlenül mentjük az adatbázisba, így hirtelen leállás esetén az utolsó mentés óta létrejött módosítások elveszhetnek. Azonban az utóbbi megoldás gyorsabb, mint az előző.

### Redis API

A Redis API készítése az azt használó fejlesztők, valamint más csoportok, egyének kezében van. A Redis oldalán található [*Clients*](https://redis.io/clients) alatt találhatóak a használható API-k, nyelv szerint felsorolva. C#-hoz két ajánlott "kliens" van, a ServiceStack.Redis, valamint a StackExchange.Redis library. A bemutatást a StackExchange.Redis segítségével fogjuk elkészíteni.

### Redis szerver

A Redis szervert docker konténer segítségével fogjuk elindítani. A konténerek kezelése és használata a bemutatott eszköz területén kívül esik. A mi esetünkben a legegyszerűbb módon fogjuk elkészíteni a szervert.

```bash
docker run --name redis -d redis
```

### StackExchange.Redis használata

A StackExchange.Redis a StackExchange weboldaliban használatosak, például a Stack Overflow backend cache tárhelye ezzel az API-val készült.

#### Kapcsolat építés a szerverrel

Az API a ConnectionMultiplexer segítségével nyit meg egy, de akár több szerverrel kapcsolatot. A mi esetünkben ez egy szerver lesz, mely alap beállítások szerint a *6379*-es port-on fog kéréseket fogadni. A ConnectionMultiplexer a *6379*-es port-ot alapértelmezett értékként fogja használni, tehát ilyen esetben nem kell specifikálni melyik port-ot használjuk.

```csharp
ConnectionMutliplexer redis = ConnectionMultiplexer.Connect("localhost");
```

A ConnectionMultiplexert tartsuk meg és használjuk újra a kódban. Nem ajánlott a *using* block használata, mivel a ConnectionMultiplexer inicializálása egy erőforrás intenzív folyamat. Az objektum segítségével el tudjuk érni a Redis összes funkcionalitását.

- Le tudjuk kérni az adatbázist
- El tudjuk érni a Redis publish/subscribe vagy pub/sub funkcióit
- El tudunk érni egy cluster-ben egy szervert is, monitorozás vagy karbantartási céllal

#### Redis adatbázis használata

Miután elkészült a ConnectionMultiplexer példányunk, egyszerű a feladatunk.

```csharp
IDatabase database = redis.GetDatabase();
```

Az IDatabase objektum előállítása már nem egy erőforrás intenzív folyamat, így ezt nem muszáj megtartanunk. Ezek után a Redis funkcionalitásának használata már egy egyszerű feladat.

#### String műveletek

Egy egyszerű string eltárolása és visszakérése a következőképp néz ki:

```csharp
database.StringSet("kulcs", "Hello World!");
string value = database.StringGet("kulcs");
Console.WriteLine($"A kulcshoz tartozó érték: {value}"); // Kimenet: "A kulcshoz tartozó érték: Hello World!"
```

#### List műveletek

Listák tárolásához is hozzáférhetünk az API-n keresztül. Ezzel bármilyen lista kezelést emulálhatunk.

```csharp
database.ListRightPush("kulcs", "Hello");   // Ezzel létrejön "kulcs" azonosítóval
                                            // a lista az első elemmel. 
database.ListRightPush("kulcs", "World!");
var value = database.ListGetByIndex("key", 0);
Console.WriteLine($"Az első elem értéke: {value}")   // Kimenet: "Az első elem értéke: Hello"
value = database.ListGetByIndex("key", 1);
Console.WriteLine($"A második elem értéke: {value}") // Kimenet: "A második elem értéke: World!"
```

FIFO

```csharp
database.ListRightPush("kulcs", "Hello");
database.ListRightPush("kulcs", "World!");
var value = database.ListLeftPop("kulcs");
Console.WriteLine($"A kivett elem értéke: {value}"); // Kimenet: "A kivett elem értéke: Hello"
```

LIFO

```csharp
database.ListRightPush("kulcs", "Hello");
database.ListRightPush("kulcs", "World!");
var value = database.ListRightPop("kulcs");
Console.WriteLine($"A kivett elem értéke: {value}"); // Kimenet: "A kivett elem értéke: World!"
```

#### Set és SortedSet műveletek

Set-ek tárolásához is hozzáférhetünk, hasonlóan az előzőkhöz.

```csharp
database.SetAdd("kulcs", "Hello");
database.SetAdd("kulcs", "World!");
var values = database.SetMembers("kulcs");
foreach (var value in values)
{
    Console.Write($"{value}");
}
Console.WriteLine();
// Kimenet: "World!Hello"
```

Sorted set-ek egy bizonyos pontozás alapján működnek. A megadott értékhez egy pontot is meg kell adni, ami alapján a sorrendezés működni fog. Ez alapvetően egy double érték, tehát a lentebb taglalt esetben a char típusú érték double-lé lesz konvertálva, mely alapján végül a sorrend felépül.

```csharp
database.SortedSetAdd("kulcs", "Hello", 'H');
database.SortedSetAdd("kulcs", "World!", 'W');
var values = database.SortedSetRangeByScore("kulcs");
foreach (var value in values)
{
    Console.Write($"{value}");
}
Console.WriteLine();
// Kimenet: "HelloWorld!"
```

#### Hash műveletek

A Hash-re úgy tekinthetünk, mint egy leegyszerűsített JSON objektumra. Nincs lehetőség egymásba ágyazott adatokat tárolni, így a JSON objektumokat le kell egyszerűsíteni, azzal hogy az attribútumok elérhetőségi útvonalát mentjük a Hash kulcsaként. Ezt egy JSONPath nevű algoritmussal meg tudjuk tenni, azonban ez okozhat kompatibilitási problémákat, mivel nincs standard implementációja. Viszont 1 dimenziós objektumokat egyszerűen lehet benne tárolni.

```csharp
database.HashSet("kulcs", "foo", "bar");
database.HashSet("kulcs", "biz", "baz");
var keys = database.HashKeys("kulcs");
var values = database.HashValues("kulcs");
for (int i = 0; i < keys.Length; i++)
{
    Console.WriteLine($"{keys[i]}: \"{values[i]}\"");
}
// Kimenet: foo: "bar"
//          biz: "baz"
```
