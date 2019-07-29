# Opcionális házi feladat: Entity Framework

A házi feladat opcionális. A teljesítéssel **2 pluszpont és 2 iMsc pont** szerezhető.

GitHub Classroom segítségével a <https://classroom.github.com/a/XBoIGuLn> linken keresztül hozz létre egy repository-t. Klónozd le a repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. A feladatok elkészítése után kommitold és pushold a megoldásod.

## Szükséges eszközök

- Microsoft SQL Server
  - Express változat ingyenesen használható, avagy Visual Studio telepítésével a gyakorlatokon is használt "localdb" változat elérhető
  - Használható a [Docker változat](../Docker-hasznalat.md) is.
- Microsoft SQL Server Management Studio
- Gyakorlatokon is használt minta adatbázis kódja (elérhető a gyakorlatok anyagai között).
  - Előkészületként hozz létre egy új adatbázist, és futtasd le a táblákat létrehozó szkriptet.
- Microsoft Visual Studio 2017/2019 [az itt található beállításokkal](VisualStudio-install.md)

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Adatbázis leképzés Code First modellel és lekérdezések (2 pont)

Készítsd el az adatbázisunk Entity Framework leképzését _Code First_ megoldással. Az Entity Framework Core csomag már része a kiinduló projektünknek, így rögtön kódolhatunk is. Az adatelérés központi eleme a DbContext. Ez az osztály már létezik `AdatvezDbContext` néven.

1. Képezd le a termékeket. Hozz létre egy új osztályt `DbTermek` néven az alábbi kóddal. (A _Db_ prefix egyértelművé teszi, hogy az osztály az adatbázis kontextusában értelmezett. Ez a későbbi feladatnál lesz érdekes.) A leképzésnél többnyire hagyatkozzunk a konvenciókra, azaz a property-k nevénél használjuk az adatbázis oszlopok nevét, így automatikus lesz a leképzés.

   ```C#
   using System.ComponentModel.DataAnnotations.Schema;

   namespace adatvez
   {
       [Table("Termek")]
       public class DbTermek
       {
           [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
           public int ID { get; set; }
           public string Nev { get; set; }
           public double NettoAr { get; set; }
           public int Raktarkeszlet { get; set; }
       }
   }
   ```

   Menj vissza a DbContext-hez és töröld a kommentet a _Termekek_ property elől.

1. Készíts egy `DbAfa` osztályt az AFA tábla leképzésére a `DbTermek`-hez hasonlóan. Ne felejtsd el felvenni a DbSet property-t az `AdatvezContext`-be `AfaKulcsok` néven.

1. Képezd le a Termek - AFA kapcsolatot.

   A `DbTermek` osztályba vegyél fel egy `DbAfa` típusú get-set property-t, ez lesz a navigation property. Használd a `ForeignKey` [attribútumot a property felett](https://docs.microsoft.com/en-us/ef/core/modeling/relationships#foreignkey), ami meghatározza a külső kulcs adatbázis mezőjét ("AFAID").

   Vedd fel ennek az egy-több kapcsolatnak a másik oldalát a DbAFA osztályba. Ez a `Termekek` nevű property `System.Collections.Generic.List` típusú legyen. (Lásd a példában is az előbbi linken.)

A megírt kód kipróbálásához találsz unit teszteket a solution-ben a `TestFeladat1` osztályban. A kódja ki van kommentezve, mert nem fordul, amíg nem írod meg a fentieket. Jelöld ki a teljes kódot, és használd az _Edit / Advanced / Uncomment Selection_ parancsot, majd futtasd a teszteket ([segítség a unit tesztek futtatásához](https://docs.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2019)). Ha nem fordulna le a teszt kód, lehet, hogy egy-egy property névnek mást használtál. A kódodban javítsd a nevet, ne a tesztekben! Az adatbázis eléréséhez a `TestConnectionStringHelper` segédosztályban módosíthatod a connection stringet.

## Feladat 2: Repository megvalósítás Entity Framework-kel (2 iMsc pont)

Az Entity Framework DbContext-je az előzőekben megírt módon nem használható kényelmesen. Például a kapcsolatok betöltését (`Include`) kézzel kell kezdeményezni, és a leképzett entitások túlságosan kötődnek az adatbázis sémájához. Egy komplex alkalmazás esetében ezért célszerű a DbContext-et a repository minta szerint becsomagolni, és ily módon nyújtani az adatelérési réteget.

Implementáld a `TermekRepository` osztályt, amely megvalósítja a termékek listázását és beszúrását. Ehhez már rendelkezésre áll egy új, un. _modell_ osztályt, ami a terméket reprezentálja, de közvetlenül tartalmazza az áfa kulcsot is. Ez az osztály az adatbázis adataiból építkezik, de egységbe zárja az adatokat anélkül, hogy az adatbázishoz kellene fordulni a kapcsolódó áfa rekord lekérdezéséhez. Ez a `TermekAfaval` nevű osztályt, ami tartalmazza a `DbTermek` leképzett tulajdonságait, de a `DbAfa`-ra mutató navigation property helyett az int típusú áfakulcs értékét tartalmazza.

Implementáld a `TermekRepository` osztály függvényeit.

- A `List` az összes terméket adja vissza `TermekAfaval` típusra leképezve.
- Az `Insert` szúrjon be egy új terméket. A kapott ÁFA kulcs értéknek megfelelően keresse ki az adatbázisból a kapcsolódó AFA rekordot, vagy ha nem létezik ilyen kulcs még, akkor szúrjon be egy új AFA rekordot is!
- A `TermekRepository` osztály definícióját (pl. osztály neve, konstruktor, függvények definíciója) ne változtasd meg, csak a függvények törzsét írd meg.

A teszteléshez az előző feladathoz hasonlóan találsz unit teszteket a `TestFeladat2` osztályban.
