# Feladat: MSSQL szerveroldali programozás

A házi feladat opcionális. A teljesítéssel **2 pluszpont és 2 iMsc pont** szerezhető.

GitHub Classroom segítségével a <TBD> linken keresztül hozz létre egy repository-t. Klónozd le a repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. A feladatok elkészítése után kommitold és pushold a megoldásod.

## Szükséges eszközök

- Windows, Linux vagy MacOS: Minden szükséges program platform független, vagy van platformfüggetlen alternatívája.
- Microsoft SQL Server
    - Express változat ingyenesen használható, avagy Visual Studio mellett feltelepülő _localdb_ változat is megfelelő
    - Van [Linux változata](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup) is.
    - MacOS-en Docker-rel futtatható.
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms), vagy kipróbálható a platformfüggetlen [Azure Data Studio](https://docs.microsoft.com/en-us/sql/azure-data-studio/download) is
- Adatbázis létrehozó script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/adatvezerelt/master/docs/db/mssql.sql)
- GitHub account és egy git kliens

## Adatbázis előkészítése

Hozz létre egy új adatbázist, amelynek **neve megegyezik a Neptun kódoddal**. Ebben az adatbázisban futtasd le a táblákat létrehozó szkriptet.

!!! warning "Neptun kód fontos"
    A feladatok alább kérnek képernyőképet, amelyen szerepelnie kell az adatbázis nevének, amely megegyezik a Neptun kódodal!

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Jelszó lejárat és karbantartása (2 pluszpont)

Biztonsági megfontolásból szeretnénk kötelezővé tenni a jelszó időnkénti megújítását. Ehhez rögzíteni fogjuk, és karbantartjuk a jelszó utolsó módosításának idejét.

1. Adj hozzá egy új oszlopot a `Customer` táblához `PasswordExpiry` néven, ami egy dátumot tartalmaz: `alter table [Customer] add [PasswordExpiry] datetime`.

1. Készíts egy triggert, amellyel jelszó változtatás esetén automatikusan kitöltésre kerül a `PasswordExpiry` mező értéke. Az új értéke a mai dátum plusz egy év legyen. Az értéket a szerver számítsa ki. Ügyelj arra, hogy új vevő regisztrálásakor (_insert_) mindig kitöltésre kerüljön a mező, viszont a vevő adatainak szerkesztésekor (_update_) csak akkor változzon a lejárat dátuma, ha változott a jelszó. (Tehát pl. ha az email címet változtatták csak, akkor a lejárat ne változzon.)

Ellenőrizd a trigger viselkedését különböző esetekre is.

!!! example "BEADANDÓ"
    A trigger kódját az `f1.sql` fájlban add be. Az sql fájl egyetlen utasítást tartalmazzon csak (egyetlen `create trigger`), ne legyen benne se `use` se `go` parancs!

    Készíts egy képernyőképet a `Customer` tábla tartalmáról, amiben látható az új oszlop és annak kitöltött értékei (a tesztelés utáni állapottal). A képen legyen látható az adatbázisod neve (a Neptun kódod). A képet `f1.png` néven mentsd el és add be a megoldásod részeként!

## Feladat 2: Termék ajánlott korhatára (2 iMsc pont)

!!! note ""
    Az iMsc pont megszerzésére az első feladat megoldásával együtt van lehetőség.

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

1. Adj hozzá egy új oszlopot a `Product` táblához `RecommendedAge` néven, ami egy szöveget tartalmaz tartalmaz: `alter table [Product] add [RecommendedAge] nvarchar(200)`.

1. Írj T-SQL szkriptet, amely minden termék esetén az xml leírásból az `<recommended_age>` elemet kiemelve feltölti az előbb létrehozott `RecommendedAge` oszlopot. Ha az xml leírás üres, vagy nincs benne a keresett elem, akkor maradjon `NULL` az új oszlop tartalma. Ellenkező esetben az xml tag szöveges tartalma kerüljön átmásolásra, és az xml dokumentumból töröld ezt az elemet. Feltételezheted, hogy csak egyetlen `<recommended_age>` elem van az xml-ben.

!!! example "BEADANDÓ"
    A scriptet az `f2.sql` fájlban add be. Ne használj se tárolt eljárást, se triggert, csak egy T-SQL kód blokkot készíts. Az sql fájl önmagában futtatható legyen, ne legyen benne se `use` se `go` parancs!

    Készíts egy képernyőképet a `Product` tábla tartalmáról a kitöltés után. Legyen látható az új oszlop és annak tartalma. A képen legyen látható az adatbázisod neve (a Neptun kódod). A képet `f2.png` néven mentsd el és add be a megoldásod részeként!
