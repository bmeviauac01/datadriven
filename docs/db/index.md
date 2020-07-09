# Minta adatbázis sémája

A félév során a gyakorlati példákat egy egységes mintapéldán keresztül szemléltetjük. A mintapélda egy egyszerű vevő-megrendelésnyilvántartási rendszer. Az alábbi leírás a Microsoft SQL Server relációs sémáját ismerteti, a MongoDB "séma" ennek átültetése.

## Az adatbázis kontextusa

A rendszer termékek értékesítési folyamatának a követésére szolgál. A termékeket (_product_) kategóriákba (_category_) lehet sorolni, mely kategóriák hierarchikusan egymásra épülhetnek. A vevők (_customer_) megrendeléseiket (_order_) és azok státuszát (_status_) nyomon tudják követni.

Az ügyfeleken több telephelyük (_customer site_) is lehet, az egyes megrendelések (_order_) feladásakor, tisztázni kell, hogy az ügyfél mely telephelyére történik meg a kiszállítás. Minden vevőnek kell rendelkeznie központi telephellyel, mely a számlázási címül is szolgál. Természetesen egy megrendeléshez több tétel (_order item_) is tartozhat, az egyes tételek státusza külön-külön is követhető, ezáltal a vevő látja, hogy esetleg mely termékre kell várnia. A kész megrendelésekről számlát (_invoice_) kell készíteni.

A számla (_invoice_) olyan bizonylat, melynek az adatai később nem változtathatók, valamint az első nyomtatást követően csak számlamásolatot lehet csak kiállítani. Figyelembe kell venni azt is, hogy az egyes termékek ÁFA besorolása (_VAT_ = _value added tax_) ill. ÁFA kulcsa megváltozhat az idők során, viszont a kiállított számlákon természetesen már ez az információ sem változhat meg.

## Adatmodell

Az alábbi ábra szemlélteti a nyilvántartórendszer adatmodelljét.

![Adatmodell](images/db.png)

### Táblák és attribútumok

