# Microsoft SQL Server programozása

A gyakorlat célja, hogy a hallgatók megismerjék a Microsoft SQL Server platform szerver oldali programozásának alapjait, elsajátítsák az alapfogalmakat és a fejlesztőeszköz használatát.

## Előfeltételek

A labor elvégzéséhez szükséges eszközök:

- Microsoft SQL Server (LocalDB vagy Express edition)
- SQL Server Management Studio
- Adatbázis létrehozó script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/datadriven/master/overrides/db/mssql.sql)

Amit érdemes átnézned:

- SQL nyelv
- Microsoft SQL Server programozása (tárolt eljárások, triggerek)
- [Microsoft SQL Server használata segédlet](../../db/mssql.md)

## Gyakorlat menete

Az első négy feladatot (beleértve a megoldások tesztelését is) a gyakorlatvezetővel együtt oldjuk meg. Az utolsó feladat önálló munka, amennyiben marad rá idő.

!!! info ""
    Emlékeztetőként a megoldások is megtalálhatóak az útmutatóban is. Előbb azonban próbáljuk magunk megoldani a feladatot!

## Feladat 0: Adatbázis létrehozása, ellenőrzése

Az adatbázis az adott géphez kötött, ezért nem biztos, hogy a korábban létrehozott adatbázis most is létezik. Ezért először ellenőrizzük, és ha nem találjuk, akkor hozzuk létre újra az adatbázist. (Ennek mikéntjét lásd az [első gyakorlat anyagában](../transactions/index.md).)

## Feladat 1: SQL parancsok (emlékeztető)

Írjon SQL lekérdezés/utasítást az alábbi feladatokhoz.

1. Hány nem teljesített megrendelésünk van (a státusz alapján)?

    ??? example "Megoldás"
        ```sql
        SELECT COUNT(*)
        FROM [Order] o JOIN Status s ON o.StatusID = s.ID
        WHERE s.Name != 'Delivered'
        ```

        A `JOIN` mellett az oszlopfüggvény (aggregáció) használatára látunk példát. (A táblák kapcsolására nem csak ez a szintaktika használható, előadáson szerepelt alternatív is.)

1. Melyek azok a fizetési módok, amit soha nem választottak a megrendelőink?

    ??? example "Megoldás"
        ```sql
        SELECT p.Method
        FROM [Order] o RIGHT OUTER JOIN PaymentMethod p ON o.PaymentMethodID = p.ID
        WHERE o.ID IS NULL
        ```
        
        A megoldás kulcsa az `OUTER JOIN`, aminek köszönhetően láthatjuk, mely fizetési mód rekordhoz _nem_ tartozik egyetlen megrendelés se.

1. Rögzítsünk be egy új vevőt! Kérdezzük le az újonnan létrejött rekord kulcsát!

    ??? example "Megoldás"
        ```sql
        INSERT INTO Customer(Name, Login, Password, Email)
        VALUES ('Teszt Elek', 't.elek', '********', 't.elek@email.com')

        SELECT @@IDENTITY
        ```

        Az `INSERT` után javasolt kiírni az oszlopneveket az egyértelműség végett, bár nem kötelező. Vegyük észre, hogy az ID oszlopnak nem adunk értéket, mert azt a tábla definíciójakor meghatározva a szerver adja automatikusan. Ezért kell utána lekérdeznünk, hogy tudjuk, milyen ID-t adott.

1. A kategóriák között hibásan szerepel a _Tricycle_ kategória név. Javítsuk át a kategória nevét _Tricycles_-re!

    ??? example "Megoldás"
        ```sql
        UPDATE Category
        SET Name = 'Tricycles'
        WHERE Name = 'Tricycle'
        ```

1. Melyik termék kategóriában van a legtöbb termék?

    ??? example "Megoldás"
        ```sql
        SELECT TOP 1 Name, (SELECT COUNT(*) FROM Product WHERE Product.CategoryID = c.ID) AS cnt
        FROM Category c
        ORDER BY cnt DESC
        ```

        A kérdésre több alternatív lekérdezés is eszünkbe juthat. Ez csak egyike a lehetséges megoldásoknak. Itt láthatunk példát az allekérdezésre is.

## Feladat 2: Termékkategória rögzítése

Hozzon létre egy tárolt eljárást, aminek a segítségével egy új kategóriát vehetünk fel. Az eljárás bemenő paramétere a felvételre kerülő kategória neve, és opcionálisan a szülőkategória neve. Dobjon hibát, ha a kategória létezik, vagy a szülőkategória nem létezik. A kategória elsődleges kulcsának generálását bízza az adatbázisra.

