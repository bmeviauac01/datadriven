# Microsoft SQL Server programozása

## Célkitűzés

A gyakorlat célja, hogy a hallgatók megismerjék az Microsoft SQL szerver oldali programozásának alapjait, elsajátítsák az alapfogalmakat és a fejlesztőeszköz használatát.

## Előfeltételek

A labor elvégzéséhez szükséges eszközök:

- Microsoft SQL Server (LocalDB vagy Express edition)
- SQL Server Management Studio
- Adatbázis létrehozó script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/gyakorlatok/master/mssql.sql)

Amit érdemes átnézned:

- SQL nyelv
- Microsoft SQL Server programozása (tárolt eljárások, triggerek)
- [Microsoft SQL Server használata segédlet](../Adatbazis/mssql-server.md)

## Gyakorlat menete

Az első négy feladatot (beleértve a megoldások tesztelését is) a gyakorlatvezetővel együtt oldjuk meg. Az utolsó feladat önálló munka, amennyiben marad rá idő.

Emlékeztetőként a megoldások is megtalálhatóak az útmutatóban is. Előbb azonban próbáljuk magunk megoldani a feladatot!

## Feladat 0: Adatbázis létrehozása, ellenőrzése

Az adatbázis az adott géphez kötött, ezért nem biztos, hogy a korábban létrehozott adatbázis most is létezik. Ezért először ellenőrizzük, és ha nem találjuk, akkor hozzuk létre újra az adatbázist. (Ennek mikéntjét lásd az első gyakorlat anyagában.)

## Feladat 1: SQL parancsok (emlékeztető)

Írjon SQL lekérdezés/utasítást az alábbi feladatokhoz.

1. Hány nem teljesített megrendelésünk van (a státusz alapján)?

   <details><summary markdown="span">Megoldás</summary>

   ```sql
   select count(*)
   from Megrendeles m join Statusz s on m.StatuszID = s.ID
   where s.Nev != 'Kiszállítva'
   ```

   A `join` mellett az oszlopfüggvény (aggregáció) használatára látunk példát. (A táblák kapcsolására nem csak ez a szintaktika használható, előadáson szerepelt alternatív is.)

   </details>

1. Melyek azok a fizetési módok, amit soha nem választottak a megrendelőink?

   <details><summary markdown="span">Megoldás</summary>

   ```sql
   select f.Mod
   from Megrendeles m right outer join FizetesMod f on m.FizetesModID = f.ID
   where m.ID is null
   ```

   A megoldás kulcsa az `outer join`, aminek köszönhetően láthatjuk, mely fizetési mód rekordhoz _nem_ tartozik egyetlen megrendelés se.

   </details>

1. Rögzítsünk be egy új vevőt! Kérdezzük le az újonnan létrejött rekord kulcsát!

   <details><summary markdown="span">Megoldás</summary>

   ```sql
   insert into Vevo(Nev, Login, Jelszo, Email)
   values ('Teszt Elek', 't.elek', '********', 't.elek@email.com')

   select @@IDENTITY
   ```

   Az `insert` után javasolt kiírni az oszlopneveket az egyértelműség végett, bár nem kötelező. Vegyük észre, hogy az ID oszlopnak nem adunk értéket, mert azt a tábla definíciójakor meghatározva a szerver adja automatikusan. Ezért kell utána lekérdeznünk, hogy tudjuk, milyen ID-t adott.

   </details>

1. A kategóriák között hibásan szerepel az _Fajáték_ kategória név. Javítsuk át a kategória nevét *Fakockák*ra!

   <details><summary markdown="span">Megoldás</summary>

   ```sql
   update Kategoria
   set Nev = 'Fakockák'
   where Nev = 'Fajáték'
   ```

   </details>

1. Melyik termék kategóriában van a legtöbb termék?

   <details><summary markdown="span">Megoldás</summary>

   ```sql
   select top 1 Nev, (select count(*) from Termek where Termek.KategoriaID = k.ID) as db
   from Kategoria k
   order by db desc
   ```

   A kérdésre több alternatív lekérdezés is eszünkbe juthat. Ez csak egyike a lehetséges megoldásoknak. Itt láthatunk példát az allekérdezésre is.

   </details>

