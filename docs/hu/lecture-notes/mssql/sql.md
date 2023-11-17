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
select p.Name, Sum(oi.Amount)
from Product p
     left outer join OrderItem oi on p.id=oi.ProductID
where p.Name like 'M%'
group by p.Name
```

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

Listázzuk ki a megrendelések dátumát, határidejét és Status-át

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

!!! info "`[Order]`"
    Az `[Order]` azért szerepel szögletes zárójelben, mert így jelöljük, hogy ez egy tábla neve, és nem az `order by` parancs kezdete.

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

A Legók árát emeljük meg 10%-kal és a raktárkészletünket 5 db-bal

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

!!! example "Rank és dense_rank"
    A dense_rank-tól eltérően a rank kihagy sorszámokat az egyenlő helyezés után. Az átugrott sorszámok száma attól függ, hogy hány sor kapott azonos rangot. Például Mary és Lisa ugyanannyi terméket adott el, így mindkettő sorszáma 1. A rank-kal a következő sorszám a 3, míg dense_rank esetén a következő sorszám a 2.

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
where q1.dr<=3
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

## XML dokumentumok lekérdezése

Egy relációs adatbázisban a relációs adatok mellett félig strukturált adatokat (pl.: XML) is eltárolhatunk, viszont a relációs a fő tartalom. A [minta adatbázisban](../../db/index.md) a `Product` tábla `Description` mezője XML formátumú.

### XPath

Egy XML dokumentum fa struktúrájú. Az [**XPath**](https://www.w3schools.com/xml/xpath_intro.asp) nyelv segítségével navigálhatunk a fában és kiválaszthatunk csomópontokat megadott szűrési szempontok alapján. Az alábbi táblázat szemlélteti az _XPath_ nyelv működését és képességeit

| **XPath kifejezés**           | **Jelentés**                                                                       |
| ----------------------------- | ---------------------------------------------------------------------------------- |
| tagnév                        | Csomópont névvel megadva                                                           |
| /                             | A gyökértől kezdődik a keresés                                                     |
| //                            | Aktuális csomóponttól kezdve bármely leszármazottban                               |
| .                             | Aktuális csomópont                                                                 |
| ..                            | Szülő csomópont                                                                    |
| @nev                          | Adott nevű attribútum                                                              |
| /konyvtar/konyv[k]            | A k. konyv gyerek a konyvtar elemen belül (1-től kezdődik az indexelés)            |
| /konyvtar/konyv[last()]       | Utolsó gyerek                                                                      |
| /konyvtar/konyv[position()<k] | Az első k-1 gyerek                                                                 |
| //cim[@nyelv="hu"]            | Azok a cim elemek, amelyeknek van "hu" értékű nyelv attribútuma                    |
| //cim[text()]                 | A cim elemek szövege (a tag-ek közötti rész)                                       |
| /konyvtar/konyv[ar>5000]      | Azok a konyvtar elemek belüli konyv elemek, amelyeknek az ar gyereke legalább 5000 |

!!! note "XQuery és XPath"
    Az _XPath_ a fentiek mellett még sok más képességgel is rendelkezik, sokkal bonyolultabb lekérdezésekre is képes.

    A további példákban [_XQuery_](https://docs.microsoft.com/en-us/sql/t-sql/xml/xml-data-modification-language-xml-dml) nyelvet használva fogjuk megadni a lekérdezendő adatokat. Az _XQuery_ az _XPath_-re épül és egészíti ki azt további funkciókkal. Mind az _XPath_, mind az _XQuery_ platformfüggetlen, W3C standardokra épülő nyelv.

### Lekérdezések

Adott tehát egy olyan tábla, amiben van egy XML típusú mező. Amellett, hogy a mező teljes értékét lekérdezhetjük, a tartalmára is képesek vagyunk lekérdezéseket megfogalmazni. Az XML dokumentumokban való lekérdezéshez az XML adattípuson definiált [`query(XQuery)`](https://docs.microsoft.com/en-us/sql/t-sql/xml/query-method-xml-data-type), [`value(XQuery, SQLType)`](https://docs.microsoft.com/en-us/sql/t-sql/xml/value-method-xml-data-type) és [`exist(XQuery)`](https://docs.microsoft.com/en-us/sql/t-sql/xml/exist-method-xml-data-type) T-SQL függvényt használhatjuk. Nézzünk ezekre pár példát.

Kérdezzük le, hogy hány csomagból állnak a termékek!

```sql
select Description.query('/product/package_parameters/number_of_packages')
from Product
```

Ennek az eredménye például a következő lehet:

```xml
<number_of_packages>1</number_of_packages>
```

A `query()` XML-lel tér vissza, ha csak az értékre van szükség, akkor a `value()` metódust használhatjuk. A `value()` metódusnak meg kell adni a lekérdezett adat típusát is string literálként.

```sql
select Description.value('(/product/package_parameters/number_of_packages)[1]', 'int')
from Product
```

Ennek az eredménye már az 1 lesz számként.

!!! info "SQLType"
    A paraméterként átadott típus nem lehet xml. A megadott típusra való konvertálás T-SQL [`CONVERT`](https://docs.microsoft.com/en-us/sql/t-sql/functions/cast-and-convert-transact-sql) függvénnyel történik.

Kérdezzük le azoknak a termékeknek a nevét, amelyek a 0-18 hónapos korosztálynak ajánlottak.

```sql
select Name
from Product
where Description.exist('(/product)[(./recommended_age)[1] eq "0-18 m"]')=1
```

Az `exist()` 1-gyel tér vissza, ha a megadott _XQuery_ kifejezéssel futtatott lekérdezés nem üres eredménnyel tér vissza; vagy 0-val, amennyiben a lekérdezés eredménye üres.

A lekérdezést `exist()` helyett `value()` metódus segítségével is megfogalmazhatjuk.

```sql
select Name
from Product
where Description.value('(/product/recommended_age)[1]', 'varchar(max)')='0-18 m'
```

### Manipuláló lekérdezések

Nem csak lekérdezni tudunk XML adatokat, hanem módosítani is. A módosítás az adatbázisban atomi módon történik, azaz nem kell kliens oldalra letölteni az XML-t, módosítani, majd visszatölteni. Helyette a szerveroldali programozás filozófiáját követve a logikát (itt: módosítás) visszük az adatbázisba. Az adatmódosító lekérdezéseket a [`modify(XML_DML)`](https://docs.microsoft.com/en-us/sql/t-sql/xml/modify-method-xml-data-type) függvénnyel hajthatjuk végre, ahol is az ún. [XML DML](https://docs.microsoft.com/en-us/sql/t-sql/xml/xml-data-modification-language-xml-dml) nyelven kell megfogalmaznunk a módosításunkat. Nézzünk erre is pár példát.

Az Lego City harbour nevű terméknél az ajánlott életkort írjuk át 6-99 évre.

```sql
update Product
set Description.modify(
'replace value of (/product/recommended_age/text())[1]
with "6-99 y"')
where Name='Lego City harbour'
```

A megadandó kifejezés két részből áll: az elsőben (`replace value of`) kell a módosítani kívánt elemet kell kiválasztani, a másodikban (`with`) az új értéket kell megadni. Egy XML-en belül csak egy elem módosítható, így az útvonalat úgy kell megadni, hogy csak egy elemre illeszkedjen - ezért szerepel példában a végén az `[1]`.

Szúrjunk be a Lego City harbour termékhez a `package_size` tag után egy `weight` tag-et a súly megadására.

```sql
update Product
set Description.modify(
'insert <weight>2.28</weight>
after (/product/package_parameters/package_size)[1]')
where Name='Lego City harbour'
```

A megadandó kifejezés itt is két részből áll: az elsőben (`insert`) kell megadni az új elemet, másodikban kell leírni azt, hogy hova szúrja be az új elemet. Az új elemet fel lehet venni a megadott elem testvéreként vagy gyerekeként.

Töröljük minden termék leírásából a `description` tag(ek)-et.

```sql
update Product
set Description.modify('delete /product/description')
where Description is not null
```

A törlésnél a `delete` után meg kell adni a törlendő elemek útvonalát.
