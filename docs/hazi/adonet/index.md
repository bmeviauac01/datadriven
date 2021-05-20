# Feladat: ADO.NET adatelérés

A házi feladat opcionális. A teljesítéssel **2 pluszpont és 2 iMsc pont** szerezhető.

GitHub Classroom segítségével hozz létre magadnak egy repository-t. A **meghívó URL-t Moodle-ben találod**. Klónozd le az így elkészült repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. A feladatok elkészítése után kommitold és pushold a megoldásod.

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

## Feladat 1: Termék repository (2 pluszpont)

Készíts a termékek (`Product`) kezeléséhez egy _repository_ osztályt **ADO.NET Connection** technológiát használva. Nyisd meg a kiinduló kódban az _sln_ fájlt Visual Studio-val. Keresd meg a `Repository.ProductRepository` és `Model.Product` osztályokat. Implementáld a `ProductRepository` osztályt alábbi funkcióit:

- A `Search(string name)` függvény keresse meg az adatbázisban a paraméterben kapott terméknévre illeszkedő találatokat, és adja őket vissza C# osztály példányaként. Ha a kapott szűrési paraméter `null`, akkor minden terméket adjon vissza, ellenkező esetben _case-insensitive_ módon a névben bárhol keressen!
- A `FindById(int id)` adja vissza az ID alapján megtalált terméket, vagy `null` értéket, ha nem található ilyen.
- Az `Update(Product p)` egy létező termék adatait frissítse az adatbázisban a kapott paraméter alapján. Csak a `Name`, `Price` és `Stock` változhat, a többi tulajdonságot itt nem kell figyelembe venni.

Ügyelj az alábbiakra:

- Csak a `ProductRepository` osztály kódját módosítsd!
- A repository kódjában az ADO.NET kapcsolat megnyitásához a `connectionString` változót használd (és **ne** a `TestConnectionStringHelper`-t).
- A termék áfakulcsát is ki kell keresni, tehát nem a kapcsolódó rekord id-ját kell a `Model.Product` osztálynak átadni, hanem az áfakulcs százalékos értékét! A termék kategóriájának nevét hasonlóan ki kell keresni.
- Csak ADO.NET technológiát használhatsz!
- Védekezz SQL injectionnel szemben!
- A `Model.Product` osztály kódját ezen feladathoz ne módosítsd!
- A `ProductRepository` osztály definícióját (pl. osztály neve, konstruktor, függvények definíciója) ne változtasd meg, csak a függvények törzsét írd meg.

A teszteléshez találsz unit teszteket a solution-ben. Ezeket a teszteket [Visual Studio-ban egyszerűen tudod futtatni](https://docs.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2019), de ha mást használsz fejlesztéshez (pl. VS Code és/vagy `dotnet cli`), akkor is [tudsz teszteket futtatni](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test). Az adatbázis eléréséhez a `TestConnectionStringHelper` segédosztályban módosíthatod a connection stringet.

!!! important "Tesztek"
    A tesztek az adatbázis kiinduló állapotát feltételezik. Futtasd le az adatbázis scriptet a kiinduló állapot visszaállításához.

    A tesztek kódját **NE** módosítsd. Ha a teszteléshez szükséges, ideiglenesen beleszerkeszthetsz, de ügyelj rá, hogy az eredeti állapottal kommitold a megoldásod.

!!! example "BEADANDÓ"
    A módosított C# forráskódot töltsd fel.

    Emellett készíts egy képernyőképet Visual Studio-ból (vagy a fejlesztéshez használt eszközból, akár `dotnet cli` is lehet), amelyben a vonatkozó teszteket lefuttattad. Látszódjon a **repository kódja** (a releváns része, amennyi kifér) és a **tesztek futásának eredménye**! A képet `f1.png` néven mentsd el és add be a megoldásod részeként!

    Ha `dotnet test`-et használsz a teszt futtatásához, a képernyőképen látszódjon az összes teszt neve. Ehhez használd a `-v n` kapcsolód a részletesebb naplózáshoz.

    A képernyőképen levő forráskód tekintetében nem szükséges, hogy a végső megoldásban szereplő kód betűről betűre megegyezen a képen és a feltöltött változatban. Tehát a tesztek sikeres lefutása után elkészített képernyőképet nem szükséges frissíteni, ha a forráskódban **kisebb** változtatást eszközölsz.

## Feladat 2: Optimista konkurenciakezelés (2 iMsc pont)

!!! note ""
    Az iMsc pont megszerzésére az első feladat megoldásával együtt van lehetőség.

A termékek adatbázisban történő frissítése esetén vegyük észre, és ne engedjük a módosítást, ha a frissítéssel felülírnánk egy nem látott módosítást. A `ProductRepository.UpdateWithConcurrencyCheck` függvénye legyen felelős a helyes viselkedésért, és ne végezze el a kért módosítást, ha _elveszett módosítás_ jellegű konkurenciaproblémát észlel.

A konkrét eset, amit el szeretnénk kerülni:

1. _A_ felhasználó lekérdez egy terméket.
1. _B_ felhasználó lekérdezi ugyanazt a terméket.
1. _A_ felhasználó módosítja a termék árát (vagy más tulajdonságát), visszamenti az adatbázisba.
1. _B_ felhasználó is módosítja a termék árát (vagy más tulajdonságát), és felülírja ezzel _A_ módosítását figyelmeztetés nélkül.

!!! tip "Optimista konkurenciakezelés"
    A megoldáshoz az optimista konkurenciakezelés koncepcióját alkalmazd. **Ne használj tranzakciót**, mert a lekérdezés és módosítás időben eltolva történik, közben az adatbázis kapcsolat megszűnik. **Ne használj több SQL utasítást** se, mert a lefutásuk között más adatbázis hozzáférések történhetnek elrontva a várt viselkedést. A megoldást a `ProductRepository.UpdateWithConcurrencyCheck` függvényben írd meg, valamint adaptáld a `Model.Product` osztályt is. Az adatbázisba **nem** vehetsz fel új oszlopot.

Ügyelj az alábbiakra:

- Csak a `ProductRepository.UpdateWithConcurrencyCheck` függvény és a `Model.Product` osztályok kódját módosítsd!
- A függvény visszatérési értékben jelezze, hogy sikeres volt-e a módosítás (vagyis, hogy nem volt konkurencia probléma).
- **Magyarázd el a viselkedést** az `UpdateWithConcurrencyCheck` függvényben egy kommentben (2-3 mondatban).
- Egyetlen SQL parancs használatával oldd meg a feladatot!
- Csak ADO.NET technológiát használhatsz!
- Védekezz SQL injectionnel szemben!
- A `ProductRepository` osztály definícióját (pl. osztály neve, konstruktor, függvények definíciója) ne változtasd meg, csak a függvény törzsét írd meg.
- A `Model.Product` osztályban konstruktorának definícióját (paraméterek darabszáma, sorrendje, nevei) ne változtasd meg, de a kódját átírhatod. A meglevő property-ket ne változtasd meg, de újakat felvehetsz.

!!! example "BEADANDÓ"
    A módosított C# forráskódot töltsd fel. Ne felejtsd a magyarázatot a C# kódban!