## Feladat 2: Termékkategória rögzítése

Hozzon létre egy tárolt eljárást, aminek a segítségével egy új kategóriát vehetünk fel. Az eljárás bemenő paramétere a felvételre kerülő kategória neve, és opcionálisan a szülőkategória neve. Dobjon alkalmazás hibát, ha a kategória létezik, vagy a szülőkategória nem létezik. A kategória elsődleges kulcsának generálását bízza az adatbázisra.

<details><summary markdown="span">Megoldás</summary>

#### Tárolt eljárás

```sql
create procedure UjKategoria
    @Kategoria nvarchar(50),
    @SzuloKategoria nvarchar(50)
as

begin tran

declare @ID int
select @ID=ID
from kategoria with (TABLOCKX)
where upper(nev) = upper(@Kategoria)

if @ID is not null
begin
    rollback
    raiserror (' A %s kategoria mar letezik',16,1,@Kategoria)
    return
end

declare @SzuloKategoriaID int
if @SzuloKategoria is not null
begin
    select @SzuloKategoriaID = id
    from kategoria
    where upper(nev) = upper(@SzuloKategoria)

    if @SzuloKategoriaID is null
    begin
        rollback
        raiserror (' A %s kategoria nem letezik',16,1,@SzuloKategoria)
        return
    end
end

insert into Kategoria
values(@Kategoria,@SzuloKategoriaID)

commit
```

#### Tesztelés

Nyissunk egy új Query ablakot és adjuk ki az alábbi parancsot.

`exec UjKategoria 'Uszogumik', NULL`

Ennek sikerülnie kell. Ellenőrizzük utána a tábla tartalmát.

Ismételjük meg a fenti beszúrást, ekkor már hibák kell dobjon.

</details>

## Feladat 3: Megrendeléstétel státuszának karbantartása

Írjon triggert, ami a megrendelés státuszának változása esetén a hozzá tartozó egyes tételek státuszát a megfelelőre módosítja, ha azok régi státusza megegyezett a megrendelés régi státuszával. A többi tételt nem érinti a státusz változása.

<details><summary markdown="span">Megoldás</summary>

#### Tárolt eljárás

```sql
create trigger StatuszKarbantartas
on Megrendeles
for update
as

update Megrendelestetel
set StatuszID =i.StatuszID
from Megrendelestetel mt
inner join inserted i on i.Id=mt.MegrendelesID
inner join deleted d on d.ID=mt.MegrendelesID
where i.StatuszID != d.StatuszID
  and mt.StatuszID=d.StatuszID
```

Szánjunk egy kis időt az `update ... from` utasítás működési elvének megértésére. Az alapelvek a következők. Akkor használjuk, ha a módosítandó tábla bizonyos mezőit más tábla vagy táblák tartalma alapján szeretnénk beállítani. A szintaktika alapvetően a már megszokott `update ... set...` formát követi, kiegészítve egy `from` szakasszal, melyben már a `select from` utasításnál megismerttel azonos szintaktikával más táblákból illeszthetünk (`join`) adatokat a módosítandó táblához. Így a `set` szakaszban az illesztett táblák oszlopai is felhasználhatók adatforrásként (vagyis állhatnak az = jobb oldalán).

#### Tesztelés

Ellenőrizzük a megrendelés és a tételek státuszát:

```sql
select megrendelestetel.statuszid, megrendeles.statuszid
from megrendelestetel join megrendeles on
megrendelestetel.megrendelesid=megrendeles.id
where megrendelesid = 1
```

Változtassuk meg a megrendelést:

```sql
update megrendeles
set statuszid=4
where id=1
```

Ellenőrizzük a megrendelést és a tételeket (update után minden
státusznak meg kell változnia):

