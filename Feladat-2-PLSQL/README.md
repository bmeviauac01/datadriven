# Opcionális házi feladat 2: szerveroldali programozás Oracle platformon

A házi feladat opcionális. A teljesítéssel **3 iMsc pont** szerezhető.

## Beadás

A feladat megoldása **1 darab sql kiterjesztésű szöveges fájl**, amely a minta adatbázison közvetlenül futtatható. GitHub Classroom segítségével a <https://classroom.github.com/a/e44RLa2_> linken keresztül hozz létre egy repository-t. Ez tartalmaz egy üres SQL fájlt, ebbe írd a megoldásod, és úgy pushold a repository-t.

Mielőtt nekiállasz a feladat megoldásának, ha még nem tetted meg, ne felejtsd [ezen a formon](https://1drv.ms/xs/s!ApHUeZ7ao_2ThuJdorOCXZoah2Rjyw?wdFormId=%7BFE4E4230%2DFBEF%2D435A%2D9363%2DF33D02A19B75%7D) megadni a neptun kódod és a GitHub account neved.

Határidő: **2018. november 4. vasárnap 23.59**

## Szükséges eszközök

* Oracle Server
  * Az Express változat ingyenesen beszerezhető.
* Oracle SQL Developer
  * Ingyenesen beszerezhető szoftver. Más, alternatív szoftver is használható.
* A tanszéki honlapról letölthető adatbázis létrehozó szkript.
  * A szerver telepítésekor létrejon a "system" felhasználó sémája, lehet ebben dolgozni, avagy [létrehozható egy új, üres séma](  https://docs.oracle.com/cd/E17781_01/admin.112/e18585/toc.htm#XEGSG110) a minta adatbázishoz. Előkészületként futtasd le a táblákat létrehozó szkriptet.

## Feladat: Irányítószám változás miatti karbantartás

A postai irányítószámok időnként meg szoktak változni. Tudomásunkra jutott, hogy az eddig 2045 irányítószámon levő község új irányítószáma 2050 lett. Több megrendelőnk is van a községből. Javítsuk ki az adataikat automatikusan.

A telephely táblában vannak a címek rögzítve. Meglevő adatot nem írhatunk át, meg kell őriznünk a korábbi megrendelésekhez kapcsolódóan az akkor érvényes adatot. Így új telephely rögzítésére lesz szükség.

Írj PL/SQL nyelven szerveroldali kódot az alábbi lépésekkel.

* Minden vásárlóhoz, akinek van 2045-es irányítószáma, készíts egy _új_ telephely rekordot, amelynek az adatai megegyeznek az eddigi rekord adataival, csak az irányítószáma 2050 legyen. (1p)

* Ha a vevőnek eddig ez a 2045-ös irányítószám volt a központi telehelye (Vevo.KozpontiTelephely), állítsd át a Vevő táblában a hivatkozást az új telephely rekordra. (1p)

* Ha van olyan megrendelés, ami még nem "Szállítás alatt" ill. "Kiszállítva" státuszú (Megrendeles.StatuszID), valamint a szállítási cím (Megrendeles.TelephelyID) a régi, 2045-ös irányítószámra mutat, változtasd meg a megrendelésben a hivatkozott telephelyet is. (1p)

Tipp: használj kurzort a megoldáshoz, amely kurzor a telephely rekordokon menjen végig.

A feladatok megoldása egy PL/SQL szkript.