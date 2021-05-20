# Feladat: Entity Framework

A házi feladat opcionális. A teljesítéssel **2 pluszpont és 2 iMsc pont** szerezhető.

GitHub Classroom segítségével a <https://classroom.github.com/a/vu7F1SHB> linken keresztül hozz létre egy repository-t. Klónozd le a repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. A feladatok elkészítése után kommitold és pushold a megoldásod.

!!! warning "Entity Framework _Core_"
    A feladatban Entity Framework **Core**-t használunk. A gyakorlaton használt Entity Framework-től eltérően ez egy platformfüggetlen technológia.

## Szükséges eszközök

- Windows, Linux vagy MacOS: Minden szükséges program platform független, vagy van platformfüggetlen alternatívája.
- Microsoft SQL Server
    - Express változat ingyenesen használható, avagy Visual Studio mellett feltelepülő _localdb_ változat is megfelelő
    - Van [Linux változata](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup) is.
    - MacOS-en Docker-rel futtatható.
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms), vagy kipróbálható a platformfüggetlen [Azure Data Studio](https://docs.microsoft.com/en-us/sql/azure-data-studio/download) is
- Adatbázis létrehozó script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/adatvezerelt/master/docs/db/mssql.sql)
- GitHub account és egy git kliens
- Microsoft Visual Studio 2019 [az itt található beállításokkal](../VisualStudio.md)
    - Linux és MacOS esetén Visual Studio Code és a .NET Core SDK-val települő [dotnet CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/) használható.
