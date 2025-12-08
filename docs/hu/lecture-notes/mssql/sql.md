# SQL nyelv, MSSQL platformfüggő SQL utasítások

A példák a [minta adatbázison](../../db/index.md) futtathatóak.

## Egyszerű lekérdezések

Melyik termék olcsóbb mint 2000 és kevesebb, mint 50 db van belőle?

```sql
SELECT Name, Price, Stock
FROM Product
WHERE Price<2000 AND Stock<50
```

Melyik termékhez nincs leírás?

```sql
SELECT *
FROM Product
WHERE Description IS NULL
```

## Táblák összekapcsolása

Budapesti központi telephellyel rendelkező vevők (a két megoldás ekvivalens)

```sql
SELECT *
FROM Customer c, CustomerSite s
WHERE c.MainCustomerSiteID=s.ID AND City='Budapest'

SELECT *
FROM Customer c INNER JOIN CustomerSite s ON c.MainCustomerSiteID=s.ID
WHERE City='Budapest'
```

Listázza ki az M betűvel kezdődő termékek nevét és a megrendelt mennyiségeket úgy, hogy azok a termékek is benne legyenek a listában melyekből nem rendeltek meg semmit

```sql
SELECT p.Name, SUM(oi.Amount)
FROM Product p
     LEFT OUTER JOIN OrderItem oi ON p.id=oi.ProductID
WHERE p.Name LIKE 'M%'
GROUP BY p.Name
```

## Rendezés

```sql
SELECT *
FROM Product
ORDER BY Name
```

Microsoft SQL Server specifikus: _collation_ a rendezés szabályait adja meg

```sql
SELECT *
FROM Product
ORDER BY Name collate SQL_Latin1_General_Cp1_CI_AI
```

Több mező szerinti rendezés

```sql
SELECT *
FROM Product
ORDER BY Stock DESC, Price
```

## Allekérdezések

Listázzuk ki a megrendelések dátumát, határidejét és Status-át

```sql
SELECT o.Date, o.Deadline, s.Name
FROM [Order] o INNER JOIN Status s ON o.StatusId=s.ID
```

Alternatív, de nem ekvivalens megoldás: az allekérdezés az outer joinnak felel meg!

```sql
SELECT o.Date, o.Deadline,
       (SELECT s.Name
        FROM Status s
        WHERE o.StatusId=s.ID)
FROM [Order] o
```

!!! info "`[Order]`"
    Az `[Order]` azért szerepel szögletes zárójelben, mert így jelöljük, hogy ez egy tábla neve, és nem az `order by` parancs kezdete.

## Duplikátum szűrése

Melyek azok a termékek, melyből egyszerre több, mint 3 db-ot rendeltek? Ugyanabból a termékből több alkalommal is előfordulhatott, de csak egyszer szeretnénk a nevét látni.

```sql
SELECT DISTINCT p.Name
FROM Product p INNER JOIN OrderItem oi ON oi.ProductID=p.ID
WHERE oi.Amount>3
```

## Oszlopfüggvények

Mennyibe kerül a legdrágább termék?

```sql
SELECT MAX(Price)
FROM Product
```

Melyek a legdrágább termékek?

```sql
SELECT *
FROM Product
WHERE Price=(SELECT MAX(Price) FROM Product)
```

Azon termékeket min, max és átlag mennyiért adták el, melyek nevében szerepel a Lego és az átlag eladási áruk 10.000-nél nagyobb

```sql
SELECT p.Id, p.Name, MIN(oi.Price), MAX(oi.Price), AVG(oi.Price)
FROM Product p
     INNER JOIN OrderItem oi ON p.ID=oi.ProductID
WHERE p.Name LIKE '%Lego%'
GROUP BY p.Id, p.Name
HAVING AVG(oi.Price)>10000
ORDER BY 2
```

## Rekordok létrehozása

Egy rekord létrehozása minden oszlop (kivéve _identity_) adatának megadásával

```sql
INSERT INTO Product
VALUES ('aa', 100, 0, 3, 2, NULL)
```

Csak megnevezett oszlopok értékeinek kitöltése