| **Tábla**     | **Oszlop**         | **Leírás**                                                                                                                           |
| ------------- | ------------------ | ------------------------------------------------------------------------------------------------------------------------------------ |
| VAT           | ID                 | Automatikusan generált azonosító, elsődleges kulcs.                                                                                  |
|               | Percentage         | ÁFA kulcs értéke százalékban megadva.                                                                                                |
| PaymentMethod | ID                 | Automatikusan generált azonosító, elsődleges kulcs.                                                                                  |
|               | Mode               | Fizetési mód megnevezése (pl.: Készpénz, Átutalás 8 napon belül).                                                                    |
|               | Deadline           | A fizetési módhoz tartozó határidő, azaz a számla teljesítési dátumához képest, hány nappal van később a fizetési határidő.          |
| Status        | ID                 | Automatikusan generált azonosító, elsődleges kulcs.                                                                                  |
|               | Name               | Megrendelés státusz megnevezése (pl.: új, feldolgozva, árura vár, csomagolva,…).                                                     |
| Category      | ID                 | Automatikusan generált azonosító, elsődleges kulcs                                                                                   |
|               | Name               | Termékkategória megnevezése (pl.: élelmiszer, tejtermék, …).                                                                         |
|               | ParentCategoryID   | Kategória hierarchiát leíró idegen kulcs, egy adott kategória szülőjére mutat. A gyökérelemeknél, a szülőkategória azonosítója NULL. |
| Product       | ID                 | Automatikusan generált azonosító, elsődleges kulcs.                                                                                  |
|               | Name               | Termék neve                                                                                                                          |
|               | Price              | Termék nettó ára                                                                                                                     |
|               | Stock              | A termékből a raktárban található mennyiség.                                                                                         |
|               | VATID              | Idegen kulcs a termék ÁFA kulcsára (VAT tábla).                                                                                      |
|               | CategoryID         | Idegen kulcs a termék kategóriájára (Category tábla).                                                                                |
|               | Description        | A termékhez tartozó XML formátumú leírás                                                                                             |
| Customer      | ID                 | Automatikusan generált azonosító, elsődleges kulcs.                                                                                  |
|               | Name               | Vevő megnevezése.                                                                                                                    |
|               | BankAccount        | Vevő bankszámla száma.                                                                                                               |
|               | Login              | Vevő login neve a webes rendszerhez.                                                                                                 |
|               | Password           | Vevő jelszava a webes rendszerhez.                                                                                                   |
|               | Email              | Vevő email címe.                                                                                                                     |
|               | MainCustomerSiteID | A vevő központi telephelyének azonosítója, külső kulcs a CustomerSite táblára.                                                       |
| CustomerSite  | ID                 | Automatikusan generált azonosító, elsődleges kulcs.                                                                                  |
|               | Zip                | A cím irányítószám része.                                                                                                            |
|               | City               | A cím város része.                                                                                                                   |
|               | Street             | A cím utca és házszám része.                                                                                                         |
|               | Tel                | A telephelyhez kapcsolódó telefonszám.                                                                                               |
|               | Fax                | A telephelyhez kapcsolódó fax szám.                                                                                                  |
|               | CustomerID         | Külső kulcs a vevőre (Customer tábla).                                                                                               |
| Order         | ID                 | Automatikusan generált azonosító, elsődleges kulcs.                                                                                  |
|               | Date               | Megrendelés dátuma.                                                                                                                  |
|               | Deadline           | Vállalt szállítási határidő.                                                                                                         |
|               | CustomerSiteID     | Külső kulcs a vevő telephelyére, ide kell kiszállítani a megrendelt árukat (CustomerSite tábla).                                     |
|               | StatusID           | Külső kulcs a státuszra, ez mutatja, hogy mi a teljes státusza a megrendelésnek (Status tábla).                                      |
|               | PaymentMethodID    | Külső kulcs a fizetési módra. A megrendeléshez tartozó számlát az itt megadott módon fogják kiegyenlíteni (PaymentMethod tábla).     |
| OrderItem     | ID                 | Automatikusan generált azonosító, elsődleges kulcs.                                                                                  |
|               | Amount             | Mennyiség, azaz az adott áruból ennyi darabot rendeltek meg.                                                                         |
|               | Price              | Egy egység nettó ára. Alapértelmezésként az termékben található nettó ár másolódik ide, de ettől eltérhet az értékesítő.             |
|               | OrderID            | Idegen kulcs a megrendelésre, azaz ez azonosítja, hogy az adott tétel mely megrendeléshez tartozik (Order tábla).                    |
|               | ProductID          | Idegen kulcs a Product táblára, ez azonosítja a megrendelt terméket.                                                                 |
|               | StatusID           | Idegen kulcs a Status táblára, ezzel lehet leírni a megrendelés tétel státuszát.                                                     |
| InvoiceIssuer | ID                 | Automatikusan generált azonosító, elsődleges kulcs                                                                                   |
|               | Name               | Cégnév, aki a kereskedést folytatja, ez szerepel a számlán.                                                                          |
|               | Zip                | A cím irányítószám része.                                                                                                            |
|               | City               | A cím város része.                                                                                                                   |
|               | Street             | A cím utca része.                                                                                                                    |
|               | TaxIdentifier      | A cég adószáma.                                                                                                                      |
|               | BankAccount        | A cég bankszámlaszáma.                                                                                                               |
| Invoice       | ID                 | Automatikusan generált azonosító, elsődleges kulcs                                                                                   |
|               | CustomerNev        | Megrendelő neve, ez az információ fog a számla vevő részén megjelenni.                                                               |
|               | CustomerZipCode    | A vevő címének irányítószáma.                                                                                                        |
|               | CustomerCity       | A vevő címének város része.                                                                                                          |
|               | CustomerStreet     | A vevő címének utca része.                                                                                                           |
|               | PrintedCopies      | A számla hányszor lett kinyomtatva.                                                                                                  |
|               | Cancelled          | A számla sztornózva lett-e?                                                                                                          |
|               | PaymentMethod      | A számla fizetési módja.                                                                                                             |
|               | CreationDate       | A számla kiállításának kelte.                                                                                                        |
|               | DeliveryDate       | A számla teljesítési dátuma.                                                                                                         |
|               | PaymentDeadline    | A számla fizetési határideje.                                                                                                        |
|               | InvoiceIssuerID    | Idegen kulcs a számla kiállítóra (InvoiceIssuer tába).                                                                               |
|               | OrderID            | Idegen kulcs a megrendelésre, a számla ezen megrendelés alapján került kiállításra (Order tábla).                                    |
| InvoiceItem   | ID                 | Automatikusan generált azonosító, elsődleges kulcs                                                                                   |
|               | Name               | Termék neve, mely a számlatételben szerepel.                                                                                         |
|               | Amount             | A vásárolt mennyiség.                                                                                                                |
|               | Price              | A tétel nettó egységára.                                                                                                             |
|               | VATPercentage      | A tétel ÁFA kulcsa                                                                                                                   |
|               | InvoiceID          | Idegen kulcs a számlára, melyhez a a számlatétel tartozik (Order tábla).                                                             |
|               | OrderItemID        | Idegen kulcs a megrendelés tételre (OrderItem tábla), melyből a számlatétel keletkezett.                                             |

