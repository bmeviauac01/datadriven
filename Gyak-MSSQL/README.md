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
   from [Order] o join Status s on o.StatusID = s.ID
   where s.Name != 'Delivered'
   ```

   A `join` mellett az oszlopfüggvény (aggregáció) használatára látunk példát. (A táblák kapcsolására nem csak ez a szintaktika használható, előadáson szerepelt alternatív is.)

   </details>

1. Melyek azok a fizetési módok, amit soha nem választottak a megrendelőink?

   <details><summary markdown="span">Megoldás</summary>

   ```sql
   select p.Method
   from [Order] o right outer join PaymentMethod p on o.PaymentMethodID = p.ID
   where o.ID is null
   ```

   A megoldás kulcsa az `outer join`, aminek köszönhetően láthatjuk, mely fizetési mód rekordhoz _nem_ tartozik egyetlen megrendelés se.

   </details>

1. Rögzítsünk be egy új vevőt! Kérdezzük le az újonnan létrejött rekord kulcsát!

   <details><summary markdown="span">Megoldás</summary>

   ```sql
   insert into Customer(Name, Login, Password, Email)
   values ('Teszt Elek', 't.elek', '********', 't.elek@email.com')

   select @@IDENTITY
   ```

   Az `insert` után javasolt kiírni az oszlopneveket az egyértelműség végett, bár nem kötelező. Vegyük észre, hogy az ID oszlopnak nem adunk értéket, mert azt a tábla definíciójakor meghatározva a szerver adja automatikusan. Ezért kell utána lekérdeznünk, hogy tudjuk, milyen ID-t adott.

   </details>

1. A kategóriák között hibásan szerepel az _Bicycles_ kategória név. Javítsuk át a kategória nevét _Tricycles_-re!

   <details><summary markdown="span">Megoldás</summary>

   ```sql
   update Category
   set Nev = 'Bicycles'
   where Nev = 'Tricycles'
   ```

   </details>

1. Melyik termék kategóriában van a legtöbb termék?

   <details><summary markdown="span">Megoldás</summary>

   ```sql
   select top 1 Name, (select count(*) from Product where Product.CategoryID = c.ID) as cnt
   from Category c
   order by cnt desc
   ```

   A kérdésre több alternatív lekérdezés is eszünkbe juthat. Ez csak egyike a lehetséges megoldásoknak. Itt láthatunk példát az allekérdezésre is.

   </details>

## Feladat 2: Termékkategória rögzítése

Hozzon létre egy tárolt eljárást, aminek a segítségével egy új kategóriát vehetünk fel. Az eljárás bemenő paramétere a felvételre kerülő kategória neve, és opcionálisan a szülőkategória neve. Dobjon alkalmazás hibát, ha a kategória létezik, vagy a szülőkategória nem létezik. A kategória elsődleges kulcsának generálását bízza az adatbázisra.

<details><summary markdown="span">Megoldás</summary>

#### Tárolt eljárás

```sql
create procedure AddNewCategory
    @Name nvarchar(50),
    @ParentName nvarchar(50)
as

begin tran

declare @ID int
select @ID = ID
from Category with (TABLOCKX)
where upper(Name) = upper(@Name)

if @ID is not null
begin
    rollback
    raiserror ('Category %s alredy exists',16,1,@Name)
    return
end

declare @ParentID int
if @ParentName is not null
begin
    select @ParentID = ID
    from Category
    where upper(Name) = upper(@ParentName)

    if @ParentID is null
    begin
        rollback
        raiserror ('Category %s does not exist',16,1,@ParentName)
        return
    end
end

insert into Category
values(@Name,@ParentID)

commit
```

#### Tesztelés

Nyissunk egy új Query ablakot és adjuk ki az alábbi parancsot.

`exec AddNewCategory 'Beach balls', NULL`

Ennek sikerülnie kell. Ellenőrizzük utána a tábla tartalmát.

Ismételjük meg a fenti beszúrást, ekkor már hibák kell dobjon.

</details>

## Feladat 3: Megrendeléstétel státuszának karbantartása

Írjon triggert, ami a megrendelés státuszának változása esetén a hozzá tartozó egyes tételek státuszát a megfelelőre módosítja, ha azok régi státusza megegyezett a megrendelés régi státuszával. A többi tételt nem érinti a státusz változása.

<details><summary markdown="span">Megoldás</summary>

#### Tárolt eljárás

```sql
create trigger UpdateOrderStatus
on [Order]
for update
as

update OrderItem
set StatusID = i.StatusID
from OrderItem oi
inner join inserted i on i.Id=oi.OrderID
inner join deleted d on d.ID=oi.OrderID
where i.StatusID != d.StatusID
  and oi.StatusID=d.StatusID
