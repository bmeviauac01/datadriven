# 1. MSSQL szerveroldali programozás

MSSQL házi feladat, a teljesítéssel **4 pont és 3 iMsc pont** szerezhető.

GitHub Classroom segítségével hozz létre magadnak egy repository-t. A **meghívó URL-t Moodle-ben találod**. Klónozd le az így elkészült repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. Hozz létre egy `megoldas` nevű branchet, és **arra dolgozz**. A feladatok elkészítése után kommitold és pushold a megoldásod.

A megoldáshoz szükséges szoftvereket és eszközöket lásd [itt](../index.md#szukseges-eszkozok).

## Adatbázis előkészítése

Hozz létre egy új adatbázist, amelynek **neve megegyezik a Neptun kódoddal**. Ebben az adatbázisban futtasd le a táblákat létrehozó szkriptet.

!!! warning "Neptun kód fontos"
    A feladatok alább kérnek képernyőképet, amelyen szerepelnie kell az adatbázis nevének, amely megegyezik a Neptun kódoddal!

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Jelszó lejárat és karbantartása (2 pont)

Biztonsági megfontolásból szeretnénk kötelezővé tenni a jelszó időnkénti megújítását. Ehhez rögzíteni fogjuk, és karbantartjuk a jelszó utolsó módosításának idejét.

1. Adj hozzá egy új oszlopot a `Customer` táblához `PasswordExpiry` néven, ami egy dátumot tartalmaz: `alter table [Customer] add [PasswordExpiry] datetime`.

1. Készíts egy triggert, amellyel jelszó változtatás esetén automatikusan kitöltésre kerül a `PasswordExpiry` mező értéke. Az új értéke a mai dátum plusz egy év legyen. Az értéket a szerver számítsa ki. Ügyelj arra, hogy új vevő regisztrálásakor (_insert_) mindig kitöltésre kerüljön a mező, viszont a vevő adatainak szerkesztésekor (_update_) csak akkor változzon a lejárat dátuma, ha változott a jelszó. (Tehát pl. ha az email címet változtatták csak, akkor a lejárat ne változzon.) A trigger csak a beszúrt/frissített rekorddal törődjön (tehát nem a tábla teljes tartalmára kell frissíteni a dátum oszlopot)! A feladatban készülnöd kell arra is, hogy egyszerre több rekord módosul.

Ellenőrizd a trigger viselkedését különböző esetekre is!

!!! example "BEADANDÓ"
    A trigger kódját az `f1.sql` fájlban add be. Az sql fájl egyetlen utasítást tartalmazzon csak (egyetlen `create trigger`), ne legyen benne se `use` se `go` parancs!

    Készíts egy képernyőképet a `Customer` tábla tartalmáról, amiben látható az új oszlop és annak kitöltött értékei (a tesztelés utáni állapottal). A képen legyen látható az adatbázisod neve (a Neptun kódod). A képet `f1.png` néven mentsd el és add be a megoldásod részeként!

## Feladat 2: Számla érvénytelenítése (2 pont)

Szeretnénk lehetőséget biztosítani rendelések lemondására is egy tárolt eljárás segítségével. Az eljárás a vevő nevével és a rendelés azonosítójával rendelkező számlát fogja érvényteleníteni, majd a hozzá tartozó rendelés tételein végigmenve visszaállítja a raktárkészletet.

1. Hozz létre egy tárolt eljárást `cancel_invoice` mely két paramétert fogad: a vevő nevét `name` néven, és a megrendelés azonosítóját `orderId` néven.

2. A tárolt eljárás ellenőrizze le, hogy van-e a megadott adatokkal számla, ha nincs, dobjon kivételt. A kivétel `error_number` értéke legyen 51000.

3. Ha az adatok jók, akkor a tárolt eljárás vegye az összes számlán szereplő terméket, nézze meg, hogy mennyit vettek belőlük, és a megrendelt mennyiséget adja hozzá a raktárkészlethez. (TIPP: az adatok összeszedéséhez több tábla, esetleg kurzor is kellhet).

Ellenőrizd az eljárás működését!

!!! example "BEADANDÓ"
    A tárolt eljárás kódját az `f2.sql` fájlban add be. Az sql fájl egyetlen utasítást tartalmazzon csak (egyetlen `create procedure cancel_invoice`), ne legyen benne se `use` se `go` parancs!

    Készíts egy képernyőképet amin látható a tárolt eljárás lefutása és annak hatása, illetve mi történik, ha hibás adatokat adunk meg (lehet egy ablakban két tabbal például). A képen legyen látható az adatbázisod neve (a Neptun kódod). A képet `f2.png` néven mentsd el és add be a megoldásod részeként!

!!! danger "MÉG NEM VÉGEZTÉL"
    Ha push-oltad a kódodat, készíts egy PR-t, amihez rendeld hozzá a gyakorlatvezetődet! (részletek: [a házi feladat leadása](../GitHub.md) oldalon)

## Feladat 3: Termék ajánlott korhatára (3 iMsc pont)

!!! note ""
    Az iMsc pont megszerzésére az első két feladat megoldásával együtt van lehetőség.

A minta adatbázisban a termékek (`Product`) rekordjaiban van egy xml típusú `Description` nevű oszlop. Ez néhány terméknél van csak kitöltve.

Egy példa a tartalmára:

```xml hl_lines="9"
<product>
  <product_size>
    <unit>cm</unit>
    <width>150</width>
    <height>50</height>
    <depth>150</depth>
  </product_size>
  <description>Requires battery (not part of the package).</description>
  <recommended_age>0-18 m</recommended_age>
</product>
```

Szeretnénk a `recommended_age` tag tartalmát könnyebb elérhetőség végett egy saját oszlopba helyezni.

1. Adj hozzá egy új oszlopot a `Product` táblához `RecommendedAge` néven, ami egy szöveget tartalmaz: `alter table [Product] add [RecommendedAge] nvarchar(200)`. (Ezt a kódot nem kell beadni a megoldásban.)

1. Írj T-SQL szkriptet, amely minden termék esetén az xml leírásból az `<recommended_age>` elemet kiemelve feltölti az előbb létrehozott `RecommendedAge` oszlopot. Ha az xml leírás üres, vagy nincs benne a keresett elem, akkor maradjon `NULL` az új oszlop tartalma. Ellenkező esetben az xml tag szöveges tartalma kerüljön átmásolásra, és az xml dokumentumból töröld ezt az elemet. Feltételezheted, hogy csak egyetlen `<recommended_age>` elem van az xml-ben.

!!! example "BEADANDÓ"
    A scriptet az `f3.sql` fájlban add be. Ne használj se tárolt eljárást, se triggert, csak egy T-SQL kód blokkot készíts. Az sql fájl önmagában futtatható legyen, ne legyen benne se `use` se `go` parancs!

    Készíts egy képernyőképet a `Product` tábla tartalmáról a kitöltés után. Legyen látható az új oszlop és annak tartalma. A képen legyen látható az adatbázisod neve (a Neptun kódod). A képet `f3.png` néven mentsd el és add be a megoldásod részeként!