### Sajátosságok

#### Számlázás

Adatmodell sajátossága, a számlázási adatok tárolása. A számla adatait nem lehet megváltoztatni kinyomtatás után, számlát nem lehet törölni csak sztornózni. Ebből adódón a számlához tartozó összes információt az `Invoice` és az `InvoiceItem` táblák tartalmazzák, a számlakiállítás során minden információt le kell másolni a vonatkozó megrendelésből. Valamint az első nyomtatást követően a számlából már csak másolatot lehet nyomtatni, eredeti példányt nem.

#### Számla kiállító

Speciális a `InvoiceIssuerID` attribútum az `Invoice` táblában, mivel a cég saját adatai ritkán változnak. Viszont a változtathatatlanság követelménye miatt a számla kiállító adatait nem lehet módosítani, ha már van hozzá számla. Ebben az esetben a számla kiállító táblába új rekordot kell felvenni, ebből adódóan az aktuális cégadatokat minidig a legmagasabb ID-vel rendelkező számla kiállító rekord tartalmazza.

#### ÁFA

Egy termék ÁFA kulcsa (`VAT`) bármikor megváltozhat, de ez a megrendelés során teljesen természetes, de a kiállított számlák ÁFA tartalma már nem változhat meg. Ezért a megrendelés során az ÁFÁ-ra idegen kulccsal kell hivatkozni, hogy a változást követni lehessen, viszont a kiállított számlában le kell tárolni a kiállítás pillanatában az aktuális ÁFA kulcsot.

#### Termék leírás

A termékekhez tartozhat egy XML formátumú leírás, ennek tartalmát szemlélteti az alábbi példa. Ezen leírás a termékhez tartozó egyéb extra információkat tartalmazza, amelyeket nem fejtettünk ki a relációs modellben

```xml
<product>
  <product_size>
    <unit>cm</unit>
    <width>150</width>
    <height>50</height>
    <depth>150</depth>
  </product_size>
  <package_parameters>
    <number_of_packages>1</number_of_packages>
    <package_size>
      <unit>cm</unit>
      <width>150</width>
      <height>20</height>
      <depth>20</depth>
    </package_size>
  </package_parameters>
  <description>
    Requires battery (not part of the package).
  </description>
  <recommended_age>0-18 m</recommended_age>
</product>
```
