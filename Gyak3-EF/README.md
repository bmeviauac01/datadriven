# Entity Framework

## Célkitűzés

A gyakorlat célja, hogy a hallgatók megismerjék a Linq lekérdezések használatát, valamint az Entity Framework működését.

## Előfeltételek

A labor elvégzéséhez szükséges eszközök:

- Microsoft Visual Studio 2015/2017/2019 (_nem_ VS Code)
- Microsoft SQL Server (LocalDB vagy Express edition)
- SQL Server Management Studio
- Adatbázis létrehozó script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/gyakorlatok/master/mssql.sql)

Amit érdemes átnézned:

- C# nyelv
- Entity Framework és Linq

## Gyakorlat menete

A gyakorlat végig vezetett, a gyakorlatvezető utasításai szerint haladjunk. Egy-egy részfeladatot próbáljunk meg először önállóan megoldani, utána beszéljük meg a megoldást közösen. Az utolsó feladat opcionális, ha belefér az időbe.

Emlékeztetőként a megoldások is megtalálhatóak az útmutatóban is. Előbb azonban próbáljuk magunk megoldani a feladatot!

## Feladat 0: Adatbázis létrehozása, ellenőrzése

Az adatbázis az adott géphez kötött, ezért nem biztos, hogy a korábban létrehozott adatbázis most is létezik. Ezért először ellenőrizzük, és ha nem találjuk, akkor hozzuk létre újra az adatbázist. (Ennek mikéntjét lásd az első gyakorlat anyagában.)

## Feladat 1: Projekt létrehozása, adatbázis leképzése

Hozz létre Visual Studio segítségével egy C# konzolalkalmazást (_File / New / Project... / Visual C# / Windows desktop / Console application_). A `c/d:\work` mappába dolgozz. (**Ne** .NET _Core_ alkalmazást hozzunk létre, mert abban nincs _Database First_ leképzés, amit használni fogunk.)

1. Adj a projekthez egy *ADO.NET Entity Data Model*t.

   - Solution Explorer-ben a projektre jobb egér / _Add / New Item / Data / ADO.NET Entity Data Model_.
   - A modellt meglévő adatbázis alapján építsd fel (a varázslóban "EF designer from database").
   - A kapcsolatok megadásánál a saját adatbázishoz kapcsolódj. Hozz létre egy új kapcsolatot a varázsló segítségével, és mentsd el a kapcsolódási adatokat a config fájlba.
     - _Data source_: Microsoft SQL Server
     - _Server name_: `(localdb)\mssqllocaldb`
     - _Select or enter database name_: adjuk meg az adatbázis nevét
     - _Save connection settings in App.Config_: igen
   - Entity Framework 6.0-as leképzést használj.
   - Az összes táblát képezzük le.
   - Jegyezd meg a választott nevet (_Model namespace_), pl. `AdatvezEntities`.

1. Keressük meg a _connection stringet_ az `app.config` fájlban. Nézzük meg a tartalmát.

   > Azért jó, ha ide kerül a _connection string_, mert az alkalmazáshoz tartozó adatbázis helye telepítésenként változhat. Ha a forráskódban van a szerver elérhetősége, akkor újra kell fordítani az alkalmazást minden telepítéshez. Az `app.config` fájl viszont az exe mellett része az alkalmazásnak, és szerkeszthető. Ha szükséges, kiemelhető a fájl más konfigurációs állományba is.

1. Nyissuk meg az EF adatmodellt. Vizsgáljuk meg: nézzük meg az entitásokat és kapcsolatokat.

   - Ha szerkeszteni akarjuk a modellt, az _Entity Data Model Browser_ és _Entity Data Model Mapping Details_ ablakokon keresztül lehet szerkeszteni (ezek a _View_ menü, _Other windows_ menüponton keresztül érhetők el).
   - Javítsuk ki az alábbi entitás tulajdonság neveket, hogy jobban illeszkedjenek a valósághoz:

     - Vevo.Telephely -> .Telephely**ek**
     - Megrendeles.MegrendelesTetel -> .MegrendelesTetel**ek**
     - Termek.MegrendelesTetel -> .MegrendelesTetel**ek**
     - AFA.Termek -> .Termek**ek**
     - Kategoria.Termek -> .Termek**ek**

     Mentsük a változtatások után a modellt.