```sql
INSERT INTO Product (Name,Price)
VALUES ('aa', 100)
```

Lekérdezés eredményeinek beszúrása

```sql
INSERT INTO Product (Name, Price)
SELECT Name, Price
FROM InvoiceItem
WHERE Amount>2
```

MSSQL specifikus: identity oszlop

```sql
CREATE TABLE VAT
(
   ID int IDENTITY PRIMARY KEY,
   Percentage int
)

INSERT INTO VAT(Percentage)
VALUES (27)

SELECT @@IDENTITY
```

MSSQL specifikus: értékadás _identity_ oszlopnak

```sql
SET identity_insert VAT ON

INSERT INTO VAT (ID, Percentage)
VALUES (123, 27)

SET identity_insert VAT off
```

## Rekordok módosítása

A Legók árát emeljük meg 10%-kal és a raktárkészletünket 5 db-bal

```sql
UPDATE Product
SET Price=1.1*Price,
    Stock=Stock+5
WHERE Name LIKE '%Lego%'
```

Módosítás, ha kapcsolódó tábla alapján kell szűrni: emeljük meg 10%-kal azon 20%-os ÁFA kulcsú termékek árát, melyből, több mint 10 db van raktáron

```sql
UPDATE Product
SET Price=1.1*Price
WHERE Stock>10
AND VATID IN
(
    SELECT ID
    FROM VAT
    WHERE Percentage=20
)
```

MSSQL Server specifikus szintaktika az előzőre

```sql
UPDATE Product
SET Price=1.1*Price
FROM Product p
     INNER JOIN VAT v ON p.VATID=v.ID
WHERE Stock>10
      AND Percentage=20
```

## Rekordok törlése

```sql
DELETE
FROM Product
WHERE ID>10
```

## Sorszámozás

Sorszámozás egy adott rendezés szerint

```sql
SELECT p.*,
       rank() over (ORDER BY Name) AS r,
       dense_rank() over (ORDER BY Name) AS dr
FROM Product p
```

Sorszámozás csoportosításonként

```sql
SELECT p.*
       ,rank() over (partition BY CategoryID ORDER BY Name) AS r
       ,dense_rank() over (partition BY CategoryID ORDER BY Name) AS dr
FROM Product p
```

!!! example "Rank és dense_rank"
    A dense_rank-tól eltérően a rank kihagy sorszámokat az egyenlő helyezés után. Az átugrott sorszámok száma attól függ, hogy hány sor kapott azonos rangot. Például Mary és Lisa ugyanannyi terméket adott el, így mindkettő sorszáma 1. A rank-kal a következő sorszám a 3, míg dense_rank esetén a következő sorszám a 2.

## CTE (Common Table Expression)

Motiváció: allekérdezéssel nehezen áttekinthetővé válnak a lekérdezések

ABC sorrendben melyik az első három termék

```sql
SELECT *
FROM
(
    SELECT p.*
            ,rank() over (ORDER BY Name) AS r
            ,dense_rank() over (ORDER BY Name) AS dr
    FROM Product p
) a
WHERE a.dr<=3
```

Ugyan az a lekérdezés CTE használatával

```sql
WITH q1
AS
(
    SELECT *
            ,rank() over (ORDER BY Name) AS r
            ,dense_rank() over (ORDER BY Name) AS dr
    FROM Product
)
SELECT *
FROM q1
WHERE q1.dr<=3
```

Hány darabot adtak el a második legdrágább termékből?

```sql
WITH q
AS
(
    SELECT *
            , dense_rank() over (ORDER BY Price DESC) AS dr
    FROM Product
)
SELECT q.ID, q.Name, SUM(Amount)
FROM q
     INNER JOIN OrderItem oi ON oi.ProductID=q.ID
WHERE q.dr = 2
GROUP BY q.ID, q.Name
```

Lapozás: termékek listázása ABC sorrendben a 3. rekordól a 8. rekordig

```sql
WITH q
AS
(
    SELECT *
            , rank() over (ORDER BY Name) AS r
    FROM Product
)
SELECT *
FROM q
WHERE q.r BETWEEN 3 AND 8
```

Lapozás: MSSQL Server (2012+) specifikus megoldás

