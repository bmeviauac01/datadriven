# MongoDB műveletek és a MongoDB .NET Driver használata

Az alábbi, illusztrációra használt kódrészletek a hivatalos [MongoDB.Driver](https://www.nuget.org/packages/mongodb.driver) Nuget csomagot használják.

## Kapcsolat létesítése

A MongoDB adatbázis eléréséhez első lépésben szükségünk van egy kapcsolatra. A kapcsolatot egy `MongoClient` osztály reprezentálja. A kapcsolathoz szükségünk van a szerver elérhetőségére (a connection stringről részletesebben lásd <https://docs.mongodb.com/manual/reference/connection-string/>).

```csharp
var client = new MongoClient("mongodb://localhost:27017");
```

A kapcsolatot singleton-ként érdemes kezelni, nem kell Dispose-olni.

> A kapcsolatot tipikusan egy globális statikus változóban tároljuk, avagy a környezet által támogatott IoC (Inversion of Control) / DI (Dependency Injection) tárolóban helyezzük el.

Az adatbázis neve szerepelhet ugyan a connection stringben (pl. `mongodb://localhost:27017/adatvez`), azt csak az authentikációhoz használja a rendszer. Így a kapcsolat felépítése után meg kell adnunk, milyen adatbázist fogunk használni.

```csharp
var db = client.GetDatabase("adatvez");
```

Az adatbázisnak nem kell előzetesen létezni. A fenti hívás hatására, ha még nem létezik az adatbázis, automatikusan létrejön.

## Gyűjtemények kezelése

Egy relációs adatbázistól eltérően **MongoDB-ben a műveleteinket mindig egyetlen gyűjteményen végezzük**, így a gyűjtemény kiválasztása nem a kiadott parancs része (mint az SQL nyelvben a `where`), hanem a művelet előfeltétele. Egy adott gyűjteményt a `GetCollection` hívással kaphatunk meg, generikus paramétere a dokumentum típust megvalósító C# osztály.

```csharp
var collection = db.GetCollection<BsonDocument>("termekek");
```

A .NET MongoDB driver alap koncepciója szerint minden dokumentumot leképez egy .NET objektumra. Ezzel automatikusan megvalósítja az un. _ODM (Object Document Mapping)_ funkciót. Az ODM az ORM megfelelője a NoSQL adatbázisok világában.

> Más nyelveken és platformokon a MongoDB driverek nem mindig végzik el a leképezést objektumokra, így az interneten található példákban gyakran "nyers" JSON dokumentumokon keresztüli kommunikációt mutatnak. Igyekezzünk ezt elkerülni, ahogy az ORM témakörében megtanultuk, kényelmesebb és biztonságosabb az objektumorientált leképzés.

Az előző példában `BsonDocument` típusú dokumentumot használunk. A `BsonDocument` egy általános dokumentum reprezentáció, amiben kulcs-érték párokat tárolhatunk. Használata kényelmetlen és nem típusbiztos, ezért általában nem ezt a megoldást használjuk. A javasolt megoldást lásd hamarosan.

A gyűjteményt reprezentáló változón tudunk további műveleteket futtatni, például beszúrunk egy dokumentumot, majd listázzuk a gyűjtemény tartalmát. A gyűjtemény első használatkor automatikusan létre fog jönni, azaz semmilyen módon nem szükséges definiálnunk.

```csharp
collection.InsertOne(new BsonDocument()
{
    { "nev", "Alma" },
    { "nettoAr", 123 }
});

// minden dokumentum listázása: szükség van egy szűrési feltételre, ami itt
// egy üres feltétel, azaz minden dokumentumra illeszkedik
var lista = collection.Find(new BsonDocument()).ToList();
foreach(var l in lista)
    Console.WriteLine(l);
```

> A dokumentumban a kulcs nevek konvenció szerint kisbetűvel kezdődnek, mint `nettoAr` (ez az un. _camel case_ írásmód). Ez a szokás a MongoDB világának megfelelő szemlélet historikus okokból. Hacsak nincs jó okunk rá, ne térjünk el ettől.

## Dokumentumok leképzése C# objektumokra

Ahogy a relációs adatbázisoknál láthattuk az objektum-relációs leképzést, MongoDB esetén is célszerű objektumokkal és osztályokkal dolgoznunk. A MongoDB .NET drivere ezt teljes mértékben biztosítja számunkra.

Első lépésként definiálnunk kell a C# osztályt, ill. osztályokat, amikre az adatbázis tartalmát leképezzük. Mivel itt nincs az adatbázisnak és táblának sémája, nem tudjuk a séma alapján gerálni a C# kódot (mint Entity Framework esetén csináltuk). Így ebben a világban inkább a _Code First_ elvet követjük, azaz a C# kódot készítjük el, és abból készül az adatbázis és gyűjtemény (habár tudjuk, hogy itt nincs szó az osztály alapján táblák létrehozásáról).

Definiáljuk a _Termékek_ reprezentáláshoz az alábbi osztályokat.

```csharp
public class Termek
{
    public ObjectId Id { get; set; } // ez lesz az elsődleges kulcs helyett az _id azonosító
    public string Nev { get; set; }
    public float NettoAr { get; set; }
    public int Raktarkeszlet { get; set; }
    public string[] Karegoriak { get; set; } // tömb értékű mező
    public AfaKulcs AfaKulcs { get; set; } // beágyazást alkalmazunk
}

public class AfaKulcs // mivel ez beágyazott entitás, így nem adunk neki egyedi azonosítót
{
    public string AfaKategoriaNev { get; set; }
    public float Kulcs { get; set; }
}
```

Figyeljük meg, hogy korábban `nettoAr` néven használtuk a dokumentumban a kulcsot, de a C# osztályban az un. _Pascal Case_ szerint nagybetűvel kezdjük: `NettoAr`. A MongoDB .NET drivere beépül a C# nyelvbe és a .NET környezetbe, és annak szokásait tiszteletben tartja, így az osztály definícióban szereplő mező nevek és a MongoDB dokumentumaiban a kulcsok leképzése automatikusan meg fog történni, a `NettoAr` osztály tulajdonságból `nettoAr` kulcs név lesz a dokumentumban.

### A leképzés testreszabása

A C# osztály - MongoDB dokumentum leképzés automatikus, de testreszabható. Amennyiben el szeretnénk térni az alap konvencióktól, több féle módon is megtehetjük.

A legegyszerűbb, ha az osztály definíciójában attribútumokkal jelöljük a testreszabást:

```csharp
public class Termek
{
    // _id mezőre képződik le
    [BsonId]
    public ObjectId Azonosito { get; set; }

    // megadhatjuk a MongoDB dokumentumban használatos nevet
    [BsonElement("nettoAr")]
    public string Ar { get; set; }

    // kihagyhatunk egyes mezőket
    [BsonIgnore]
    public string NemMentett { get; set; }
}
```

Másik lehetőségünk magasabb szinten un. konvenció-csomagokat beregisztrálni. A konvenció-csomagok általánosan leírják, hogyan történjen a leképezés. (Az alap viselkedés is egy konvenció-csomag alapján definiált.)

Például az alábbiakkal megadhatjuk, hogy camel case-re szeretnénk a mező neveket leképezni, valamint a default értékkel rendelkező adattagokat (C# nyelv szerint definiált default érték) szeretnénk kihagyni a dokumentumból.

```csharp
// konvenciók definiálása
var pack = new ConventionPack();
pack.Add(new CamelCaseElementNameConvention());
pack.Add(new IgnoreIfDefaultConvention(true));

// konvenciók beregisztrálása
// az első paraméter egy név
// az utolsó paraméterrel szűrési feltételt adhatunk meg, hol használandóak a konvenciók
ConventionRegistry.Register("adatvez", pack, t => true);
```

Ennél bonyolultabb testreszabásokra is lehetőségünk van, például definiálhatunk konverziós logikát a C# reprezentáció és a MongoDB reprezentáció közötti fordításhoz, illetve megadhatjuk a leszármazási hierarchia mentésének módját. Ezekről részletesebben a hivatalos dokumentációban: <https://mongodb.github.io/mongo-csharp-driver/2.8/reference/bson/serialization/>.

## Lekérdezések

A továbbiakban a típusos, `Termek` osztályra leképző módon használjuk a gyűjteményt, és így végzünk műveletet. Ez a javasolt megoldás, a `BsonDocument` alapú megoldást csak szükség esetén használjuk.

A legegyszerűbb lekérdezést már láthattuk, listázzunk minden dokumentumot:

```csharp
var collection = db.GetCollection<Termek>("termekek");

var lista = collection.Find(new BsonDocument()).ToList();
foreach (var l in lista)
    Console.WriteLine($"Id: {l.Id}, Név: {l.Nev}");
```

A listázás a `Find` metódussal történik. Az elnevezés jól mutatja a MongoDB filozófiáját: az adatbázis keresésre való, minden elem listázása nem praktikus, ezért nincs is rá egyszerű szintaktika. A `Find` egy keresési feltételt vár, ami itt egy üres feltétel, azaz mindenre illeszkedik.

A keresési feltételt több féle módon leírhatjuk.

A **`BsonDocument`** alapú szűrésben nyersen, a MongoDB szintaktikája szerint kell megírni a szűrési feltételt. Erre lehetőségünk van ugyan, de elkerüljük, mert a MongoDB .NET drivere ezt megoldja számunkra, ha az alábbiak szerint adjuk meg a keresési feltételt.

A legtöbb esetben egy **Lambda-kifejezéssel** leírhatjuk a feltételt.

```csharp
collection.Find(x => x.NettoAr < 123);
```

Ilyenkor a Lambda-kifejezés egy `Predicate<T>` típusú delegate, azaz a megadott osztálytípuson (itt: `Termek`) fogalmazzuk meg, és `bool` visszatérési értékű. Tehát a fenti példában az `x` változó egy `Termek` objektumot reprezentál. Ez a keresés működik természetesen bonyolultabb esetekre is.

```csharp
collection.Find(x => x.NettoAr < 123 && x.Nev.Contains("piros"));
```

A Lambda kifejezésekkel leírt szűrési feltételek elrejtik, hogy a MongoDB-ben valójában milyen keresési feltételeink is vannak. Például az előbbi `Contains` keresési feltétel egy reguláris kifejezéssel való keresést fog valójában jelenteni.

A MongoDB saját nyelvén az előbbi szűrés így néz ki:

```json
{
  "nettoAr": {
    "$lt": 123.0
  },
  "nev": "/piros/s"
}
```

Vegyük észre, hogy ez a fajta leírás önmaga is egy dokumentum. Ha saját magunk akarnánk megírni a szűrési feltételt, akkor egy `BsonDocument`-ben kellene ezt a dokumentumot összeállítanunk. A szűrési feltételt leíró dokumentum kulcsai a szűréshez használt mezők, az érték pedig a szűrési feltétel. A feltétel bizonyos esetekben egy skalár érték, mint a reguláris kifejezés (vagy ha egyenlőségre szűrnénk), más esetekben a feltétel egy beágyazott dokumentum, mint a `<` feltétel esetén. Ebben az `$lt` kulcs egy speciális kulcs, azt jelöli, hogy a _less than_ operátorral kell a kiértékelés végezni, és az operátor jobb oldalán a 123.0 érték áll. A reguláris kifejezést a [JavaScript RegExp szintaktika](https://www.w3schools.com/jsref/jsref_obj_regexp.asp) szerint kell megadni. Az ilyen módon felsorolt feltételek automatikusan _és_ kapcsolatba kerülnek.

A Lambda-kifejezés helyett egy hasonló leírást magunk is előállíthatunk anélkül, hogy szöveges formában kellene összeállítanunk a szűrési feltételt. A MongoDB .NET drivere lehetőséget ad nekünk arra, hogy egy un. **_builder_** segítségével építsük fel a szűrési feltételt.

```csharp
collection.Find(
    Builders<Termek>.Filter.And(
        Builders<Termek>.Filter.Lt(x => x.NettoAr, 123),
        Builders<Termek>.Filter.Regex(x => x.Nev, "/piros/s"),
    )
);
```

A fenti szintaktikai kicsit bőbeszédűbb ugyan, mint a Lambda-kifejezés, de közelebb áll a MongoDB világához, és jobban leírja, mit is szeretnénk valójában. Tekinthetünk erre a szintaktikára úgy, mint az SQL nyelvre: deklaratív, célorientált, de a platform képességeit szem előtt tartó leírás. Emellett azonban típusbiztos is.

A `Builders<T>` generikus osztály egy segédosztály, amivel szűrési, és később látni fogjuk, egyéb MongoDB specifikus definíciókat építhetünk fel. A `Builders<Termek>.Filter` a _Termek_ C# osztályhoz illeszkedő szűrési feltételek definiálására használható. Először egy _és_ kapcsolatot hozunk létre, amelyen belül két szűrési feltételünk lesz. Az operátorok a korábban látott _less than_ és a reguláris kifejezés. Ezen függvényeknek két paramétert adunk át: a mezőt, amire szűrni szeretnénk, és az operandust.

> Vegyük észre, hogy se itt, se a Lambda-kifejezésekben nem használtunk string alapú mezőneveket, mindenhol ugyanazzal a szintaktikával (ez a _C# Expression_) az osztálydefinícióra hivatkoztunk. Ez azért praktikus így, mert elkerüljük a mezőnevek elgépelését.

Valójában mindegyik leírás, amit használtunk, ugyanazt a szűrési feltételt jelenti. A MongoDB driver mindegyik szintaktikát leképezi a saját belső reprezentációjává. A Lambda-kifejezés alapú kevesebb karaktert igényel, és jobban illeszkedik a C# nyelvbe, míg az utóbbi a MongoDB sajátosságainak kifejezésére való. Bármelyiket használhatjuk.

### Lekérdezés eredményének felhasználása

A `collection.Find(...)` függvény eredménye még nem az eredményhalmaz, hanem csak egy leíró a lekérdezés végrehajtásához. Az eredmény általában három féle módon kérhető le és dolgozható fel.

#### Listázás

Kérjük a teljes eredményhalmazt egy listaként: `collection.Find(...).ToList()`.

#### Első/egyetlen elem lekérése

Amennyiben csak az első elemre van szükségünk, vagy tudjuk, hogy csak egy elem lesz, akkor használhatjuk a `collection.Find(...).First()`, `.FirstOrDefault()`, vagy `.Single()`, `.SingleOrDefault()` függvényeket.

#### Kurzor

Ha az eredményhalmaz sok dokumentumot tartalmaz, célszerű kurzorral feldolgozni. A MongoDB limitálja a lekérdezésre adott válasz méretét, ezért ha túl sok dokumentumot kérdezünk le, előfordulhat, hogy az eredmény helyett hibát fogunk kapni. Ennek feloldására használjuk a kurzorokat, ahol mindig csak egy részhalmazát kapjuk a dokumentumoknak.

```csharp
var cur = collection.Find(...).ToCursor();
while (cur.MoveNext()) // kurzor léptetése
{
    foreach (var t in cur.Current) // a kurzor aktuális eleme nem egy dokumentum, hanem egy lista
    { ... }
}
```

### Szűréshez használható operátorok

A szűrési feltételek a dokumentumban található mezőkre vonatkoznak, és a szűrési feltétel mindig egy konstans. Tehát **nem lehetséges például két mezőt összehasonlítani**, és nem tudunk más gyűjteményekre se hivatkozni. Létezik a MongoDB-ben egy un. aggregációs pipeline, amely segítségével bonyolultabb lekérdezéseket is megfogalmazhatunk, most viszont az egyszerű lekérdezésekre koncentrálunk.

A szűrési feltétel tehát a dokumentum egy mezőjét egy általunk megadott konstanshoz hasonlítja. Az alábbi lehetőségek a leggyakrabban használtak.

#### Összehasonlítási operátorok

```csharp
collection.Find(x => x.NettoAr == 123);
collection.Find(Builders<Termek>.Filter.Eq(x => x.NettoAr, 123)); //Eq, mint equals

collection.Find(x => x.NettoAr != 123);
collection.Find(Builders<Termek>.Filter.Ne(x => x.NettoAr, 123)); // Ne, mint not equals

collection.Find(x => x.NettoAr >= 123);
collection.Find(Builders<Termek>.Filter.Gte(x => x.NettoAr, 123)); // Gte, mint greater than or equal to

collection.Find(x => x.NettoAr < 123);
collection.Find(Builders<Termek>.Filter.Lt(x => x.NettoAr, 123)); // Lt, mint less than
```

#### Logikai operátorok

```csharp
collection.Find(x => x.NettoAr > 500 && x.NettoAr < 1000);
collection.Find(
    Builders<Termek>.Filter.And(
        Builders<Termek>.Filter.Gt(x => x.NettoAr, 500),
        Builders<Termek>.Filter.Lt(x => x.NettoAr, 1000)
    )
);

collection.Find(x => x.NettoAr < 500 || x.Raktarkeszlet < 10);
collection.Find(
    Builders<Termek>.Filter.Or(
        Builders<Termek>.Filter.Lt(x => x.NettoAr, 500),
        Builders<Termek>.Filter.Lt(x => x.Raktarkeszlet, 10)
    )
);

collection.Find(x => !(x.NettoAr < 500 || x.Raktarkeszlet < 10));
collection.Find(
    Builders<Termek>.Filter.Not(
        Builders<Termek>.Filter.And(
            Builders<Termek>.Filter.Lt(x => x.NettoAr, 500),
            Builders<Termek>.Filter.Lt(x => x.Raktarkeszlet, 10)
        )
    )
);
```

#### Több érték közül valamelyikkel megegyező

```csharp
collection.Find(x => x.Id == ... || x.Id = ...);
collection.Find(Builders<Termek>.Filter.In(x => x.Id, new[] { ... }));
// hasonlóan létezik a Nin, mint not in operátor
```

#### Érték létezik (nem null)

```csharp
collection.Find(x => x.AfaKulcs != null);
collection.Find(Builders<Termek>.Filter.Exists(x => x.AfaKulcs));
```

> A létezik-e, azaz nem null szűrés azért különleges, mert a MongoDB szempontjából két módon is lehet null egy érték: ha a kulcs létezik a dokumentumban és értéke null; avagy, ha a kulcs nem is létezik.

### Szűrés beágyazott dokumentum mezőjére

A MongoDB szempontjából a beágyazott dokumentumok ugyanúgy használhatók szűrésre, tehát az alábbiak mind érvényesek, és az se okoz gondot, ha a beágyaztott dokumentum (a példákban az _AfaKulcs_ nem létezik):

```csharp
collection.Find(x => x.AfaKulcs.Kulcs < 27);
collection.Find(Builders<Termek>.Filter.Lt(x => x.AfaKulcs.Kulcs));

collection.Find(Builders<Termek>.Filter.Exists(x => x.AfaKulcs.Kulcs, exists: false));
// ez a nem létezik, azaz null szűrés
```

### Szűrés tömb értékű mezőre

A dokumentum bármely mezője lehet tömb értékű, mint a példában a `string[] Kategoriak`. MongoDB-ben a tömbökkel is egyszerűen tudunk dolgozni, az `Any*` szűrési feltételekkel.

```csharp
// azon termékeket, amelyek a jelzett kategóriában vannak
collection.Find(Builders<Termek>.Filter.AnyEq(x => x.Karegoriak, "Labdák"));

// azon termékeket, amelyek legalább egy olyan kategóriához tartoznak, amelyet nem soroltunk fel
collection.Find(Builders<Termek>.Filter.AnyNin(x => x.Karegoriak, new[] { "Labdák", "Ütők" }));
```

> Az `Any*` feltételek a tömb minden elemét vizsgálják, de a dokumentum szempontjából csak egyszer illeszkednek. Tehát, ha a tömb több eleme is illeszkedik egy feltételre, attól még csak egyszer kapjuk meg a dokumentumot az eredményhalmazban.

## Lekérdezés-végrehajtó pipeline

A MongoDB lekérdezések egy un. pipeline-on haladnak végig. Ennek részleteivel nem fogunk megismerkedni, de az egyszerű szűréseken kívül pár további, lekérdezésekben használt elemet fogunk látni.

#### Lapozás, rendezés

A lapozáshoz megadatjuk, maximálisan hány illeszkedő dokumentumot kérünk:

```csharp
collection.Find(...).Limit(100);
```

A következő lapon található elemekhez pedig kihagyjuk az első lapon már látott elemeket:

```csharp
collection.Find(...).Skip(100).Limit(100);
```

A `Skip` és `Limit` ebben a formában nem értelmes, ugyanis rendezés nélkül az "első 100 elem" lekérdezés (a kliens számára) nem determinisztikus. Tehát az ilyen jellegű lekérdezésekhez szükséges, hogy egy megfelelő rendezést is megadjunk. A rendezés definiálása a korábban már látott `Builders<T>` segítségével történik.

```csharp
collection.Find(...)
    .Sort(Builders<Termek>.Sort.Ascending(x => x.Nev))
    .Skip(100).Limit(100);
```

> A fenti lapozás még mindig nem teljesen helyes. Például, ha a két lap lekérdezése közben egy termék törlésre kerül, akkor "eggyel arrébb csúsznak" a termékek, és lesz egy termék, amely kimarad a következő lapozásnál. Ez nem csak a MongoDB problémája. Gondolkodtató feladat: hogyan oldható meg ez a probléma?

#### Darabszám lekérdezés

A lekérdezésre illeszkedő dokumentumok számát két féle módon is lekérdezhetjük:

```csharp
collection.CountDocuments(Builders<Termek>.Filter.AnyEq(x => x.Karegoriak, "Labdák"));

collection.Find(Builders<Termek>.Filter.AnyEq(x => x.Karegoriak, "Labdák")).CountDocuments();
```

#### Csoportosítás

A csoportosítás szintaktikailag bonyolult művelet. A csoportosításhoz egy aggregációs pipeline-t kell definiálnunk. Ezzel részletesebben nem foglalkozunk, az alábbi példa mutatja a használatát.

```csharp
// A "Labdák" kategóriába tartozó termékek az ÁFA kulcs szerint csoportosítva
foreach (var g in collection.Aggregate()
                            .Match(Builders<Termek>.Filter.AnyEq(x => x.Karegoriak, "Labdák")) // szűrés
                            .Group(x => x.AfaKulcs.Kulcs, x => x) // csoportosítás
                            .ToList())
{
    Console.WriteLine($"Áfa kulcs: {g.Key}");
    foreach(var t in g)
        Console.WriteLine($"\tTermék: {t.Nev}");
}
```

## Beszúrás, módosítás, törlés

A lekérdezések után ismerkedjünk meg az adatmódosításokkal.

### Új dokumentum beszúrása

Új dokumentum beszúrásához az új dokumentumot reprezentáló objektumra van szükségünk. Ezt a gyűjteményhez tudjuk hozzáadni.

```csharp
var ujTermek = new Termek
{
    Nev = "Alma",
    NettoAr = 890,
    Karegoriak = new[] { "Gyümölcsök" }
};
collection.InsertOne(ujTermek);

Console.WriteLine($"Beszúrt rekord id: {ujTermek.Id}"); // beszúrás után frissítésre kerül a C# objektum, és lekérdezhető az Id-ja
```

Figyeljük meg, hogy az `Id` mezőt nem töltöttük ki. Ezt a kliens oldali driver pótolni fogja. Ha akarjuk, mi is adhatunk neki értéket, de nem szokás.

Emlékezzünk rá, hogy a MongoDB-ben nincs séma, így a beszúrt dokumentum lehet teljesen eltérő a gyűjteményben található többi elemtől. Illetve figyeljük meg, hogy nem adtunk minden mezőnek értéket. Mivel nincsenek integritási kritériumok, így minden beszúrás sikerrel fog járni, viszont a lekérdezésnél lehetnek belőle problémák (pl. ha feltételezzük, hogy a raktárkészlet mindig ki van töltve).

Több dokumentum beszúrására az `InsertMany` függvény használható, azonban ne felejtkezzünk el arról, hogy nincsenek tranzakciók, így a több dokumentum beszúrása egyenként független művelet. Ha a beszúrások végrehajtása közben valamely okból hiba történik, az addig sikeresen beszúrt dokumentumok az adatbázisban maradnak. Az egyes dokumentumok azonban atomi módon kerülnek mentésre, tehát egy hiba során se kerülhet egy "fél" dokumentum az adatbázisba.

### Dokumentumok törlése

A törléshez egy szűrési feltételt kell definiálnunk, és vagy a `DeleteOne`, vagy a `DeleteMany` függvénnyel törölhetünk. A különbség, hogy a `DeleteOne` az _első_ illeszkedő dokumentumot törli csak, míg a `DeleteMany` az összeset. Ha tudjuk, hogy a feltételnek csak egy dokumentum felelhet meg (például id alapján törlünk), akkor érdemes a `DeleteOne`-t használni, mert az adatbázisnak nem kell kimerítő keresést végeznie.

A törlés feltétele a keresésnél megismert szintaktikákkal írható le.

> A törlés tehát eltér az Entity Framework esetén tapasztalható viselkedésről. Itt nem kell az entitásnak betöltve lennie, és nem az entitást töröljük, hanem szűrési feltétellel írjuk le a törlést.

```csharp
var deleteResult = collection.DeleteOne(x => x.Id == new ObjectId("..."));
Console.WriteLine($"Törölve: {deleteResult.DeletedCount} db");
```

Ha szeretnénk a törölt elemet megkapni, akkor használhatjuk a `FindOneAndDelete`-t, amely visszaadja a törölt entitást magát.

### Dokumentumok megváltoztatása

A MongoDB talán legérdekesebb képességei a dokumentumok megváltoztatása körül találhatóak. Míg a korábbiak, a lekérdezések, beszúrások, törlések a legtöbb adatbázis (akár relációs, akár NoSQL) esetén hasonlóak, a MongoDB a módosító műveletekben jóval szélesebb spektrumot támogat.

Alapvetően két féle módon tudunk egy dokumentumot megváltoztatni: lecserélni az egész dokumentumot egy újra, avagy részeit frissíteni.

#### Dokumentum teljes cseréje

A dokumentum teljes cseréjéhez szükségünk van egy szűrési feltételre, amellyel megadjuk, mely dokumentumot akarjuk cserélni; valamint szükségünk van az új dokumentumra.

```csharp
var csereTermek = new Termek
{
    Nev = "Alma",
    NettoAr = 890,
    Karegoriak = new[] { "Gyümölcsök" }
};
var replaceResult = collection.ReplaceOne(x => x.Id == new ObjectId("..."), csereTermek);
Console.WriteLine($"Módosítva: {replaceResult.ModifiedCount}");
```

Ez a csere 1-1 jellegű, azaz egy dokumentumot cserélünk egy dokumentumra. A művelet magában atomi, azaz ha menet közben megszakad, akkor se fordulhat elő, hogy egy fél dokumentum került elmentésre. Ha szeretnénk megkapni a csere előtti dokumentumot, akkor a `FindOneAndReplace` metódust használhatjuk.

> Érdekesség: a csere során lehetőség van a dokumentum id-jának módosítására is. Ha a csere dokumentumban más id szerepel, a dokumentum id-ja megváltozik.

#### Dokumentum módosító operátorok

A dokumentum módosító operátorokkal atomi módon tudunk a dokumentum mezőinek értékén változtatni anélkül, hogy a teljes dokumentumot lecserélnénk. A módosító műveletek leírásához a korábban már látott `Builder<T>` segítségét vesszük igénybe.

Állítsunk be a raktárkészletet egy konstans értékre:

```csharp
collection.UpdateOne(
    filter: x => x.Id == new ObjectId("..."),
    update: Builders<Termek>.Update.Set(x => x.Raktarkeszlet, 5));
```

Az `UpdateOne` függvény első paramétere a szűrési feltétel. Leírásához bármely korábban ismertetett szintaktika használható. Második paramétere a módosító művelet leírója, amelyet a `Builder<T>` segítségével építhetünk fel.

A fenti példakódban a paraméterek nevét is kiírtuk (`filter:` és `update:`), hogy egyértelmű legyen, paraméter mit jelképez. Ez nem kötelező, de az olvashatóságot növeli (a kódsorok hosszának rovására).

A módosítás nem csak egy műveletet tartalmazhat.

```csharp
collection.UpdateOne(
    filter: x => x.Id == new ObjectId("..."),
    update: Builders<Termek>.Update
                .Set(x => x.Raktarkeszlet, 5) // raktárkészlet legyen 5
                .CurrentDate(x => x.RaktarFeltolve) // mai dátumot beírjuk, mint a feltöltés ideje
                .Unset(x => x.Hianycikk) // töröljük a hiányzikk jelzést
);
```

A tipikusan használt módosító operátorok:

- `Set`: mező értékének beállítása;
- `SetOnInsert`: mint a `Set`, de csak új dokumentum beszúrása esetén fut le (lásd _upsert_ alább);
- `Unset`: mező törlése (a kulcs és érték eltávolítása a dokumentumból);
- `CurrentDate`: aktuális dátum beírása;
- `Inc`: számláló növelése;
- `Min`, `Max`: mező értékének lecserélése, amennyiben a megadott érték kisebb/nagyobb, mint a mező jelenlegi értéke;
- `Mul`: érték megszorzása;
- `PopFirst`, `PopLast`: tömbből első/utolsó elem eltávolítása;
- `Pull`: tömbből érték eltávolítása;
- `Push`: tömbhöz érték hozzáadása a végére (további lehetőségek ugyanebben az operátorban: tömb sorrendezése, tömb első _n_ elemének megtartása);
- `AddToSet`: tömbhöz érték hozzáadása, ha még nem létezik.

A fenti műveletek akkor is értelmezettek, ha a megadott mező nem létezik. Az operátor típusától függően egy alapértelmezett értéken végzi a módosítást az adatbázis. Például az `Inc` és `Mul` esetén a mező 0 értéket vesz fel, és azon történik a módosítás. A tömb műveletek esetén egy üres tömb kerül módosításra. A többi művelet esetén [dokumentációból](https://docs.mongodb.com/manual/reference/operator/update/) kikereshető a viselkedés.

A fent látott módszerrel nem csak egyetlen dokumentum módosítható. A kért szerkesztő művelet több, a szűrési feltételre illeszkedő dokumentumon is elvégezhető.

Például: a nyári szezonra való tekintettel _minden_ labdára adjunk 25% engedményt, és adjuk őket hozzá az akciós kategóriához.

```csharp
collection.UpdateMany(
    filter: Builders<Termek>.Filter.AnyEq(x => x.Karegoriak, "Labdák"),
    update: Builders<Termek>.Update.Mul(x => x.Raktarkeszlet, 0.75)
                                   .AddToSet(x => x.Karegoriak, "Akciós termékek"));
```

A módosító operátorok atomi módon teszik szerkeszthetővé a dokumentumainkat. Használatukkal kiküszöbölhető a konkurens adathozzáférésből eredő problémák egy része.

#### _Upsert_: nemlétező dokumentum cseréje

Mindkét módosító művelet során lehetőségünk van az un. _upsert (update/insert)_ jellegű működésre. Ez azt jelenti, hogy vagy beszúrás, vagy módosítás történik, annak függvényében, hogy megtalálható volt-e az elem az adatbázisban. Az alapvető viselkedés _nem_ upsert, azt külön kérnünk kell.

```csharp
collection.ReplaceOne(
    filter: x => x.Id == new ObjectId("..."),
    replacement: cseret,
    options: new UpdateOptions() { IsUpsert = true });
```

Nem csak a teljes dokumentum cseréje esetén van lehetőségünk upsert-re. A dokumentum módosító operátorokkal is elvégezhetjük ugyanezt. Ahogy láthattuk, a módosító operátorokat nem zavarja, ha nem létezik egy mező. Ugyanígy nem okoz gondot, ha nem létezik a dokumentum; ez azzal egyenértékű, mintha egy teljesen üres dokumentumon végeznénk el a módosító műveleteket.

```csharp
collection.UpdateOne(filter: ..., update: ..., options: new UpdateOptions() { IsUpsert = true });
```

Az upsert művelet egy eszköz a konkurens módosítások terén a tranzakció hiányára. Mivel nincs tranzakciónk, ezért nem tudunk meggyőződni arról a beszúrás előtt, hogy még nem létezik egy adott rekord. Helyette használhatjuk az upsert módszert, ami atomi lekérdezést és beszúrást/módosítást tesz lehetővé.

> Megjegyzés: SQL nyelvben a `merge` parancs nyújt erre hasonló megoldást.