1. Nézd meg a _DbContext_ és egy választott entitás osztály C# kódját. Bontsd ki a _Solution Explorer_-ben az EDM modell fájlját, és alatta ott találhatóak a C# fájlok.

   > Ezen fájlokba _nem_ szerkesztünk bele, mert minden EDM módosítás után újragenerálódnak. Viszont figyeljük meg, hogy minden osztály _partial_-ként van definiálva, így ha szükséges, tudunk a meglevő kód "mellé" új forrásfájlokba sajátot is írni.

## Feladat 2: Lekérdezések

A leképzett adatmodellen fogalmazd meg az alábbi lekérdezéseket Linq használatával. Írd ki konzolra az eredményeket.

Debugger segítségével nézd meg, hogy milyen SQL utasítás generálódik: az IQueryable típusú változóra húzva az egeret látható a generált SQL, amint az eredményhalmaz iterálása elkezdődik.

1. Listázd azon termékek nevét és raktárkészletét, melyből több mint 30 darab van raktáron!

1. Írj olyan lekérdezést, mely kilistázza azon termékeket, melyből legalább kétszer rendeltek!

1. Készíts olyan lekérdezést, mely kilistázza azokat a megrendeléseket, melyek összértéke több mint 30000 Ft! Az eredményhalmaz kiírásakor a vevő nevet követően soronként szerepeljenek az egyes tételek (Termék név, mennyiség, nettó ár).

1. Listázd ki a legdrágább termék adatait!

1. Listázd ki azokat a vevő párokat, akiknek ugyanabban a városban van telephelyük. Egy pár, csak egyszer szerepeljen a listában.

<details><summary markdown="span">Megoldás</summary>

```csharp
Console.WriteLine("***** Második feladat *****");
using (var db = new AdatvezEntities())
{
    // 2.1
    Console.WriteLine("\t2.1:");
    var qTermekRaktarkeszlet = from t in db.Termek
        where t.Raktarkeszlet > 30
        select t;
    foreach (var t in qTermekRaktarkeszlet)
        Console.WriteLine("\t\tNév={0}\tRaktrákészlet={1}", t.Nev, t.Raktarkeszlet);

    // 2.2
    Console.WriteLine("\t2.2:");
    var qTermekRendeles = from t in db.Termek
        where t.MegrendelesTetelek.Count >= 2
        select t;

    foreach (var t in qTermekRendeles)
        Console.WriteLine("\t\tNév={0}\tRaktrákészlet={1}", t.Nev, t.Raktarkeszlet);

    // 2.3
    Console.WriteLine("\t2.3:");
    var qMegrendelesOssz = from m in db.Megrendeles
        where m.MegrendelesTetelek.Sum(mt => mt.Mennyiseg * mt.NettoAr) > 30000
        select m;
    foreach (var m in qMegrendelesOssz)
    {
        Console.WriteLine("\t\tNév={0}", m.Telephely.Vevo.Nev);
        foreach (var mt in m.MegrendelesTetelek)
            Console.WriteLine("\t\t\tTermék={0}\tÁr={1}\tDb={2}", mt.Termek.Nev, mt.NettoAr, mt.Mennyiseg);
    }

    // 2.3 második megoldás
    // Csak egy lekérdezést fog generálni, a Navigation Propertyket is feltölti rögtön
    Console.WriteLine("\tc 2.3 alternatív megoldás:");
    var qMegrendelesOssz2 = from m in
        db.Megrendeles.Include("MegrendelesTetel").Include("MegrendelesTetel.Termek")
            .Include("Telephely").Include("Telephely.Vevo")
        where m.MegrendelesTetelek.Sum(mt => mt.Mennyiseg * mt.NettoAr) > 30000
        select m;
    foreach (var m in qMegrendelesOssz2)
    {
        Console.WriteLine("\t\tNév={0}", m.Telephely.Vevo.Nev);
        foreach (var mt in m.MegrendelesTetelek)
            Console.WriteLine("\t\t\tTermék={0}\tÁr={1}\tDb={2}", mt.Termek.Nev, mt.NettoAr, mt.Mennyiseg);
    }

    // 2.4
    Console.WriteLine("\t2.4:");
    var qTermekMax = from t in db.Termek
        where t.NettoAr == db.Termek.Max(a => a.NettoAr)
        select t;
    foreach (var t in qTermekMax)
        Console.WriteLine("\t\tNév={0}\tÁrt={1}", t.Nev, t.NettoAr);

    // 2.5
    Console.WriteLine("\t2.5:");
    var qJoin = from t1 in db.Telephely
        join t2 in db.Telephely on t1.Varos equals t2.Varos
        where t1.VevoID > t2.VevoID
        select new { v1 = t1.Vevo, v2 = t2.Vevo };
    foreach (var v in qJoin)
        Console.WriteLine("\t\tVevő 1={0}\tVevő 2={1}", v.v1.Nev, v.v2.Nev);

}
```