```sql
SELECT *
FROM Product
ORDER BY Name
offset 2 rows
fetch next 6 rows only

SELECT TOP 3 *
FROM Product
ORDER BY Name
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
SELECT Description.query('/product/package_parameters/number_of_packages')
FROM Product
```

Ennek az eredménye például a következő lehet:

```xml
<number_of_packages>1</number_of_packages>
```

A `query()` XML-lel tér vissza, ha csak az értékre van szükség, akkor a `value()` metódust használhatjuk. A `value()` metódusnak meg kell adni a lekérdezett adat típusát is string literálként.

```sql
SELECT Description.value('(/product/package_parameters/number_of_packages)[1]', 'int')
FROM Product
```

Ennek az eredménye már az 1 lesz számként.

!!! info "SQLType"
    A paraméterként átadott típus nem lehet xml. A megadott típusra való konvertálás T-SQL [`CONVERT`](https://docs.microsoft.com/en-us/sql/t-sql/functions/cast-and-convert-transact-sql) függvénnyel történik.

Kérdezzük le azoknak a termékeknek a nevét, amelyek a 0-18 hónapos korosztálynak ajánlottak.

```sql
SELECT Name
FROM Product
WHERE Description.exist('(/product)[(./recommended_age)[1] eq "0-18 m"]')=1
```

Az `exist()` 1-gyel tér vissza, ha a megadott _XQuery_ kifejezéssel futtatott lekérdezés nem üres eredménnyel tér vissza; vagy 0-val, amennyiben a lekérdezés eredménye üres.

A lekérdezést `exist()` helyett `value()` metódus segítségével is megfogalmazhatjuk.

```sql
SELECT Name
FROM Product
WHERE Description.value('(/product/recommended_age)[1]', 'varchar(MAX)')='0-18 m'
```

### Manipuláló lekérdezések

Nem csak lekérdezni tudunk XML adatokat, hanem módosítani is. A módosítás az adatbázisban atomi módon történik, azaz nem kell kliens oldalra letölteni az XML-t, módosítani, majd visszatölteni. Helyette a szerveroldali programozás filozófiáját követve a logikát (itt: módosítás) visszük az adatbázisba. Az adatmódosító lekérdezéseket a [`modify(XML_DML)`](https://docs.microsoft.com/en-us/sql/t-sql/xml/modify-method-xml-data-type) függvénnyel hajthatjuk végre, ahol is az ún. [XML DML](https://docs.microsoft.com/en-us/sql/t-sql/xml/xml-data-modification-language-xml-dml) nyelven kell megfogalmaznunk a módosításunkat. Nézzünk erre is pár példát.

Az Lego City harbour nevű terméknél az ajánlott életkort írjuk át 6-99 évre.

```sql
UPDATE Product
SET Description.modify(
'replace value of (/product/recommended_age/text())[1]
WITH "6-99 y"')
WHERE Name='Lego City harbour'
```

A megadandó kifejezés két részből áll: az elsőben (`replace value of`) kell a módosítani kívánt elemet kell kiválasztani, a másodikban (`with`) az új értéket kell megadni. Egy XML-en belül csak egy elem módosítható, így az útvonalat úgy kell megadni, hogy csak egy elemre illeszkedjen - ezért szerepel példában a végén az `[1]`.

Szúrjunk be a Lego City harbour termékhez a `package_size` tag után egy `weight` tag-et a súly megadására.

```sql
UPDATE Product
SET Description.modify(
'INSERT <weight>2.28</weight>
after (/product/package_parameters/package_size)[1]')
WHERE Name='Lego City harbour'
```

A megadandó kifejezés itt is két részből áll: az elsőben (`insert`) kell megadni az új elemet, másodikban kell leírni azt, hogy hova szúrja be az új elemet. Az új elemet fel lehet venni a megadott elem testvéreként vagy gyerekeként.

Töröljük minden termék leírásából a `description` tag(ek)-et.

```sql
UPDATE Product
SET Description.modify('DELETE /product/description')
WHERE Description IS NOT NULL
```

A törlésnél a `delete` után meg kell adni a törlendő elemek útvonalát.