```

Szánjunk egy kis időt az `update ... from` utasítás működési elvének megértésére. Az alapelvek a következők. Akkor használjuk, ha a módosítandó tábla bizonyos mezőit más tábla vagy táblák tartalma alapján szeretnénk beállítani. A szintaktika alapvetően a már megszokott `update ... set...` formát követi, kiegészítve egy `from` szakasszal, melyben már a `select from` utasításnál megismerttel azonos szintaktikával más táblákból illeszthetünk (`join`) adatokat a módosítandó táblához. Így a `set` szakaszban az illesztett táblák oszlopai is felhasználhatók adatforrásként (vagyis állhatnak az egyenlőség jobb oldalán).

#### Tesztelés

Ellenőrizzük a megrendelés és a tételek státuszát:

```sql
select OrderItem.StatusID, [Order].StatusID
from OrderItem join [Order] on OrderItem.OrderID=[Order].ID
where OrderID = 1
```

Változtassuk meg a megrendelést:

```sql
update [Order]
set StatusID=4
where ID=1
```

Ellenőrizzük a megrendelést és a tételeket (update után minden
státusznak meg kell változnia):

```sql
select OrderItem.StatusID, [Order].StatusID
from OrderItem join [Order] on OrderItem.OrderID=[Order].ID
where OrderID = 1
```

</details>

## Feladat 4: Vevő megrendeléseinek összegzése

Tároljuk el a vevő összes megrendelésének végösszegét a Vevő táblában!

1. Adjuk hozzá az a táblához az új oszlopot: `alter table Customer add Total float`
1. Számoljuk ki az aktuális végösszeget. A megoldáshoz használjunk kurzort, ami minden vevőn megy végig.

<details><summary markdown="span">Megoldás</summary>

```sql
declare cur_customer cursor
    for select ID from Customer
declare @CustomerId int
declare @Total float

open cur_customer
fetch next from cur_customer into @CustomerId
while @@FETCH_STATUS = 0
begin

    select @Total = sum(oi.Amount * oi.Price)
    from CustomerSite s
    inner join [Order] o on o.CustomerSiteID=s.ID
    inner join OrderItem oi on oi.OrderID=o.ID
    where s.CustomerID = @CustomerId

    update Customer
    set Total = ISNULL(@Total, 0)
    where ID = @CustomerId

    fetch next from cur_customer into @CustomerId
end

close cur_customer
deallocate cur_customer
```

</details>

## Feladat 5: Vevő összemegrendelésének karbantartása (önálló feladat)

Az előző feladatban kiszámolt érték az aktuális állapotot tartalmazza csak. Készítsünk triggert, amivel karbantartjuk azt az összeget minden megrendelést érintő változás esetén. Az összeg újraszámolása helyett csak frissítse a változásokkal az értéket!

<details><summary markdown="span">Megoldás</summary>

A megoldás kulcsa meghatározni, mely táblára kell a triggert tenni. A megrendelések változása érdekes számunkra, de valójában a végösszeg a megrendeléshez felvett tételek módosulásakor fog változni, így erre a táblára kell a trigger.

A feladat nehézségét az adja, hogy az `inserted` és `deleted` táblákban nem csak egy vevő adatai módosulhatnak. Egy lehetséges megoldás a korábban használt kurzoros megközelítés (itt a változásokon kell iterálni). Avagy megpróbálhatjuk megírni egy utasításban is, ügyelve arra, hogy vevők szerint csoportosítsuk a változásokat.

#### Trigger

```sql
create trigger CustomerTotalUpdate
on OrderItem
for insert, update, delete
as

update Customer
set Total=isnull(Total,0) + TotalChange
from Customer
inner join
    (select s.CustomerId, sum(Amount * Price) as TotalChange
    from CustomerSite s
    inner join [Order] o on o.CustomerSiteID=s.ID
    inner join inserted i on i.OrderID=o.ID
    group by s.CustomerId) CustomerChange on Customer.ID = CustomerChange.CustomerId

update Customer
set Total=isnull(Total,0) - TotalChange
from Customer
inner join
    (select s.CustomerId, sum(Amount * Price) as TotalChange
    from CustomerSite s
    inner join [Order] o on o.CustomerSiteID=s.ID
    inner join deleted d on d.OrderID=o.ID
    group by s.CustomerID) CustomerChange on Customer.ID = CustomerChange.CustomerId
```

#### Tesztelés

Nézzük meg az összmegrendelések aktuális értékét, jegyezzük meg a számokat.

```sql
select ID, Total
from Customer
```

Módosítsunk egy megrendelés mennyiségén.

```sql
update OrderItem
set Amount=3
where ID=1
```

Nézzük meg az összegeket ismét, meg kellett változnia a számnak.

```sql
select ID, Total
from Customer
```

</details>