</details>

## Feladat 3: Adatmódosítások

A DbContext nem csak lekérdezéshez használható, hanem rajta keresztül módosítások is végrehajthatóak.

1. Írj olyan Linq-ra épülő C# kódot, mely az "Építo elemek" (ügyelj az írásmódra!) árát megemeli 10 százalékkal!

1. Hozz létre egy új kategóriát a *Drága játékok*nak, és sorod át ide az összes olyan terméket, melynek ára, nagyobb, mint 8000 Ft!

<details><summary markdown="span">Megoldás</summary>

```csharp
Console.WriteLine("***** Harmadik feladat *****");
using (var db = new AdatvezEntities())
{
    // 3.1
    Console.WriteLine("\t3.1:");
    var qTermekEpito = from t in db.Termek
        where t.Kategoria.Nev == "Építo elemek"
        select t;
    Console.WriteLine("\tMódosítás előtt:");
    foreach (var t in qTermekEpito)
    {
        Console.WriteLine("\t\t\tNév={0}\tRaktrákészlet={1}\tÁr={2}", t.Nev, t.Raktarkeszlet, t.NettoAr);
        t.NettoAr = 1.1 * t.NettoAr;
    }

    db.SaveChanges();

    qTermekEpito = from t in db.Termek
        where t.Kategoria.Nev == "Építo elemek"
        select t;
    Console.WriteLine("\tMódosítás után:");
    foreach (var t in qTermekEpito)
        Console.WriteLine("\t\t\tNév={0}\tRaktrákészlet={1}\tÁr={2}", t.Nev, t.Raktarkeszlet, t.NettoAr);

    // 3.2
    Console.WriteLine("\t3.2:");
    Kategoria dragaJatek = (from k in db.Kategoria
        where k.Nev == "Drága Játék"
        select k).SingleOrDefault();

    if (dragaJatek == null)
    {
        dragaJatek = new Kategoria { Nev = "Drága Játék" };
        
        // Erre nem feltetlenul van szukseg: ha van atrendelt termek, ahhoz hozzakotjuk a kategoria entitast
        // es bekerul automatikusan a kategoria tablaba is. Igy viszont, hogy explicit felvesszuk, (1) jobban
        // kifejezi a szandekunkat; es (2) akkor is felvesszuk a kategoriat, ha vegul nincs atrendelt termek.
        db.Category.Add(dragaJatek);
    }

    var qTermekDraga = from t in db.Termek
        where t.NettoAr > 8000
        select t;

    foreach (var t in qTermekDraga)
        t.Kategoria = dragaJatek;
    db.SaveChanges();

    qTermekDraga = from t in db.Termek
        where t.Kategoria.Nev == "Drága Játék"
        select t;

    foreach (var t in qTermekDraga)
        Console.WriteLine("\t\tNév={0}\tÁrt={1}", t.Nev, t.NettoAr);
}
```

</details>

## Feladat 4: Tárolt eljárások használata