??? example "Megoldás"
    **Tárolt eljárás**

    ```sql
    CREATE OR ALTER PROCEDURE AddNewCategory
        @Name NVARCHAR(50),
        @ParentName NVARCHAR(50)
    AS

    BEGIN TRAN;

    -- Létezik-e ilyen névvel már kategória
    DECLARE @ID INT;
    SELECT @ID = ID
    FROM Category WITH (TABLOCKX)
    WHERE Name = @Name;

    IF @ID IS NOT NULL
    BEGIN
        ROLLBACK;
        DECLARE @ErrorMessage NVARCHAR(255) = 'Category ' + @Name + ' already exists';
        THROW 51000, @ErrorMessage, 1;
    END

    -- Szülő kategóriának léteznie kell
    DECLARE @ParentID INT;
    IF @ParentName IS NOT NULL
    BEGIN
        SELECT @ParentID = ID
        FROM Category
        WHERE Name = @ParentName;

        IF @ParentID IS NULL
        BEGIN
            ROLLBACK;
            DECLARE @ParentErrorMessage NVARCHAR(255) = 'Category ' + @ParentName + ' does not exist';
            THROW 51001, @ParentErrorMessage, 1;
        END
    END

    INSERT INTO Category(Name, ParentCategoryID)
    VALUES (@Name, @ParentID);

    COMMIT;
    ```

    **Tesztelés**

    Nyissunk egy új Query ablakot és adjuk ki az alábbi parancsot.

    ```sql
    EXEC AddNewCategory 'Beach balls', NULL
    ```

    Ennek sikerülnie kell. Ellenőrizzük utána a tábla tartalmát.

    Ismételjük meg a fenti beszúrást, ekkor már hibát kell dobjon.

    Próbáljuk ki szülőkategóriával is.

    ```sql
    EXEC AddNewCategory 'LEGO Star Wars', 'LEGO'
    ```

    ??? tip "Kell-e pontosvessző MSSQL-ben?"
        A Microsoft SQL Server-ben a pontosvessző nem kötelező, és a legtöbb esetben felesleges is. Azonban a fenti `AddNewCategory` tárolt eljárásban a `THROW` utasítás [**előtt kötelező**](https://learn.microsoft.com/en-us/sql/t-sql/language-elements/throw-transact-sql?view=sql-server-ver17#remarks) a pontosvessző használata, így az egységes stílus érdekében érdemes minden utasítás végére pontosvesszőt tenni.

        A pontosvessző használata egyébként is jó gyakorlat lehet, ha a kódolási stílus egységességére törekszünk.

    **Aktív zárak listázása hibakereséshez**

    A tárolt eljárásunk tranzakciót és zárolást használ (`TABLOCKX`), ami fontos a konkurens műveletek helyes kezeléséhez. Hibakeresés során hasznos lehet látni, hogy éppen milyen zárak vannak aktívak az adatbázisban.

    Az aktív zárak lekérdezéséhez használhatjuk a következő SQL parancsot:

    ```sql
    SELECT
        OBJECT_NAME(P.object_id) AS TableName,
        resource_type, request_status, request_session_id
    FROM
        sys.dm_tran_locks dtl
        JOIN sys.partitions P ON dtl.resource_associated_entity_id = P.hobt_id
    ```

    Ez a lekérdezés megmutatja:
    
    - **TableName**: Melyik táblán van a zár
    - **resource_type**: A zár típusa (pl. KEY, PAGE, TABLE)
    - **request_status**: A zár állapota (GRANT = megadva, WAIT = várakozik)
    - **request_session_id**: Melyik session tartja a zárat

    Ha szeretnénk tesztelni, hogy mit látunk zárak esetén, nyissunk két Query ablakot, és az egyikben indítsunk egy tranzakciót, ami nem fut le azonnal:

    ```sql
    -- 1. Query ablak
    BEGIN TRAN
    SELECT * FROM Category WITH (TABLOCKX)
    -- NE COMMIT-oljuk még!
    ```

    Majd a másik Query ablakban futtassuk le a zárak listázását. Látni fogjuk, hogy az első session tart egy táblaszintű zárat a Category táblán. Ne felejtsük el lezárni a tranzakciót az első ablakban:

    ```sql
    -- 1. Query ablak
    COMMIT
    ```

## Feladat 3: Megrendeléstétel státuszának karbantartása

Írjon triggert, ami a megrendelés státuszának változása esetén a hozzá tartozó egyes tételek státuszát a megfelelőre módosítja, ha azok régi státusza megegyezett a megrendelés régi státuszával. A többi tételt nem érinti a státusz változása.

??? example "Megoldás"
    **Trigger**

    ```sql
    CREATE OR ALTER TRIGGER UpdateOrderStatus
      ON [Order]
      FOR UPDATE
    AS

    UPDATE OrderItem
    SET StatusID = i.StatusID
    FROM OrderItem oi
      INNER JOIN inserted i ON i.Id = oi.OrderID
      INNER JOIN deleted d ON d.ID = oi.OrderID
    WHERE i.StatusID != d.StatusID
      AND oi.StatusID = d.StatusID
    ```

    Szánjunk egy kis időt az `UPDATE ... FROM` utasítás működési elvének megértésére. Az alapelvek a következők. Akkor használjuk, ha a módosítandó tábla bizonyos mezőit más tábla vagy táblák tartalma alapján szeretnénk beállítani. A szintaktika alapvetően a már megszokott `UPDATE ... SET ...` formát követi, kiegészítve egy `FROM` szakasszal, melyben már a `SELECT FROM` utasításnál megismerttel azonos szintaktikával más táblákból illeszthetünk (`JOIN`) adatokat a módosítandó táblához. Így a `SET` szakaszban az illesztett táblák oszlopai is felhasználhatók adatforrásként (vagyis állhatnak az egyenlőség jobb oldalán).

    **Tesztelés**

    Ellenőrizzük a megrendelés és a tételek státuszát:

    ```sql
    SELECT OrderItem.StatusID, [Order].StatusID
    FROM OrderItem JOIN [Order] ON OrderItem.OrderID = [Order].ID
    WHERE OrderID = 1
    ```

    Változtassuk meg a megrendelést:

    ```sql
    UPDATE [Order]
    SET StatusID = 4
    WHERE ID = 1
    ```

    Ellenőrizzük a megrendelést és a tételeket (update után minden státusznak meg kell változnia):

    ```sql
    SELECT OrderItem.StatusID, [Order].StatusID
    FROM OrderItem JOIN [Order] ON OrderItem.OrderID = [Order].ID
    WHERE OrderID = 1
    ```

## Feladat 4: Vevő megrendeléseinek összegzése

Tároljuk el a vevő összes megrendelésének végösszegét a Vevő táblában!

1. Adjuk hozzá az a táblához az új oszlopot: `ALTER TABLE Customer ADD Total FLOAT`
1. Számoljuk ki az aktuális végösszeget. A megoldáshoz használjunk kurzort, ami minden vevőn megy végig.

??? example "Megoldás"
    ```sql
    DECLARE cur_customer CURSOR
        FOR SELECT ID FROM Customer
    DECLARE @CustomerId INT
    DECLARE @Total FLOAT

    OPEN cur_customer
    FETCH NEXT FROM cur_customer INTO @CustomerId
    WHILE @@FETCH_STATUS = 0
    BEGIN

        SELECT @Total = SUM(oi.Amount * oi.Price)
        FROM CustomerSite s
        INNER JOIN [Order] o ON o.CustomerSiteID = s.ID
        INNER JOIN OrderItem oi ON oi.OrderID = o.ID
        WHERE s.CustomerID = @CustomerId

        UPDATE Customer
        SET Total = ISNULL(@Total, 0)
        WHERE ID = @CustomerId

        FETCH NEXT FROM cur_customer INTO @CustomerId
    END

    CLOSE cur_customer
    DEALLOCATE cur_customer
    ```

    Ellenőrizzük a `Customer` tábla tartalmát.

## Feladat 5: Vevő összmegrendelésének karbantartása (önálló feladat)

Az előző feladatban kiszámolt érték az aktuális állapotot tartalmazza csak. Készítsünk triggert, amivel karbantartjuk azt az összeget minden megrendelést érintő változás esetén. Az összeg újraszámolása helyett csak frissítse a változásokkal az értéket!

??? example "Megoldás"
    A megoldás kulcsa meghatározni, mely táblára kell a triggert tenni. A megrendelések változása érdekes számunkra, de valójában a végösszeg a megrendeléshez felvett tételek módosulásakor fog változni, így erre a táblára kell a trigger.

    A feladat nehézségét az adja, hogy az `inserted` és `deleted` táblákban nem csak egy vevő adatai módosulhatnak. Egy lehetséges megoldás a korábban használt kurzoros megközelítés (itt a változásokon kell iterálni). Avagy megpróbálhatjuk megírni egy utasításban is, ügyelve arra, hogy vevők szerint csoportosítsuk a változásokat.

    **Trigger**

    ```sql
    CREATE OR ALTER TRIGGER CustomerTotalUpdate
    ON OrderItem
    FOR INSERT, UPDATE, DELETE
    AS

    UPDATE Customer
    SET Total = ISNULL(Total, 0) + TotalChange
    FROM Customer
    INNER JOIN
        (SELECT s.CustomerId, SUM(Amount * Price) AS TotalChange
        FROM CustomerSite s
        INNER JOIN [Order] o ON o.CustomerSiteID = s.ID
        INNER JOIN inserted i ON i.OrderID = o.ID
        GROUP BY s.CustomerId) CustomerChange ON Customer.ID = CustomerChange.CustomerId

    UPDATE Customer
    SET Total = ISNULL(Total, 0) - TotalChange
    FROM Customer
    INNER JOIN
        (SELECT s.CustomerId, SUM(Amount * Price) AS TotalChange
        FROM CustomerSite s
        INNER JOIN [Order] o ON o.CustomerSiteID = s.ID
        INNER JOIN deleted d ON d.OrderID = o.ID
        GROUP BY s.CustomerID) CustomerChange ON Customer.ID = CustomerChange.CustomerId
    ```

    **Tesztelés**

    Nézzük meg az összmegrendelések aktuális értékét, jegyezzük meg a számokat.

    ```sql
    SELECT ID, Total
    FROM Customer
    ```

    Módosítsunk egy megrendelés mennyiségén.

    ```sql
    UPDATE OrderItem
    SET Amount = 3
    WHERE ID = 1
    ```

    Nézzük meg az összegeket ismét, meg kellett változnia a számnak.

    ```sql
    SELECT ID, Total
    FROM Customer
    ```
