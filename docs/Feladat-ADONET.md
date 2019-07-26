# Szorgalmi házi feladat: Adatelérés ADO.NET technológiával

A házi feladat opcionális. A teljesítéssel **2 pluszpont és 2 iMsc pont** szerezhető.

GitHub Classroom segítségével a <https://classroom.github.com/a/xckRuv3o> linken keresztül hozz létre egy repository-t. Klónozd le a repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. A feladatok elkészítése után kommitold és pushold a megoldásod.

## Szükséges eszközök

- Microsoft SQL Server
  - Express változat ingyenesen használható, avagy Visual Studio telepítésével a gyakorlatokon is használt "localdb" változat elérhető
  - Használható a [Docker változat](Docker-hasznalat.md) is.
- Microsoft SQL Server Management Studio
- Gyakorlatokon is használt minta adatbázis kódja (elérhető a gyakorlatok anyagai között).
  - Előkészületként hozz létre egy új adatbázist, és futtasd le a táblákat létrehozó szkriptet.
- Microsoft Visual Studio 2017/2019

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Termék repository (2 pluszpont)

Készítsd a termékek kezeléséhez egy _repository_ osztályt **ADO.NET Connection** technológiát használva. Nyisd meg a kiinduló kódban az _sln_ fájlt Visual Studion-val. Keresd meg a `TermekRepository` és `Termek` osztályokat. Implementáld a `TermekRepository` osztályt alábbi funkcióit:

- A `Search(string name)` függvény keresse meg az adatbázisban a parméterben kapott terméknévre illeszkedő találatokat, és adja őket vissza C# osztály példányaként. Ha a kapott szűrési paraméter `null`, akkor minden terméket adjon vissza, ellenkező esetben _case-insensitive_ módon a névben bárhol keressen!
- A `FindById(int id)` adja vissza az ID alapján megtalált terméket, vagy `null` értéket, ha nem található ilyen.
- Az `Update(Termek t)` egy létező termék adatait frissítse az adatbázisban a kapott paraméter alapján. Csak a név, nettóár és raktárkészlet változhat, a többi tulajdonságot itt nem kell figyelembe venni.

Ügyelj az alábbiakra:

- Csak a `TermekRepository` osztály kódját módosítsd!
- Csak ADO.NET technológiát használhatsz!
- Védekezz az SQL injectionnel szemben!
- A `Termek` osztály kódját ezen feladathoz ne módosítsd!
- A `TermekRepository` osztály definícióját (pl. osztály neve, konstruktor, függvények definíciója) ne változtasd meg, csak a függvények törzsét írd meg.

A teszteléshez találsz unit teszteket a solution-ben ([segítség a unit tesztek futtatásához](https://docs.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2019)). Az adatbázis eléréhez a `TestConnectionStringHelper` segédosztályban módosíthatod a connection stringet.

## Feladat 2: Optimista konkurenciakezelés (2 iMsc pont)

A termékek adatbázisban történő frissítése esetén vegyük észre, és ne engedjük a módosítást, ha a frissítéssel felülírnánk egy nem látott módosítást. A `TermekRepository.Update` függvénye legyen felelős a helyes viselkedésért, és ne végezze el a kért módosítást, ha _elveszett módosítás_ jellegű konkurenciaproblémát észlel.

A konkrét eset, amit el szeretnénk kerülni:

1. _Alma_ felhasználó lekérdez egy terméket.
1. _Banán_ felhasználó lekérdezi ugyanazt a terméket.
1. _Alma_ felhasználó módosítja a termék árát (vagy más tulajdonságát), visszamenti az adatbázisba.
1. _Banán_ felhasználó is módosítja a termék árát (vagy más tulajdonságát), és felülírja ezzel _Alma_ módosítását figyelmeztetés nélkül.

A megoldáshoz az _optimista konkurenciakezelés_ koncepcióját alkalmazd. Ne használj tranzakciót, mert a lekérdezés és módosítás időben eltolva történik, közben az adatbázis kapcsolat megszűnik. A megoldáshoz az adatbázis sémáján ne módosíts, nem vehetsz fel verzió vagy időbélyeg oszlopot! Arra viszont építhetsz, hogy a lekérdezés-módosítás során ugyanazon `Termek` példányon dolgozik a felhasználó.

A megoldást a `TermekRepository.Update` függvényben írd meg, felülírva az előző feladat megoldását, de megtartva annak viselkedését, amennyiben nincs észlelt probléma.

Ügyelj az alábbiakra:

- Csak a `TermekRepository` és `Termek` osztályok kódját módosítsd!
- Csak ADO.NET technológiát használhatsz!
- Védekezz az SQL injectionnel szemben!
- A `TermekRepository` osztály definícióját (pl. osztály neve, konstruktor, függvények definíciója) ne változtasd meg, csak a függvények törzsét írd meg.
- A `Termek` osztályban konstruktorának definícióját (paraméterek darabszáma, sorrendje, nevei) ne változtasd meg, de a kódját átírhatod. A meglevő property-ket ne változtasd meg, de újakat felvehetsz.
