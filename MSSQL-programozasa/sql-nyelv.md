# SQL nyelv, MSSQL platformfüggő SQL utasítások

A példák a minta adatbázison futtathatóak.

## Egyszerű lekérdezések

Melyik termék olcsóbb mint 2000 és kevesebb, mint 50 db van belőle?

```sql
select Nev, NettoAr,Raktarkeszlet
from Termek
where NettoAr<2000 and Raktarkeszlet<50
```

Melyik termékhez nincs leírás, de van hozzá kép?

```sql
select *
from Termek
where Leiras is null and Kep is not null
```

## Táblák összekapcsolása

Budapesti központi telephellyel rendelkező vevők (a két megoldás ekvivalens)

```sql
select *
from vevo v, telephely t
where v.KozpontiTelephely=t.ID and Varos='Budapest'

select *
from vevo v inner join telephely t on v.KozpontiTelephely=t.ID
where Varos='Budapest'
```

Listázza ki a M betűvel kezdődő termékek nevét és amegrendelt mennyiségeket úgy, hogy azok a termékek is benne legyenek a listában melyekből nem rendeltek meg semmit

```sql
select t.Nev, mt.Mennyiseg
from Termek t
     left outer join MegrendelesTetel mt on t.id=mt.TermekID
     left outer join Megrendeles m on m.ID=mt.MegrendelesID
where t.Nev like 'M%'
```

## Rendezés

```sql
select *
from Termek
order by Nev
```

Microsoft SQL Server specifikus: *collation* a rendezés szabályait adja meg

```sql
select *
from Termek
order by Nev collate SQL_Latin1_General_Cp1_CI_AI	  
```

Több mező szerinti rendezés

```sql
select * 
from Termek
order by Raktarkeszlet desc, NettoAr
```

## Allekérdezések

Listázzuk ki a megrendelések Statuszát határidejét és dátumát

```sql
select m.Datum, m.Hatarido, s.Nev
from Megrendeles m inner join Statusz s on m.StatuszId=s.ID
```

Alternatív, de nem ekvivalens megoldás: az allekérdezés az outer joinnak felel meg!

```sql
select m.Datum,m.Hatarido,
       (select s.Nev
        from Statusz s
        where m.StatuszId=s.ID)
from Megrendeles m
```

## Duplikátum szűrése

Melyek azok a termékek, melyből egyszerre több, mint 3 db-ot rendeltek? Ugyanabból a termékből több alkalommal is előfordulhatott, de csak egyszer szeretnénk a nevét látni.

```sql
select distinct t.Nev
from Termek t inner join MegrendelesTetel mt on mt.TermekID=t.ID
where mt.Mennyiseg>3
```

## Oszlopfüggvények

Mennyibe kerül a legdrágább termék?


```sql
select max(NettoAr)
from Termek
```

Melyek a legdrágább termékek?

```sql
select *
from Termek
where NettoAr=(select max(NettoAr) from Termek)
```

Azon termékeket min, max és átlag mennyiért adták el, melyek nevében szerepel a Lego és az átlag eladási áruk 10.000-nél nagyobb

```sql
select t.id, t.Nev, min (mt.NettoAr), max (mt.NettoAr), avg (mt.NettoAr)
from Termek t
     inner join MegrendelesTetel mt on t.ID=mt.TermekID
Where t.Nev like '%Lego%'
group by t.id, t.Nev
having avg (mt.NettoAr)>10000
order by 2
```

## Rekordok létrehozása

Egy rekord létrehozása minden oszlop (kivéve *identity*) adatának megadásával

```sql
insert into Termek
values ('aa', 100, 0, 3, 2, null, null)
```

Csak megnevezett oszlopok értékeinek kitöltése

```sql
insert into Termek (Nev,NettoAr)
values ('aa', 100)
```

Lekérdezés eredményeinek beszúrása

```sql
insert into Termek (Nev, NettoAr)
select Nev, NettoAr
from SzamlaTetel
where Mennyiseg>2
```

MSSQL specifikus: identity oszlop

```sql
create table AFA
(
   ID int identity primary key,
   Kulcs int
)

insert into AFA (Kulcs)
values (27)

select @@identity
```

MSSQL specifikus: értékadás *identity* oszlopnak

```sql
set identity_insert AFA on

insert into AFA (ID, Kulcs)
values (123, 27)

set identity_insert Termek off
```

## Rekordok módosítása

A Legók árát emeljük meg 10%-kal és a raktrákészletüket 5 db-bal

```sql
update Termek
set NettoAr=1.1*NettoAr,
    Raktarkeszlet=Raktarkeszlet+5
where Nev like '%Lego%'
```

Módosítás, ha kapcsolódó tábla alapján kell szűrni: emeljük meg 10%-kal azon 20%-os ÁFA kulcsú termékek árát, melyből, több mint 10 db van raktáron

```sql
update Termek
set NettoAr=1.1*NettoAr
where Raktarkeszlet>10
and AFAID in
(
    select ID
    from AFA
    where kulcs=20
)
```

MSSQL Server specifikus szintaktika az előzőre

```sql
update Termek
set NettoAr=1.1*NettoAR
from Termek t
     inner join AFA a on t.AFAID=a.ID
where Raktarkeszlet>10
      and kulcs=20
```

## Rekordok törlése

```sql
delete
from termek
where ID>10
```

## Sorszámozás

Sorszámozás egy adott rendezés szerint

```sql
select t.*,
       rank() over (order by Nev) as r,
       dense_rank() over (order by Nev) as dr
from termek t
```

Sorszámozás csoportosításonként

```sql
select t.*
       ,rank() over (partition by KategoriaID order by Nev) as r
       ,dense_rank() over (partition by KategoriaID order by Nev) as dr
from termek 
```

## CTE (Common Table Expression)

Motiváció: allkérdezésel nehezen áttekinthetővé válnak a lekérdezések

ABC sorrendben melyik az első három termék

```sql
select *
from
(
    select t.*
            ,rank() over (order by Nev) as r
            ,dense_rank() over (order by Nev) as dr
    from termek t
) a
where a.dr<=3
```

Ugyan az a lekérdezés CTE használatával

```sql
with q1
as
(
    select *
           ,rank() over (order by Nev) as r
          ,dense_rank() over (order by Nev) as dr
    from termek t
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
            , dense_rank() over (order by NettoAr desc) dr
    from termek
)
select q.ID,q.Nev,sum(Mennyiseg)
from q
     inner join megrendelestetel mt on mt.TermekID=q.ID
where q.dr = 2
group by q.ID, q.Nev
```

Lapozás: termékek listázása ABC sorrendben a 3. rekordól a 8. rekordig

```sql
with q
as
(
    select *
            , rank() over (order by Nev) r
    from termek
)
select *
from q
where q.r between 3 and 8
```

Lapozás: MSSQL Server (2012+) specifikus megoldás

```sql
select *
from termek
order by nev
offset 2 rows
fetch next 6 rows only

select top 3 *
from Termek
order by Nev
```
