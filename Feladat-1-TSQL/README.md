# Opcionális házi feladat 1: szerveroldali programozás MSSQL platformon

A házi feladat opcionális. A teljesítéssel **3 iMsc pont** szerezhető.

## Beadás

A feladat megoldása **1 darab sql kiterjesztésű szöveges fájl**, amely a minta adatbázison közvetlenül futtatható. GitHub Classroom segítségével a <https://classroom.github.com/a/thGZmyuY> linken keresztül hozz létre egy repository-t. Ez tartalmaz egy üres SQL fájlt, ebbe írd a megoldásod, és úgy pushold a repository-t.

Határidő: **2018. november 4. vasárnap 23.59**

## Szükséges eszközök

* Microsoft SQL Server
  * Express változat ingyenesen használható, avagy Visual Studio telepítésével a gyakorlatokon is használt "localdb" változat elérhető
* Microsoft SQL Server Management Studio
* A tanszéki honlapról letölthető adatbázis létrehozó szkript.
  * Előkészületként hozz létre egy új adatbázist, és futtasd le a táblákat létrehozó szkriptet.

## Feladat: Jelszó lejárat és karbantartása

Biztonsági megfontolásból szeretnénk kötelezővé tenni a jelszó időnkénti megújítását. Ehhez rögzíteni fogjuk, és karbantartjuk a jelszó utolsó módosításának idejét.

Írj T-SQL nyelven szerveroldali kódot az alábbi lépésekkel.

1. Adj hozzá egy új oszlopot a Vevo táblához JelszoLejarat néven, ami egy dátumot tartalmaz. Az oszlop egyelőre maradjon üres. (1p)

1. Készíts triggert, amellyel jelszó változtatás esetén automatikusan kitöltésre kerül a JelszoLejarat mező értéke. Az új értéke a mai dátum plusz egy év legyen. Az értéket a szerver számítsa ki. Ügyelj arra, hogy új vevő regisztrálásakor (insert) mindig kitöltésre kerüljön a mező, viszont a vevő adatainak szerkesztésekor (update) csak akkor változzon a lejárat dátuma, ha változott a jelszó. (Tehát pl. ha az email címet változtatták csak, akkor a lejárat ne változzon.) (2p)

A feladatok megoldása egy-egy T-SQL szkript (avagy egyetlen szkriptben mindkettő megoldás).