Tárolt eljárások is felvehetők az EDM modellbe modellfrissítés során. A tárolj eljárás vagy a DbContext függvényeként, vagy entitás módosító műveletére köthető be.

A tárolt eljárás leképzésének beállításait (pl. a tárolt eljárás visszatérési típusát) az *Entity Data Model Browser*ben, az adott függvény *Function Import*jához tartozó tulajdonságainál szerkesztenünk.

![Tárolt eljárás mappelése](images/vs-storedproc-mapping.png)

1. Készíts egy tárolt eljárást, mely új fizetési mód rögzítésére szolgál, és visszaadja az új rekord azonosítóját! Használd ezt a tárolt eljárást új entitás felvételéhez!

   - Hozd létre a tárolt eljárást SQL Management Studio segítségével.

     ```sql
     CREATE PROCEDURE FizetesModLetrehozasa
     (
     @Mod nvarchar(20),
     @Hatarido int
     )
     AS
     insert into FizetesMod
     values(@Mod,@Hatarido)
     select scope_identity() as UjId
     ```

   - A tárolt eljárást állítsd be a `FizetesMod` entitás _insert_ metódusának.

     - Add hozzá a tárolt eljárást az EDM-hez. Az EDM Browser-ben jobb kantitással hozd elő a kontextus menüt, használd az "Update model from database"-t, és importáld (_Add_) az új tárolt eljárást.
     - Mentsd el a modell változásait. Ekkor generálódik a háttérben a C# kód.
     - Állítsd be ezt a metódust a `FizetesiMod` entitás _insert_ metódusaként: kiválasztva az EDM-ben a `FizetesiMod` elemet a _Mapping Details_ ablakban válts át a _Map Entity to Functions_ nézetre, és állítsd be _Insert_ metódusnak. A visszatérési értéket feleltesd meg az _ID_ tulajdonságnak. Mentsd el a modell változásait.

       ![Tárolt eljárás mappelése](images/vs-insert-storedproc.png)

   - Próbáld ki a működést: C# kódból adj hozzá egy új fizetési módot a DbContext FizetesiMod gyűjteményéhez az Add metódussal. Ellenőrizd az adatbázisban a rekord létrejöttét.

1. Készíts egy tárolt eljárást, mely kilistázza azon termékeket melyből legalább egy megadott darabszám felett adtak el. Hívd meg a tárolj eljárást C# kódból!

   - Hozd létre a tárolt eljárást az alábbi kóddal.

     ```sql
     CREATE PROCEDURE dbo.NepszeruTermek (
     @MinDB int = 10
     )
     AS
     SELECT Termek.* FROM Termek INNER JOIN
     (
     SELECT MegrendelesTetel.TermekID
     FROM MegrendelesTetel
     GROUP BY MegrendelesTetel.TermekID
     HAVING SUM(MegrendelesTetel.Mennyiseg) > @MinDB
     )a ON Termek.ID=a.TermekID
     ```

   - Importáld az EDM-be a tárolt eljárást. Az eljárás beállításainál (_EDM Model Browser_-ben a _function_-re dupla kattintással nyílik) állítsd be a visszatérési értéket `Termek` típusúra. Mentsd el a modell változásait.

   - Használd a DbContext-en generált új függvényt a tárolt eljárás meghívásához, és írasd ki a termékek nevét!

<details><summary markdown="span">Megoldás</summary>

```csharp
Console.WriteLine("***** Negyedik feladat *****");
using (var db = new AdatvezEntities())
{
    // 4.3
    Console.WriteLine("\t4.3:");

    var f = new FizetesMod
    {
        Mod = "Valamikor hozom",
        Hatarido = 99999
    };

    db.FizetesMod.Add(f);
    db.SaveChanges();

    // 4.6
    Console.WriteLine("\t4.6:");
    var qTermekNepszeru = db.NepszeruTermek(5);
    foreach (var t in qTermekNepszeru)
        Console.WriteLine("\t\tNév={0}\tRaktrákészlet={1}\tÁr={2}", t.Nev, t.Raktarkeszlet, t.NettoAr));
}
```

</details>