```sql
select megrendelestetel.statuszid, megrendeles.statuszid
from megrendelestetel join megrendeles on
megrendelestetel.megrendelesid=megrendeles.id
where megrendelesid = 1
```

</details>

## Feladat 4: Vevő megrendeléseinek összegzése

Tároljuk el a vevő összes megrendelésének végösszegét a Vevő táblában!

1. Adjuk hozzá az a táblához az új oszlopot: `alter table vevo add vegosszeg float`
1. Számoljuk ki az aktuális végösszeget. A megoldáshoz használjunk kurzort, ami minden vevőn megy végig.

<details><summary markdown="span">Megoldás</summary>

```sql
declare cur_vevo cursor
    for select ID from Vevo
declare @vevoId int
declare @osszeg float

open cur_vevo
fetch next from cur_vevo into @vevoId
while @@FETCH_STATUS = 0
begin

    select @osszeg = sum(mt.Mennyiseg * mt.NettoAr)
    from Vevo v
    inner join Telephely t on v.ID=t.VevoID
    inner join Megrendeles m on m.TelephelyID=t.ID
    inner join MegrendelesTetel mt on mt.MegrendelesID=m.ID
    where v.ID = @vevoId

    update Vevo
    set vegosszeg = ISNULL(@osszeg, 0)
    where ID = @vevoId

    fetch next from cur_vevo into @vevoId
end

close cur_vevo
deallocate cur_vevo
```

</details>

## Feladat 5: Vevő összemegrendelésének karbantartása (önálló feladat)

Az előző feladatban kiszámolt érték az aktuális állapotot tartalmazza csak. Készítsünk triggert, amivel karbantartjuk azt az összeget minden megrendelést érintő változás esetén. Az összeg újraszámolása helyett csak frissítse a változásokkal az értéket!

<details><summary markdown="span">Megoldás</summary>

A megoldás kulcsa meghatározni, mely táblára kell a triggert tenni. A megrendelések változása érdekes számunkra, de valójában a végösszeg a megrendeléshez felvett tételek módosulásakor fog változni, így erre a táblára kell a trigger.

A feladat nehézségét az adja, hogy az `inserted` és `deleted` táblákban nem csak egy vevő adatai módosulhatnak. Egy lehetséges megoldás a korábban használt kurzoros megközelítés (itt a változásokon kell iterálni). Avagy megpróbálhatjuk megírni egy utasításban is, ügyelve arra, hogy vevők szerint csoportosítsuk a változásokat.

#### Trigger

```sql
create trigger VegosszegKarbatartas
on MegrendelesTetel
for insert, update, delete
as

update Vevo
set vegosszeg=isnull(vegosszeg,0) + OsszegValtozas
from Vevo
inner join
    (select v.ID, sum(mennyiseg * NettoAr) as OsszegValtozas
    from Vevo v
    inner join Telephely t on v.ID=t.VevoID
    inner join Megrendeles m on m.TelephelyID=t.ID
    inner join inserted i on i.MegrendelesID=m.ID
    group by v.ID) VevoValtozas on Vevo.ID = VevoValtozas.ID

update Vevo
set vegosszeg=isnull(vegosszeg,0) - OsszegValtozas
from Vevo
inner join
    (select v.ID, sum(mennyiseg * NettoAr) as OsszegValtozas
    from Vevo v
    inner join Telephely t on v.ID=t.VevoID
    inner join Megrendeles m on m.TelephelyID=t.ID
    inner join deleted d on d.MegrendelesID=m.ID
    group by v.id) VevoValtozas on Vevo.ID = VevoValtozas.ID
```

#### Tesztelés

Nézzük meg az összmegrendelések aktuális értékét, jegyezzük meg a
számokat.

```sql
select id, osszmegrendeles
from vevo
```

Módosítsunk egy megrendelés mennyiségén.

```sql
update megrendelestetel
set mennyiseg=3
where id=1
```

Nézzük meg az összegeket ismét, meg kellett változnia a számnak.

```sql
select id, osszmegrendeles
from vevo
```

</details>
