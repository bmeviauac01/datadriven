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

        A `join` mellett az oszlopfüggvény (aggregáció) használatára látunk példát. (A táblák kapcsolására nem csak ez a szintaktika használható, előadáson szerepelt alternatív is.)

1. Melyek azok a fizetési módok, amit soha nem választottak a megrendelőink?

    ??? example "Megoldás"
        ```sql
        SELECT p.Method
        FROM [Order] o RIGHT OUTER JOIN PaymentMethod p ON o.PaymentMethodID = p.ID
        WHERE o.ID IS NULL
        ```
        
        A megoldás kulcsa az `outer join`, aminek köszönhetően láthatjuk, mely fizetési mód rekordhoz _nem_ tartozik egyetlen megrendelés se.

1. Rögzítsünk be egy új vevőt! Kérdezzük le az újonnan létrejött rekord kulcsát!

    ??? example "Megoldás"
        ```sql
        INSERT INTO Customer(Name, Login, Password, Email)
        VALUES ('Teszt Elek', 't.elek', '********', 't.elek@email.com')

        SELECT @@IDENTITY
        ```

        Az `insert` után javasolt kiírni az oszlopneveket az egyértelműség végett, bár nem kötelező. Vegyük észre, hogy az ID oszlopnak nem adunk értéket, mert azt a tábla definíciójakor meghatározva a szerver adja automatikusan. Ezért kell utána lekérdeznünk, hogy tudjuk, milyen ID-t adott.

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
    DECLARE @ID INT
    SELECT @ID = ID
    FROM Category WITH (TABLOCKX)
    WHERE UPPER(Name) = UPPER(@Name)

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
        WHERE UPPER(Name) = UPPER(@ParentName)

        IF @ParentID IS NULL
        BEGIN
            ROLLBACK;
            DECLARE @ParentErrorMessage NVARCHAR(255) = 'Category ' + @ParentName + ' does not exist';
            THROW 51000, @ParentErrorMessage, 1;
        END
    END

    INSERT INTO Category
    VALUES(@Name, @ParentID)

    COMMIT
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

    Szánjunk egy kis időt az `update ... from` utasítás működési elvének megértésére. Az alapelvek a következők. Akkor használjuk, ha a módosítandó tábla bizonyos mezőit más tábla vagy táblák tartalma alapján szeretnénk beállítani. A szintaktika alapvetően a már megszokott `update ... set...` formát követi, kiegészítve egy `from` szakasszal, melyben már a `select from` utasításnál megismerttel azonos szintaktikával más táblákból illeszthetünk (`join`) adatokat a módosítandó táblához. Így a `set` szakaszban az illesztett táblák oszlopai is felhasználhatók adatforrásként (vagyis állhatnak az egyenlőség jobb oldalán).

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

    OPEN cur_customer;
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
