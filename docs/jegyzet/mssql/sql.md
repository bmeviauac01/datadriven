# SQL nyelv, MSSQL platformfüggő SQL utasítások

A példák a [minta adatbázison](../../db/index.md) futtathatóak.

## Egyszerű lekérdezések

Melyik termék olcsóbb mint 2000 és kevesebb, mint 50 db van belőle?

```sql
select Name, Price, Stock
from Product
where Price<2000 and Stock<50
```

Melyik termékhez nincs leírás?

```sql
select *
from Product
where Description is null
```

## Táblák összekapcsolása

Budapesti központi telephellyel rendelkező vevők (a két megoldás ekvivalens)

```sql
select *
from Customer c, CustomerSite s
where c.MainCustomerSiteID=s.ID and City='Budapest'

select *
from Customer c inner join CustomerSite s on c.MainCustomerSiteID=s.ID
where City='Budapest'
```

Listázza ki az M betűvel kezdődő termékek nevét és a megrendelt mennyiségeket úgy, hogy azok a termékek is benne legyenek a listában melyekből nem rendeltek meg semmit

```sql
select p.Name, oi.Amount
from Product p
     left outer join OrderItem oi on p.id=oi.ProductID
     left outer join [Order] o on o.ID=oi.OrderID
where p.Name like 'M%'
```

!!! info "`[Order]`"
    Az `[Order]` azért szerepel szögeletes zárójelben, mert így jelöljük, hogy ez egy tábla neve, és nem az `order by` parancs kezdete.

## Rendezés

```sql
select *
from Product
order by Name
```

Microsoft SQL Server specifikus: _collation_ a rendezés szabályait adja meg

```sql
select *
from Product
order by Name collate SQL_Latin1_General_Cp1_CI_AI
```

Több mező szerinti rendezés

```sql
select *
from Product
order by Stock desc, Price
```

## Allekérdezések

Listázzuk ki a megrendelések Statuszát határidejét és dátumát

```sql
select o.Date, o.Deadline, s.Name
from [Order] o inner join Status s on o.StatusId=s.ID
```

Alternatív, de nem ekvivalens megoldás: az allekérdezés az outer joinnak felel meg!

```sql
select o.Date, o.Deadline,
       (select s.Name
        from Status s
        where o.StatusId=s.ID)
from [Order] o
```

## Duplikátum szűrése

Melyek azok a termékek, melyből egyszerre több, mint 3 db-ot rendeltek? Ugyanabból a termékből több alkalommal is előfordulhatott, de csak egyszer szeretnénk a nevét látni.

```sql
select distinct p.Name
from Product p inner join OrderItem oi on oi.ProductID=p.ID
where oi.Amount>3
```

## Oszlopfüggvények

Mennyibe kerül a legdrágább termék?

```sql
select max(Price)
from Product
```

Melyek a legdrágább termékek?

```sql
select *
from Product
where Price=(select max(Price) from Product)
```

Azon termékeket min, max és átlag mennyiért adták el, melyek nevében szerepel a Lego és az átlag eladási áruk 10.000-nél nagyobb

```sql
select p.Id, p.Name, min(oi.Price), max(oi.Price), avg(oi.Price)
from Product p
     inner join OrderItem oi on p.ID=oi.ProductID
Where p.Name like '%Lego%'
group by p.Id, p.Name
having avg(oi.Price)>10000
order by 2
```

## Rekordok létrehozása

Egy rekord létrehozása minden oszlop (kivéve _identity_) adatának megadásával

```sql
insert into Product
values ('aa', 100, 0, 3, 2, null)
```

Csak megnevezett oszlopok értékeinek kitöltése

```sql
insert into Product (Name,Price)
values ('aa', 100)
```

Lekérdezés eredményeinek beszúrása

```sql
insert into Product (Name, Price)
select Name, Price
from InvoiceItem
where Amount>2
```

MSSQL specifikus: identity oszlop

```sql
create table VAT
(
   ID int identity primary key,
   Percentage int
)

insert into VAT(Percentage)
values (27)

select @@identity
```

MSSQL specifikus: értékadás _identity_ oszlopnak

```sql
set identity_insert VAT on

insert into VAT (ID, Percentage)
values (123, 27)

set identity_insert VAT off
```

## Rekordok módosítása

A Legók árát emeljük meg 10%-kal és a raktrákészletüket 5 db-bal

```sql
update Product
set Price=1.1*Price,
    Stock=Stock+5
where Name like '%Lego%'
```

Módosítás, ha kapcsolódó tábla alapján kell szűrni: emeljük meg 10%-kal azon 20%-os ÁFA kulcsú termékek árát, melyből, több mint 10 db van raktáron

```sql
update Product
set Price=1.1*Price
where Stock>10
and VATID in
(
    select ID
    from VAT
    where Percentage=20
)
```

MSSQL Server specifikus szintaktika az előzőre

```sql
update Product
set Price=1.1*Price
from Product p
     inner join VAT v on p.VATID=v.ID
where Stock>10
      and Percentage=20
```

## Rekordok törlése

```sql
delete
from Product
where ID>10
```

## Sorszámozás

Sorszámozás egy adott rendezés szerint

```sql
select p.*,
       rank() over (order by Name) as r,
       dense_rank() over (order by Name) as dr
from Product p
```

Sorszámozás csoportosításonként

```sql
select p.*
       ,rank() over (partition by CategoryID order by Name) as r
       ,dense_rank() over (partition by CategoryID order by Name) as dr
from Product p
```

## CTE (Common Table Expression)

Motiváció: allekérdezéssel nehezen áttekinthetővé válnak a lekérdezések

ABC sorrendben melyik az első három termék

```sql
select *
from
(
    select p.*
            ,rank() over (order by Name) as r
            ,dense_rank() over (order by Name) as dr
    from Product p
) a
where a.dr<=3
```

Ugyan az a lekérdezés CTE használatával

```sql
with q1
as
(
    select *
            ,rank() over (order by Name) as r
            ,dense_rank() over (order by Name) as dr
    from Product
)
select *
from q1
where q1.dr<3
```

Hány darabot adtak el a második legdrágább termékből?

```sql
with q
as
(
    select *
            , dense_rank() over (order by Price desc) as dr
    from Product
)
select q.ID, q.Name, sum(Amount)
from q
     inner join OrderItem oi on oi.ProductID=q.ID
where q.dr = 2
group by q.ID, q.Name
```

Lapozás: termékek listázása ABC sorrendben a 3. rekordól a 8. rekordig

```sql
with q
as
(
    select *
            , rank() over (order by Name) as r
    from Product
)
select *
from q
where q.r between 3 and 8
```

Lapozás: MSSQL Server (2012+) specifikus megoldás

```sql
select *
from Product
order by Name
offset 2 rows
fetch next 6 rows only

select top 3 *
from Product
order by Name
```
