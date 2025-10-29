# 2. Entity Framework

A házi feladat teljesítésével **4 pont és 3 iMsc pont** szerezhető.

GitHub Classroom segítségével hozz létre magadnak egy repository-t. A **meghívó URL-t Moodle-ben találod**. Klónozd le az így elkészült repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. Hozz létre egy `megoldas` nevű branchet, és **arra dolgozz**. A feladatok elkészítése után kommitold és pushold a megoldásod.

A megoldáshoz szükséges szoftvereket és eszközöket lásd [itt](../index.md#szukseges-eszkozok). A feladat MSSQL adatbázist használ.

!!! warning "Entity Framework _Core_"
    A feladatban Entity Framework **Core**-t használunk. A gyakorlaton használt Entity Framework-től eltérően ez egy platformfüggetlen technológia.

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Adatbázis leképzés Code First modellel és lekérdezések (2 pont)

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

1. Készíts egy `DbVat` osztályt a `VAT` tábla leképzésére az `ef` névtérbe a `DbProduct`-hoz hasonlóan. Ne felejtsd el felvenni a DbSet property-t a `ProductDbContext`-be `Vat` néven.

1. Képezd le a Product - VAT kapcsolatot.

    A `DbProduct` osztályba vegyél fel egy `DbVat` típusú `Vat` nevű get-set property-t, ez lesz a navigation property. Használd a `ForeignKey` [attribútumot a property felett](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=data-annotations%2Cdata-annotations-simple-key%2Csimple-key#foreign-key), ami meghatározza a külső kulcs adatbázis mezőjét ("VatID").

    Vedd fel ennek az egy-több kapcsolatnak a másik oldalát a `DbVat` osztályba. Ez a `Products` nevű property `System.Collections.Generic.List` típusú legyen. (Lásd a példában is az előbbi linken.)

A teszteléshez találsz unit teszteket a solution-ben. A tesztek kódja ki van kommentezve, mert nem fordul, amíg nem írod meg a fentieket. Jelöld ki a teljes kódot, és használd az _Edit / Advanced / Uncomment Selection_ parancsot. Ezután a teszteket [Visual Studio-ban egyszerűen tudod futtatni](https://docs.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2022), de ha mást használsz fejlesztéshez (pl. VS Code és/vagy `dotnet cli`), akkor is [tudsz teszteket futtatni](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test). Az adatbázis eléréséhez a `TestConnectionStringHelper` segédosztályban módosíthatod a connection stringet.

!!! important "Tesztek"
    A tesztek az adatbázis kiinduló állapotát feltételezik. Futtasd le az adatbázis scriptet a kiinduló állapot visszaállításához.

    A tesztek kódját **NE** módosítsd. Ha a teszteléshez szükséges, ideiglenesen beleszerkeszthetsz, de ügyelj rá, hogy az eredeti állapottal kommitold a megoldásod.

!!! danger "Ha a teszt nem fordul"
    Ha nem fordulna le a teszt kód, lehet, hogy egy-egy property névnek mást használtál. A **saját kódodban javítsd a nevet, ne a tesztekben**!

!!! danger "`OnConfiguring`"
    A _DbContext_ kódjában nincs szükséged connection stringre. A konstruktor intézi a kapcsolat felépítését. Ne írj `OnConfiguring` függvényt az osztályba!

!!! example "BEADANDÓ"
    A módosított C# forráskódot töltsd fel.

    Emellett készíts egy képernyőképet Visual Studio-ból (vagy másik, a fejlesztéshez használt eszközből, ami akár `dotnet cli` is lehet), amelyben a vonatkozó teszteket lefuttattad. Látszódjon a **DbContext kódja** és a **tesztek futásának eredménye**! A képet `f1.png` néven mentsd el és add be a megoldásod részeként!

    Ha `dotnet test`-et használsz a teszt futtatásához, a képernyőképen látszódjon az összes teszt neve. Ehhez használd a `-t` kapcsolót a megtalált tesztek listázásához, majd utána futtasd újból a parancsot, valahogy így:

    `dotnet test -t && dotnet test`

    A képernyőképen levő forráskód tekintetében nem szükséges, hogy a végső megoldásban szereplő kód betűről betűre megegyezzen a képen és a feltöltött változatban. Tehát a tesztek sikeres lefutása után elkészített képernyőképet nem szükséges frissíteni, ha a forráskódban **kisebb** változtatást eszközölsz.

## Feladat 2: Repository megvalósítás Entity Framework-kel (2 pont)

!!! note ""
    A pont megszerzésére az első feladat megoldásával együtt van lehetőség.

Az Entity Framework DbContext-je az előzőekben megírt módon nem használható kényelmesen. Például a kapcsolatok betöltését (`Include`) kézzel kell kezdeményezni, és a leképzett entitások túlságosan kötődnek az adatbázis sémájához. Egy komplex alkalmazás esetében ezért célszerű a DbContext-et a repository minta szerint becsomagolni, és ily módon nyújtani az adatelérési réteget.

Implementáld a `ProductRepository` osztályt, amely megvalósítja a termékek listázását és beszúrását. Ehhez már rendelkezésre áll egy új, ún. _modell_ osztály, ami a terméket reprezentálja, de közvetlenül tartalmazza az áfa kulcs százalékos értékét is. Ez az osztály az adatbázis adataiból építkezik, de egységbe zárja az adatokat anélkül, hogy az adatbázishoz kellene fordulni a kapcsolódó áfa rekord lekérdezéséhez. Ez a `Model.Product` nevű osztály, ami tartalmazza a `DbProduct` leképzett tulajdonságait, de a `DbVat`-ra mutató navigation property _helyett_ az int típusú áfakulcs (`VAT.Percentage`) százalékos értékét tartalmazza.

Implementáld a `ProductRepository` osztály függvényeit.

- A `List` az összes terméket adja vissza `Model.Product` típusra leképezve.
- Az `Insert` szúrjon be egy új terméket. A kapott ÁFA kulcs értéknek megfelelően keresse ki az adatbázisból a kapcsolódó `VAT` rekordot, vagy ha nem létezik ilyen kulcs még, akkor szúrjon be egy új `VAT` rekordot is! A metódus visszatérési értéke az új elem ID-ja legyen (amit természetesen az adatbázis generál).
- A `Delete` törölje a termék rekordot a megadott id alapján. Csak a termék rekordot kell törölni, kapcsolódó sorokat nem. Ha a törlés külső kulcsok miatt nem hajtható végre, engedd a hívót értesülni a hibáról. Ha a termék nem létezik, a függvény _hamis_ visszatérési értékkel jelezze, a sikeres törlést pedig _igazzal_.
- A `ProductRepository` osztály definícióját (pl. osztály neve, konstruktor, függvények definíciója) ne változtasd meg, csak a függvények törzsét írd meg.
- A kódban a `ProductRepository.createDbContext()`-et használd a _DbContext_ létrehozásához (és **ne** a `TestConnectionStringHelper`-t).

!!! example "BEADANDÓ"
    A módosított C# forráskódot töltsd fel.

!!! danger "MÉG NEM VÉGEZTÉL"
    Ha push-oltad a kódodat, készíts egy PR-t, amihez rendeld hozzá a gyakorlatvezetődet! (részletek: [a házi feladat leadása](../GitHub.md) oldalon)

## Feladat 3: Logikai törlés Entity Framework-kel (3 iMSc pont)

!!! note ""
    A pont megszerzésére az első két feladat megoldásával együtt van lehetőség.

Az adatbázisból való törlés egy olyan művelet, ami számos nemkívánt hatással rendelkezik. Egy törölt adatot visszaállítani sokkal nehezebb, néha nem is lehetséges következmények nélkül. Az adat törlésével akár a teljes adattörténet elveszhet, nem tudunk a törlés előtti állapotról, különböző statisztikákban nem tudjuk felhasználni. Ráadásul előfordulnak olyan esetek, például amikor olyan más táblákkal való kapcsolatok és külső kulcs kényszerek vannak, hogy a törlés kihatással van azokra a táblákra is.

Ezen problémák áthidalására a leggyakoribb megoldás, hogy egy nem végleges törlést, hanem egy úgynevezett logikai törlést (soft delete) vezetünk be. Ebben az esetben egy mező (tipikusan `IsDeleted` névvel) átállításával jelezzük, hogy az adott adat már törölve van. Így az megmarad az adatbázisban is, de tudjuk szűrni, hogy töröltek-e.

A szűrés naiv implementációja azonban nem kényelmes. Képzeljük el, hogy minden lekérdezés vagy mentés esetén oda kell írni a kifejezésbe, hogy ne hasson ezekre a törölt elemekre.
Ennek érdekében az Entity Framework egyik funkcióját érdemes kihasználni, a *Global Query Filter*-t. Ennek a segítségével olyan szűrőfeltételeket határozhatunk meg, amiket globálisan, minden egyes lekérdezésnél automatikusan alkalmaz az Entity Framework. 

Implementáld a logikai törlést az előbbiekben elkészített `DbProduct` osztályhoz (több megoldási lehetőség is van, tetszőlegesen választhatod bármelyiket):

!!! important "Módosíthatóság"
    Bár az előző feladatban volt megkötés, hogy ne legyen az `OnConfiguring` függvény felüldefiniálva, amennyiben szükségesnek látod, itt nyugodtan lehet (illetve más függvényeket is felüldefiniálhatsz a DBContext megvalósításban)!

1. Vegyél fel egy `IsDeleted` változót, ami jelzi az alkalmazásunk számára, hogy az adott entitás törölt állapotban van!

1. Vegyél fel egy *QueryFilter*-t, ami minden lekérdezéskor kiszűri azokat a termékeket, amiket már töröltünk, így azokat nem kapjuk vissza! 

1. Az adatbázisból való törlés viselkedését változtasd meg **általánosan** a `DbContext` mentés műveleteit kibővítve (erre több kiterjesztési pontot is nyújt az EFCore), hogy az igazi törlés helyett csak átváltoztassa az `IsDeleted` változót! Ne változtasd meg a törlés műveletet a repositoryban módosításra!

!!! example "BEADANDÓ"
    A módosított C# forráskódot töltsd fel.