- [.NET Core **3.1** SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)

    !!! warning ".NET Core 3.1"
        A feladat megoldásához **3.1**-es .NET Core SDK telepítése szükséges.

        Windows-on Visual Studio verzió függvényében lehet, hogy telepítve van (lásd [itt](../VisualStudio.md#net-core-sdk-ellenorzese-es-telepitese) az ellenőrzés módját); ha nem, akkor a fenti linkről kell telepíteni (az SDK-t és _nem_ a runtime-ot.) Linux és MacOS esetén telepíteni szükséges.

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Adatbázis leképzés Code First modellel és lekérdezések (2 pluszpont)

Készítsd el az adatbázisunk (egy részének) Entity Framework leképzését _Code First_ megoldással. Az Entity Framework Core csomag már része a kiinduló projektünknek, így rögtön kódolhatunk is. Az adatelérés központi eleme a DbContext. Ez az osztály már létezik `ProductDbContext` néven.

1. Képezd le a termékeket. Hozz létre egy új osztályt `DbProduct` néven az alábbi kóddal. (A _Db_ prefix egyértelművé teszi, hogy az osztály az adatbázis kontextusában értelmezett. Ez a későbbi feladatnál lesz érdekes.) A leképzésnél többnyire hagyatkozzunk a konvenciókra, azaz a property-k nevénél használjuk az adatbázis oszlopok nevét, így automatikus lesz a leképzés.

    ```C#
    using System.ComponentModel.DataAnnotations.Schema;

    namespace ef
    {
        [Table("Product")]
        public class DbProduct
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int ID { get; set; }
            public string Name { get; set; }
            public double Price { get; set; }
            public int Stock { get; set; }
        }
    }
    ```

    Menj a `ProductDbContext` osztályhoz és töröld a kommentet a `Products` property elől.

1. Készíts egy `DbVat` osztályt a `VAT` tábla leképzésére a `DbProduct`-hoz hasonlóan. Ne felejtsd el felvenni a DbSet property-t az `ProductDbContext`-be `Vat` néven.

1. Képezd le a Product - VAT kapcsolatot.

    A `DbProduct` osztályba vegyél fel egy `DbVat` típusú `Vat` nevű get-set property-t, ez lesz a navigation property. Használd a `ForeignKey` [attribútumot a property felett](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=data-annotations%2Cdata-annotations-simple-key%2Csimple-key#foreign-key), ami meghatározza a külső kulcs adatbázis mezőjét ("VatID").

    Vedd fel ennek az egy-több kapcsolatnak a másik oldalát a `DbVat` osztályba. Ez a `Products` nevű property `System.Collections.Generic.List` típusú legyen. (Lásd a példában is az előbbi linken.)

A teszteléshez találsz unit teszteket a solution-ben. A tesztek kódja ki van kommentezve, mert nem fordul, amíg nem írod meg a fentieket. Jelöld ki a teljes kódot, és használd az _Edit / Advanced / Uncomment Selection_ parancsot. Ezután a teszteket [Visual Studio-ban egyszerűen tudod futtatni](https://docs.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2019), de ha mást használsz fejlesztéshez (pl. VS Code és/vagy `dotnet cli`), akkor is [tudsz teszteket futtatni](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test). Az adatbázis eléréséhez a `TestConnectionStringHelper` segédosztályban módosíthatod a connection stringet.

!!! important "Tesztek"
    A tesztek az adatbázis kiinduló állapotát feltételezik. Futtasd le az adatbázis scriptet a kiinduló állapot visszaállításához.

    A tesztek kódját **NE** módosítsd. Ha a teszteléshez szükséges, ideiglenesen beleszerkeszthetsz, de ügyelj rá, hogy az eredeti állapottal kommitold a megoldásod.

!!! danger "Ha a teszt nem fordul"
    Ha nem fordulna le a teszt kód, lehet, hogy egy-egy property névnek mást használtál. A **saját kódodban javítsd a nevet, ne a tesztekben**!

!!! danger "`OnConfiguring`"
    A _DbContext_ kódjában nincs szükséged connection stringre. A konstruktor intézi a kapcsolat felépítését. Ne írj `OnConfiguring` függvény az osztályba!

!!! example "BEADANDÓ"
    A módosított C# forráskódot tölts fel.

    Emellett készíts egy képernyőképet Visual Studio-ból (vagy a fejlesztéshez használt eszközból, akár `dotnet cli` is lehet), amelyben a vonatkozó teszteket lefuttattad. Látszódjon a **DbContext kódja** és a **tesztek futásának eredménye**! A képet `f1.png` néven mentsd el és add be a megoldásod részeként!

    Ha `dotnet test`-et használsz a teszt futtatásához, a képernyőképen látszódjon az összes teszt neve. Ehhez használd a `-v n` kapcsolód a részletesebb naplózáshoz.

    A képernyőképen levő forráskód tekintetében nem szükséges, hogy a végső megoldásban szereplő kód betűről betűre megegyezen a képen és a feltöltött változatban. Tehát a tesztek sikeres lefutása után elkészített képernyőképet nem szükséges frissíteni, ha a forráskódban **kisebb** változtatást eszközölsz.

## Feladat 2: Repository megvalósítás Entity Framework-kel (2 iMsc pont)

!!! note ""
    Az iMsc pont megszerzésére az első feladat megoldásával együtt van lehetőség.

Az Entity Framework DbContext-je az előzőekben megírt módon nem használható kényelmesen. Például a kapcsolatok betöltését (`Include`) kézzel kell kezdeményezni, és a leképzett entitások túlságosan kötődnek az adatbázis sémájához. Egy komplex alkalmazás esetében ezért célszerű a DbContext-et a repository minta szerint becsomagolni, és ily módon nyújtani az adatelérési réteget.

Implementáld a `ProductRepository` osztályt, amely megvalósítja a termékek listázását és beszúrását. Ehhez már rendelkezésre áll egy új, un. _modell_ osztályt, ami a terméket reprezentálja, de közvetlenül tartalmazza az áfa kulcs százalékos értékét is. Ez az osztály az adatbázis adataiból építkezik, de egységbe zárja az adatokat anélkül, hogy az adatbázishoz kellene fordulni a kapcsolódó áfa rekord lekérdezéséhez. Ez a `Model.Product` nevű osztály, ami tartalmazza a `DbProduct` leképzett tulajdonságait, de a `DbVat`-ra mutató navigation property _helyett_ az int típusú áfakulcs (`VAT.Percentage`) százalékos értékét tartalmazza.

Implementáld a `ProductRepository` osztály függvényeit.

- A `List` az összes terméket adja vissza `Model.Product` típusra leképezve.
- Az `Insert` szúrjon be egy új terméket. A kapott ÁFA kulcs értéknek megfelelően keresse ki az adatbázisból a kapcsolódó `VAT` rekordot, vagy ha nem létezik ilyen kulcs még, akkor szúrjon be egy új `VAT` rekordot is!
- A `ProductRepository` osztály definícióját (pl. osztály neve, konstruktor, függvények definíciója) ne változtasd meg, csak a függvények törzsét írd meg.
- A kódban a `ProductRepository.createDbContext()`-et használd a _DbContext_ létrehozásához (és **ne** a `TestConnectionStringHelper`-t).

!!! example "BEADANDÓ"
    A módosított C# forráskódot tölts fel.
