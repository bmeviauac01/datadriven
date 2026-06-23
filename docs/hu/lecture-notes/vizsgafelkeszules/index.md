# Vizsgafelkészülés és AI-asszisztált gyakorlás

Az AI egyetemi környezetben való használatának általános szabályai és ajánlásai: https://vik.bme.hu/hallgatoknak/altalanos/mi-hasznalat-ajanlasok

Ez az oldal segédanyagokat, mintafeladatokat és AI-alapú gyakorlási stratégiákat tartalmaz az Adatvezérelt rendszerek tárgy vizsgájára való felkészüléshez. A minták a [minta adatbázison](../../db/index.md) futtathatóak, amelynek sémáját az alábbi táblázat foglalja össze.

!!! info "Minta adatbázis főbb táblái"
    - **Product** (ID, Name, Price, Stock, VATID, CategoryID, Description)
    - **Category** (ID, Name, ParentCategoryID)
    - **VAT** (ID, Percentage)
    - **Customer** (ID, Name, Login, Password, Email, MainCustomerSiteID)
    - **CustomerSite** (ID, CustomerID, ZipCode, City, Street, Tel, Fax)
    - **Order** (ID, CustomerID, Date, DeadLine, StatusID, PaymentMethodID, SiteID)
    - **OrderItem** (ID, OrderID, ProductID, Amount, Price)
    - **Status** (ID, Name)
    - **PaymentMethod** (ID, Method, Deadline)

---

## Hogyan használj AI-t a felkészüléshez?

A nagy nyelvi modellek (ChatGPT, GitHub Copilot, Gemini stb.) kiváló eszközök arra, hogy **vizsgaszerű feladatokat generálj magadnak** és **azonnali visszajelzést kapj** a megoldásaidra. Az alábbi stratégia bizonyítottan hatékony:

### Általános munkafolyamat

```
1.  Add meg az AI-nak a kontextust (séma, technológia, nehézségi szint).
2.  Kérj tőle feladatot.
3.  Oldd meg a feladatot ÖNÁLLÓAN – ne nézz megoldásra!
4.  Kérd az AI-t, hogy értékelje a megoldásod.
5.  Kérd, hogy mutassa meg a jobb/alternatív megoldást és magyarázza el.
6.  Ismételd 2–5-öt más témákra.
```

!!! warning "Fontos"
    Az AI néha hibás kódot generál. Mindig futtasd le a saját környezetedben, és ha az eredmény gyanús, kérdőjelezd meg!

### Hasznos AI-promptok – sablon gyűjtemény

Az alábbi promptokat másold be egy AI-csevegőbe (pl. ChatGPT, Copilot). A `[ ]`-ben lévő részeket írd felül.

---

#### Feladatgenerálás – általános sablon

```
Te egy adatbázis-tárgy vizsgáztató professzor vagy a BME-n.
Az adatbázis sémája:
  Product(ID, Name, Price, Stock, VATID, CategoryID)
  Category(ID, Name, ParentCategoryID)
  VAT(ID, Percentage)
  Customer(ID, Name, Login, Password, Email, MainCustomerSiteID)
  CustomerSite(ID, CustomerID, ZipCode, City, Street)
  Order(ID, CustomerID, Date, DeadLine, StatusID, PaymentMethodID, SiteID)
  OrderItem(ID, OrderID, ProductID, Amount, Price)
  Status(ID, Name)
  PaymentMethod(ID, Method, Deadline)

Adj nekem egy [NEHÉZSÉG: könnyű/közepes/nehéz] szintű [TÉMAKÖR] feladatot.
Ne adj megoldást, csak a feladatleírást!
```

---

#### Célzott gyakorlás gyenge témakörökre

Ha tudod, mely nyelvi elemekkel küzdesz, **mondd meg az AI-nak kifejezetten**, hogy pontosan azokat gyakoroltassa veled. Ez sokkal hatékonyabb az általános gyakorlásnál.

```
Gyenge vagyok a következőkből: [pl. HAVING és GROUP BY, korrelált
al-lekérdezések, ablakfüggvények, LEFT OUTER JOIN, EF Include,
MongoDB aggregációs pipeline].

Adj nekem 5 feladatot, egyesével, amelyek kifejezetten ezeket a
nyelvi elemeket igénylik. Kezdd könnyűvel, és minden helyes válasz
után növeld a nehézséget. Ne add meg a megoldást, amíg nem válaszoltam.
Minden válaszom után mondd meg, helyesen használtam-e a célzott
nyelvi elemet, és ha nem, magyarázd el, mit hibáztam.
```

!!! tip "Először mérd fel a gyenge pontjaidat"
    Nem tudod, miben vagy gyenge? Kérdezd meg az AI-t: *„Adj egy rövid, vegyes felmérő kvízt (témánként egy kérdés: join-ok, csoportosítás/HAVING, al-lekérdezések, ablakfüggvények, tranzakciók). A válaszaim alapján mondd meg, mely témákat kellene többet gyakorolnom.”* Aztán ezeket a témákat add vissza a fenti promptba.

---

#### Megoldás-értékelés

```
Értékeld az alábbi [SQL/C# LINQ/EF/MongoDB] megoldásomat az előző feladatra.
Jelezd, ha hibás, és magyarázd el, miért. Ha helyes, mutass alternatív megoldást.

Az én megoldásom:
[IDE MÁSOLD A MEGOLDÁSOD]
```

---

#### Vizsga szimulálása

```
Szimuláld az Adatvezérelt rendszerek tárgy vizsgáját!
Tegyél fel nekem egymás után 5 feladatot a következő témákból:
[pl. SQL lekérdezések, tranzakciókezelés, Entity Framework, MongoDB]
Minden feladat után várd meg a válaszomat, mielőtt a következőre térsz.
A végén adj összesített értékelést.
```

---

## Mintafeladatok témakörönként

### SQL lekérdezések

Az alábbi feladatok a `SQL`, az `MSSQL szerveroldali programozás` és a `Transactions` témakörökhöz, valamint az MSSQL gyakorlat anyagához kapcsolódnak. Szándékosan nehezebbek, és lefedik a jegyzet teljes spektrumát (ablakfüggvények, CTE-k, korrelált al-lekérdezések, feltételes aggregáció, XML/XQuery, szerveroldali programozás, triggerek és kurzorok).

??? example "1. feladat – Ablakfüggvények és csoporton belüli rangsorolás"
    **Feladat:** Minden kategóriához add vissza a **két legdrágább terméket** (kategórianév, terméknév, ár)! Ha több termék ára azonos, az összes holtversenyben lévő termék szerepeljen. Add meg a termék kategórián belüli rangsorát is!

    ??? success "Megoldás"
        Használj `DENSE_RANK()`-et, hogy a holtverseny azonos rangot kapjon, és a „top 2” ne csorbuljon önkényesen.

        ```sql
        WITH Ranked AS
        (
            SELECT c.Name AS Category, p.Name AS Product, p.Price,
                   DENSE_RANK() OVER (PARTITION BY p.CategoryID ORDER BY p.Price DESC) AS rnk
            FROM Product p
                 INNER JOIN Category c ON c.ID = p.CategoryID
        )
        SELECT Category, Product, Price, rnk
        FROM Ranked
        WHERE rnk <= 2
        ORDER BY Category, Price DESC
        ```

??? example "2. feladat – CTE és lapozás"
    **Feladat:** Listázd a termékeket név szerint ábécé sorrendben, de **csak a második 10-es oldalt** (11–20. sor)! Jelenítsd meg a sorszámot is! Adj CTE-alapú és `OFFSET/FETCH` megoldást is!

    ??? success "Megoldás"
        ```sql
        -- CTE + ROW_NUMBER
        WITH q AS
        (
            SELECT ROW_NUMBER() OVER (ORDER BY Name) AS rn, Name, Price
            FROM Product
        )
        SELECT rn, Name, Price
        FROM q
        WHERE rn BETWEEN 11 AND 20
        ORDER BY rn

        -- MSSQL 2012+ lapozási szintaktika
        SELECT Name, Price
        FROM Product
        ORDER BY Name
        OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY
        ```

??? example "3. feladat – Korrelált al-lekérdezés / anti-join"
    **Feladat:** Listázd azon vevők nevét, akik **még soha nem adtak le rendelést**! Mutass `NOT EXISTS` megoldást, és magyarázd el, miért egyenértékű a `LEFT JOIN ... IS NULL` megoldással!

    ??? success "Megoldás"
        ```sql
        SELECT c.Name
        FROM Customer c
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [Order] o
            WHERE o.CustomerID = c.ID
        )
        ```

        Egyenértékű anti-join:

        ```sql
        SELECT c.Name
        FROM Customer c
             LEFT JOIN [Order] o ON o.CustomerID = c.ID
        WHERE o.ID IS NULL
        ```

        A `NOT IN` is működne, de nem biztonságos, ha az al-lekérdezés `NULL`-t adhat vissza (ekkor az egész feltétel `UNKNOWN` lesz, és nem ad sort).

??? example "4. feladat – Több táblát érintő aggregáció HAVING-gel"
    **Feladat:** Keresd meg azokat a vevőket, akiknek a **teljes elköltött összege** (az `OrderItem.Amount * OrderItem.Price` szorzatok összege az összes rendelésükön) meghaladja a 100000-et! Add meg a vevő nevét és az összeget, az összeg szerint csökkenő sorrendben!

    ??? success "Megoldás"
        ```sql
        SELECT c.Name, SUM(oi.Amount * oi.Price) AS TotalSpent
        FROM Customer c
             INNER JOIN [Order] o      ON o.CustomerID = c.ID
             INNER JOIN OrderItem oi   ON oi.OrderID = o.ID
        GROUP BY c.ID, c.Name
        HAVING SUM(oi.Amount * oi.Price) > 100000
        ORDER BY TotalSpent DESC
        ```

??? example "5. feladat – Feltételes aggregáció (pivot-szerű)"
    **Feladat:** Készíts **kategóriánként egy sort**, amely megmutatja a kategória nevét, hány terméke van raktáron (`Stock > 0`) és hány nincs (`Stock = 0`)!

    ??? success "Megoldás"
        ```sql
        SELECT c.Name,
               SUM(CASE WHEN p.Stock > 0 THEN 1 ELSE 0 END) AS InStock,
               SUM(CASE WHEN p.Stock = 0 THEN 1 ELSE 0 END) AS OutOfStock
        FROM Category c
             INNER JOIN Product p ON p.CategoryID = c.ID
        GROUP BY c.ID, c.Name
        ```

??? example "6. feladat – XML lekérdezése XQuery-vel"
    **Feladat:** A `Product` tábla `Description` oszlopa XML-t tárol. Listázd minden olyan termék nevét, amelynek a leírása **egynél több csomagot** ad meg (`/product/package_parameters/number_of_packages`)!

    ??? success "Megoldás"
        ```sql
        SELECT Name
        FROM Product
        WHERE Description.value('(/product/package_parameters/number_of_packages)[1]', 'int') > 1
        ```

        `exist()`-alapú alternatíva a „0-18 hónapos korra ajánlott” termékekre:

        ```sql
        SELECT Name
        FROM Product
        WHERE Description.exist('(/product)[(./recommended_age)[1] eq "0-18 m"]') = 1
        ```

??? example "7. feladat – Korrelált UPDATE (MSSQL `UPDATE ... FROM`)"
    **Feladat:** Minden olyan rendelési tételnél, ahol a rögzített ár **alacsonyabb** a termék aktuális áránál, írd felül a rögzített árat a termék aktuális árával!

    ??? success "Megoldás"
        ```sql
        UPDATE oi
        SET oi.Price = p.Price
        FROM OrderItem oi
             INNER JOIN Product p ON p.ID = oi.ProductID
        WHERE oi.Price < p.Price
        ```

??? example "8. feladat – Tárolt eljárás tranzakcióval és hibakezeléssel"
    **Feladat:** Írj egy `RecordSale @OrderID, @ProductID, @Amount` tárolt eljárást, amely **egyetlen tranzakción belül** ellenőrzi, van-e elég készlet; ha igen, beszúr egy `OrderItem`-et (az árat a `Product`-ból véve) és csökkenti a termék készletét; egyébként hibát dob, és nem módosít semmit! Olyan izolációs szintet használj, amely megakadályozza, hogy egy másik tranzakció megváltoztassa a készletet az ellenőrzés és a módosítás között!

    ??? success "Megoldás"
        ```sql
        CREATE OR ALTER PROCEDURE RecordSale
            @OrderID INT,
            @ProductID INT,
            @Amount INT
        AS
        BEGIN
            SET XACT_ABORT ON;
            SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
            BEGIN TRAN;

            DECLARE @Stock INT, @Price INT;

            SELECT @Stock = Stock, @Price = Price
            FROM Product
            WHERE ID = @ProductID;

            IF @Stock IS NULL
                THROW 51001, 'No such product', 1;

            IF @Stock < @Amount
                THROW 51002, 'Not enough stock', 1;

            INSERT INTO OrderItem (OrderID, ProductID, Amount, Price)
            VALUES (@OrderID, @ProductID, @Amount, @Price);

            UPDATE Product
            SET Stock = Stock - @Amount
            WHERE ID = @ProductID;

            COMMIT;
        END
        ```

        A `REPEATABLE READ` garantálja, hogy a készletellenőrzéskor olvasott sort más tranzakció nem módosíthatja az `UPDATE` befejezése előtt, ezzel megakadályozva az elveszett módosítást / túlrendelést.

??? example "9. feladat – DML trigger"
    **Feladat:** Hozz létre egy `AFTER INSERT` triggert az `OrderItem` táblán, amely automatikusan csökkenti a megfelelő termék készletét a beszúrt mennyiséggel! Ügyelj rá, hogy akkor is helyesen működjön, ha egyszerre több sort szúrnak be!

    ??? success "Megoldás"
        Egy trigger **utasításonként egyszer** fut le, és az `inserted` több sort is tartalmazhat, ezért aggregálj – ne feltételezz egyetlen sort.

        ```sql
        CREATE OR ALTER TRIGGER OrderItemDecreaseStock
            ON OrderItem
            AFTER INSERT
        AS
        BEGIN
            UPDATE p
            SET p.Stock = p.Stock - i.TotalAmount
            FROM Product p
                 INNER JOIN (SELECT ProductID, SUM(Amount) AS TotalAmount
                             FROM inserted
                             GROUP BY ProductID) i ON i.ProductID = p.ID
        END
        ```

??? example "10. feladat – Kurzor"
    **Feladat:** Kurzor segítségével járd be a `Stock < 5` készletű termékeket! Minden ilyen terméknél: ha még **soha** nem rendelték, töröld; egyébként emeld meg az árát 10%-kal!

    ??? success "Megoldás"
        ```sql
        DECLARE @ProductID INT, @OrderCount INT;

        DECLARE lowstock_cur CURSOR FOR
            SELECT ID FROM Product WHERE Stock < 5;

        OPEN lowstock_cur;
        FETCH NEXT FROM lowstock_cur INTO @ProductID;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SELECT @OrderCount = COUNT(*)
            FROM OrderItem
            WHERE ProductID = @ProductID;

            IF @OrderCount = 0
                DELETE FROM Product WHERE ID = @ProductID;
            ELSE
                UPDATE Product SET Price = Price * 1.1 WHERE ID = @ProductID;

            FETCH NEXT FROM lowstock_cur INTO @ProductID;
        END
        CLOSE lowstock_cur;
        DEALLOCATE lowstock_cur;
        ```

        Megjegyzés: ez egy oktató példa a kurzorokra; a gyakorlatban egy halmazalapú `DELETE` + `UPDATE` páros gyorsabb lenne.

---

### LINQ lekérdezések

Az alábbi feladatok a `LINQ` anyagához kapcsolódnak. Tételezz fel memóriabeli listákat, mint `List<Product> products`, `List<VAT> vat`, `List<Category> categories`, `List<OrderItem> orderItems` (a séma mezőivel összhangban).

??? example "1. feladat – Csoportosítás aggregációval és HAVING-szerű szűréssel"
    **Feladat:** Csoportosítsd a termékeket `VATID` szerint! Minden csoporthoz add vissza az ÁFA-azonosítót, a termékszámot és az átlagárat, de **csak azoknál a csoportoknál, ahol legalább 3 termék van**, átlagár szerint csökkenő sorrendben!

    ??? success "Megoldás"
        ```csharp
        // Fluent szintaktika
        var result = products
            .GroupBy(p => p.VATID)
            .Where(g => g.Count() >= 3)
            .Select(g => new { VatID = g.Key, Count = g.Count(), AvgPrice = g.Average(p => p.Price) })
            .OrderByDescending(x => x.AvgPrice);

        // Query szintaktika
        var result =
            from p in products
            group p by p.VATID into g
            where g.Count() >= 3
            orderby g.Average(p => p.Price) descending
            select new { VatID = g.Key, Count = g.Count(), AvgPrice = g.Average(p => p.Price) };
        ```

??? example "2. feladat – Csoportos join (bal oldali külső join LINQ-ben)"
    **Feladat:** Minden ÁFA-kategóriához listázd a százalékot és az azt **használó termékek számát**, beleértve azokat az ÁFA-kategóriákat is, amelyekhez egyetlen termék sem tartozik!

    ??? success "Megoldás"
        Egy `join ... into` (csoportos join), amelyet `DefaultIfEmpty` követ, valósít meg bal oldali külső joint.

        ```csharp
        var result =
            from v in vat
            join p in products on v.ID equals p.VATID into grp
            select new { v.Percentage, Count = grp.Count() };

        // Fluent megfelelő
        var result2 = vat.GroupJoin(
            products,
            v => v.ID,
            p => p.VATID,
            (v, grp) => new { v.Percentage, Count = grp.Count() });
        ```

??? example "3. feladat – Halmazműveletek"
    **Feladat:** Állítsd elő azon termékek nevét, amelyek **vagy** olcsóbbak 1000-nél, **vagy** drágábbak 100000-nél, duplikációk nélkül, halmazművelettel!

    ??? success "Megoldás"
        ```csharp
        var cheap     = products.Where(p => p.Price < 1000).Select(p => p.Name);
        var expensive = products.Where(p => p.Price > 100000).Select(p => p.Name);

        var result = cheap.Union(expensive); // a Union eltávolítja a duplikációkat
        ```

??? example "4. feladat – Korrelált al-lekérdezés vetítésben"
    **Feladat:** Minden termékhez add ki a nevét és a **megrendelt összmennyiséget** (az `OrderItem.Amount` összege az adott termékre), beleértve a soha nem rendelt termékeket is (összeg 0)!

    ??? success "Megoldás"
        ```csharp
        var result = products.Select(p => new
        {
            p.Name,
            TotalOrdered = orderItems.Where(oi => oi.ProductID == p.ID)
                                     .Sum(oi => oi.Amount)
        });
        ```

??? example "5. feladat – Késleltetett kiértékelés (elméleti)"
    **Feladat:** Mit ír ki az alábbi kód, és miért? Mikor hajtódik végre ténylegesen a lekérdezés?

    ```csharp
    var q = products.Where(p => p.Price < 1000);
    products.Add(new Product { Name = "Cheap", Price = 10 });
    Console.WriteLine(q.Count());
    ```

    ??? success "Megoldás"
        A kiírt darabszám **tartalmazza** az újonnan hozzáadott „Cheap" terméket is. A `Where` egy `IEnumerable<T>` leírót ad vissza, és **késleltetett kiértékelést** használ – a `q` deklarálásakor semmi nem értékelődik ki. A lekérdezés csak akkor fut le, amikor bejárjuk, itt a `Count()` hívásakor, addigra az új elem már a listában van. Az eredmény „befagyasztásához" közvetlenül a `Where` után `.ToList()`-et hívnánk.

??? example "6. feladat – Vetítés nevesített típusba számított mezővel"
    **Feladat:** Kapcsold össze a `products` és `vat` listákat, és vetítsd egy `PriceInfo(string Name, int GrossPrice)` osztályba, ahol a bruttó ár `Price * (1 + Percentage/100)`!

    ??? success "Megoldás"
        ```csharp
        var result =
            from p in products
            join v in vat on p.VATID equals v.ID
            select new PriceInfo(p.Name, p.Price * (100 + v.Percentage) / 100);
        ```

---

### Entity Framework Core

Az alábbi feladatok a `Entity Framework` anyagához és az EF-gyakorlathoz kapcsolódnak. Tételezz fel egy konfigurált `AdatvezDbContext`-et `Products`, `Orders`, `OrderItems`, `Categories`, `Customers`, `VATs` `DbSet`-ekkel és a jegyzetből ismert navigation property-kkel.

??? example "1. feladat – Eager loading több szinten át"
    **Feladat:** EF Core segítségével listázd minden „Delivered" rendeléshez a **vevő nevét** és a **rendelés összértékét** (az `OrderItem.Amount * OrderItem.Price` összege)! Csak azt töltsd be, amire szükség van!

    ??? success "Megoldás"
        ```csharp
        using var db = new AdatvezDbContext();

        var orders = db.Orders
            .Where(o => o.Status.Name == "Delivered")
            .Select(o => new
            {
                Customer = o.Customer.Name,
                Total = o.OrderItems.Sum(oi => oi.Amount * oi.Price)
            })
            .ToList();
        ```

        Mivel pontosan a szükséges oszlopokra **vetítünk**, az EF a navigation property eléréseket joinokká alakítja, egyetlen SQL lekérdezésben – vetítéskor nincs szükség `Include`-ra.

??? example "2. feladat – Az N+1 lekérdezés probléma"
    **Feladat:** Az alábbi kód egy lekérdezést futtat a termékekért, majd **termékenként egy további** lekérdezést a kategóriáért. Magyarázd el, miért, és írd át úgy, hogy egyetlen körfordulóban töltsön be mindent!

    ```csharp
    var products = db.Products.ToList();
    foreach (var p in products)
        Console.WriteLine($"{p.Name} - {p.Category.Name}");
    ```

    ??? success "Megoldás"
        Az első lekérdezés csak a termékeket tölti be; a `Category` navigation property **nem** töltődik be eagerly, így minden `p.Category.Name` elérés külön lekérdezést indít (lazy/explicit betöltéssel) – ez a klasszikus **N+1 probléma**. Eager loadinggal javítható:

        ```csharp
        var products = db.Products
            .Include(p => p.Category)
            .ToList();
        foreach (var p in products)
            Console.WriteLine($"{p.Name} - {p.Category.Name}");
        ```

        Az `Include` egyetlen `LEFT JOIN`-t generál, így a termék és a kategória adat egyetlen lekérdezésben érkezik.

??? example "3. feladat – Módosítás navigation property-n keresztül"
    **Feladat:** Helyezz át minden terméket a „Toys" nevű kategóriából a „Games" nevű kategóriába (tételezd fel, hogy mindkettő létezik)!

    ??? success "Megoldás"
        ```csharp
        using var db = new AdatvezDbContext();

        var games = db.Categories.Single(c => c.Name == "Games");
        var toysProducts = db.Products
            .Where(p => p.Category.Name == "Toys")
            .ToList();

        foreach (var p in toysProducts)
            p.CategoryID = games.ID;   // a change tracker rögzíti a módosítást

        db.SaveChanges();              // soronként egy UPDATE, egy tranzakcióban
        ```

??? example "4. feladat – Explicit betöltés"
    **Feladat:** Már betöltöttél egy `Product`-ot, és később úgy döntesz, szükséged van a `VAT`-jára. Töltsd be a kapcsolódó entitást globális lazy loading **bekapcsolása nélkül** és a termék **újra-lekérdezése nélkül**!

    ??? success "Megoldás"
        ```csharp
        var product = db.Products.First();
        // product.VAT itt még null

        db.Entry(product).Reference(p => p.VAT).Load();
        Console.WriteLine(product.VAT.Percentage); // most már betöltve
        ```

        Gyűjtemény-navigációhoz `.Reference(...)` helyett `.Collection(...)`-t használj.

??? example "5. feladat – Kapcsolat konfigurálása (Fluent API)"
    **Feladat:** Az `OnModelCreating`-ben konfiguráld explicit módon az egy-a-többhöz kapcsolatot a `Category` (egy) és a `Product` (több) között, `Product.CategoryID` idegen kulccsal!

    ??? success "Megoldás"
        ```csharp
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID);
        }
        ```

??? example "6. feladat – A generált SQL átgondolása"
    **Feladat:** Mi a különbség az alábbi két lekérdezés között a generált SQL és a visszaadott adat szempontjából?

    ```csharp
    var a = db.Products.Where(p => p.Name.Contains("Lego")).Select(p => p.VAT.Percentage);
    var b = db.Products.Include(p => p.VAT).Where(p => p.Name.Contains("Lego"));
    ```

    ??? success "Megoldás"
        - Az `a` lekérdezés egyetlen skalárt (`Percentage`) **vetít**, így az SQL csak ezt az oszlopot választja ki egy `LEFT JOIN`-nal a `VAT`-ra; az eredmény `IQueryable<int>`.
        - A `b` lekérdezés teljes `Product` entitásokat ad vissza, a `VAT`-juk **betöltve**; az SQL minden `Product` és `VAT` oszlopot kiválaszt `LEFT JOIN`-nal, `IQueryable<Product>`-et adva vissza.

        A generált SQL megtekintéséhez használd a `ToQueryString()`-et:

        ```csharp
        Console.WriteLine(a.ToQueryString());
        ```

---

### MongoDB műveletek

Az alábbi feladatok a `MongoDB` anyagához és a MongoDB-gyakorlathoz kapcsolódnak. A `Product` leképezést a jegyzetből feltételezzük (`Name`, `Price`, `Stock`, `string[] Categories`, beágyazott `VAT { VATCategoryName, Percentage }`). Ahol releváns, a MongoDB Shell és a .NET Driver szintaktika is szerepel.

??? example "1. feladat – Összetett szűrő builderrel"
    **Feladat:** Kérd le az összes olyan terméket, amely a „Balls" kategóriában van, ára 500 és 1000 közé esik (beleértve), **és** az ÁFA-százaléka nem 27! Írd meg a .NET Driver builder verziót!

    ??? success "Megoldás (.NET Driver)"
        ```csharp
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.AnyEq(p => p.Categories, "Balls"),
            Builders<Product>.Filter.Gte(p => p.Price, 500),
            Builders<Product>.Filter.Lte(p => p.Price, 1000),
            Builders<Product>.Filter.Ne(p => p.VAT.Percentage, 27)
        );
        var result = collection.Find(filter).ToList();
        ```

??? example "2. feladat – Tömbmező szűrése"
    **Feladat:** Találd meg azokat a termékeket, amelyek **legalább egy** kategóriába tartoznak a „Balls" és „Rackets" körén kívül! Majd találd meg azokat, amelyek **egyszerre** a „Balls" és az „Outdoor" kategóriában is benne vannak!

    ??? success "Megoldás (.NET Driver)"
        ```csharp
        // legalább egy kategória, amely nem szerepel a listán
        var notListed = collection.Find(
            Builders<Product>.Filter.AnyNin(p => p.Categories, new[] { "Balls", "Rackets" })).ToList();

        // mindkét kategóriát tartalmazza
        var both = collection.Find(
            Builders<Product>.Filter.All(p => p.Categories, new[] { "Balls", "Outdoor" })).ToList();
        ```

        A Shell megfelelők `$nin` (tömbökre `$elemMatch` szemantikával) és `$all`.

??? example "3. feladat – Beágyazott dokumentum és létezés vizsgálata"
    **Feladat:** Listázd azokat a termékeket, amelyekhez **van** beágyazott `VAT` dokumentum, amelynek százaléka kisebb, mint 27! Ügyelj arra, hogy a hiányzó beágyazott dokumentum ne okozzon hibát!

    ??? success "Megoldás (.NET Driver)"
        ```csharp
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Exists(p => p.VAT),
            Builders<Product>.Filter.Lt(p => p.VAT.Percentage, 27)
        );
        var result = collection.Find(filter).ToList();
        ```

??? example "4. feladat – Részleges frissítés több operátorral"
    **Feladat:** A „Clearance" kategóriában lévő minden terméknél: csökkentsd a készletet 1-gyel, állíts be egy új `OnSale = true` mezőt, és add hozzá a „Discounted" elemet a `Categories` tömbhöz (duplikáció nélkül)! Tedd ezt egyetlen `UpdateMany` hívásban!

    ??? success "Megoldás (.NET Driver)"
        ```csharp
        var filter = Builders<Product>.Filter.AnyEq(p => p.Categories, "Clearance");
        var update = Builders<Product>.Update
            .Inc(p => p.Stock, -1)
            .Set("OnSale", true)
            .AddToSet(p => p.Categories, "Discounted");

        collection.UpdateMany(filter, update);
        ```

        Az `$inc` egy (esetleg negatív) értékkel növeli a mezőt; az `$addToSet` elkerüli a tömb-duplikációkat (ellentétben a `$push`-sal).

??? example "5. feladat – Aggregációs pipeline"
    **Feladat:** A „Balls" kategóriában lévő termékekre csoportosíts ÁFA-százalék szerint, és add vissza csoportonként a **termékek számát** és a **teljes készletet**, a teljes készlet szerint csökkenő sorrendben!

    ??? success "Megoldás (.NET Driver)"
        ```csharp
        var result = collection.Aggregate()
            .Match(Builders<Product>.Filter.AnyEq(p => p.Categories, "Balls"))
            .Group(
                p => p.VAT.Percentage,
                g => new
                {
                    Percentage = g.Key,
                    Count = g.Count(),
                    TotalStock = g.Sum(p => p.Stock)
                })
            .SortByDescending(x => x.TotalStock)
            .ToList();
        ```

    ??? success "Megoldás (MongoDB Shell)"
        ```javascript
        db.products.aggregate([
            { $match: { categories: "Balls" } },
            { $group: { _id: "$vat.percentage",
                        count: { $sum: 1 },
                        totalStock: { $sum: "$stock" } } },
            { $sort: { totalStock: -1 } }
        ])
        ```

??? example "6. feladat – Rendezett lapozás"
    **Feladat:** Add vissza a termékek harmadik oldalát (20-as oldalméret), név szerint növekvő, majd ár szerint csökkenő sorrendben!

    ??? success "Megoldás (.NET Driver)"
        ```csharp
        var page = collection.Find(Builders<Product>.Filter.Empty)
            .Sort(Builders<Product>.Sort.Combine(
                Builders<Product>.Sort.Ascending(p => p.Name),
                Builders<Product>.Sort.Descending(p => p.Price)))
            .Skip(40)   // az 1. és 2. oldal = 40 dokumentum
            .Limit(20)
            .ToList();
        ```

        `Sort` nélkül a `Skip`/`Limit` nem determinisztikus.

??? example "7. feladat – ReplaceOne vs. UpdateOne (elméleti)"
    **Feladat:** Magyarázd el a különbséget a `ReplaceOne` és az `UpdateOne`/`$set` között! Mi történik azokkal a mezőkkel, amelyeket nem adsz meg?

    ??? success "Megoldás"
        - A `ReplaceOne` a teljes egyező dokumentumot lecseréli az újra (megőrzi az `_id`-t). Minden mező, amely nem szerepel a csere-dokumentumban, **elveszik**.
        - Az `UpdateOne` `$set`-tel (és más frissítési operátorokkal) **csak** a megadott mezőket módosítja, az összes többit érintetlenül hagyja. Ez a biztonságos választás részleges módosításhoz, és nagy dokumentumokon hatékonyabb is.

---

### Tranzakciókezelés

Az alábbi feladatok a `Transactions` anyagához kapcsolódnak, lefedve az izolációs problémákat, izolációs szinteket, zárolást/holtpontokat és tranzakció-naplózást.

??? example "1. feladat – Az anomália azonosítása"
    **Feladat:** Két tranzakció párhuzamosan fut:

    - T1: olvassa az 5. termék készletét (10-et kap).
    - T2: olvassa az 5. termék készletét (10-et kap), 8-ra állítja, véglegesít.
    - T1: 9-re állítja a készletet (10 − 1), véglegesít.

    Melyik izolációs anomália lépett fel? Mi a végső készletérték, és mi lett volna a helyes érték?

    ??? success "Megoldás"
        Ez egy **elveszett módosítás (lost update)**. A végső készlet 9 (T1 írása), de T2 8-ra való csökkentése felülíródott, mintha meg sem történt volna. Mindkét csökkentéssel a helyes érték 7 lenne (vagy legalább T2 véglegesített 8-a nem tűnhetett volna el csendben). Megelőzése: `REPEATABLE READ`/`SERIALIZABLE`, pesszimista zárolás vagy optimista egyidejűség-kezelés (sorverzió-ellenőrzés frissítéskor).

??? example "2. feladat – Izolációs szint kiválasztása"
    **Feladat:** Egy riport összegzi az összes rendelés értékét, több lekérdezésben iterálva a rendelési tételeken. A riport futása közben pénz nem tűnhet el és nem jelenhet meg, tehát **sem új rendelési tételek nem válhatnak láthatóvá, sem az egyszer már olvasott sorok nem változhatnak**. Melyik izolációs szint szükséges, és miért nem elegendő az eggyel alacsonyabb?

    ??? success "Megoldás"
        A **SERIALIZABLE** szint szükséges. A `REPEATABLE READ` megakadályozná, hogy a már olvasott sorok változzanak (nincs dirty/non-repeatable read), de még mindig lehetővé teszi **fantomsorokat** – a lekérdezésnek megfelelő, újonnan beszúrt rendelési tételek megjelenhetnek az ugyanazon tranzakción belüli egy későbbi lekérdezésben. Csak a `SERIALIZABLE` véd a fantomok ellen is.

??? example "3. feladat – Optimista vs. pesszimista egyidejűség-kezelés"
    **Feladat:** Magyarázd el, hogyan akadályoznád meg az 1. feladat elveszett módosítását (a) pesszimista és (b) optimista egyidejűség-kezeléssel! Mindkettőhöz nevezz meg egy kompromisszumot!

    ??? success "Megoldás"
        - **Pesszimista:** olvasáskor zárolj (`UPDLOCK` hint vagy magasabb izolációs szint), hogy a másik tranzakció várjon, amíg az első befejeződik. Kompromisszum: csökkentett párhuzamosság/áteresztőképesség és holtpont-kockázat.
        - **Optimista:** ne zárolj; tarts fenn egy verzió/időbélyeg oszlopot, és frissítéskor ellenőrizd, hogy a sor nem változott az olvasás óta (`WHERE Version = @readVersion`). Ha 0 sor módosult, valaki más változtatta meg – próbáld újra. Kompromisszum: felesleges munka és újrapróbálási ciklus magas versenyfeltételek esetén.

??? example "4. feladat – Tranzakciónaplózás nyomkövetése"
    **Feladat:** Egy tranzakció A-t 2-vel csökkenti (10→8), B-t 2-vel növeli (20→22). **Undo naplózással** mit írnak a naplóba a két íráshoz, és milyen sorrendben kell a napló kiürítésének és az adatbázis-írásoknak megtörténniük a véglegesítéskor?

    ??? success "Megoldás"
        Az undo naplózás az **eredeti** értékeket rögzíti:

        | Művelet | Naplóbejegyzés |
        |---|---|
        | Write(A) | `T1, A, 10` |
        | Write(B) | `T1, B, 20` |

        A véglegesítés sorrendje: **először a napló kiürítése**, majd A és B kiírása az adatbázisfájlokba, végül a `Commit T1` jelzés írása. Visszaállításkor minden tranzakció, amelynek nincs commit jelzése, a naplóban tárolt eredeti értékekkel visszavonódik. (Összehasonlításképpen: a redo naplózás a végső értékeket, 8-at és 22-t tárolja, és a commit jelzést az adatbázisfájlok írása *előtt* rögzíti.)

??? example "5. feladat – Holtpont diagnózis MSSQL-ben"
    **Feladat:** Két tranzakció ellentétes sorrendben frissíti a `Lefty` és `Righty` táblákat, és holtpontba kerülnek. Melyik DMV-lekérdezések mutatják meg (a) az egyes munkamenetek által tartott zárolásokat, és (b) melyik munkamenet blokkolja a másikat? Mit csinál az SQL Server automatikusan?

    ??? success "Megoldás"
        (a) Aktuális zárolások és a táblák, amelyeken ülnek:

        ```sql
        SELECT OBJECT_NAME(P.object_id) AS TableName,
               Resource_type, request_status, request_session_id
        FROM sys.dm_tran_locks dtl
             JOIN sys.partitions P ON dtl.resource_associated_entity_id = p.hobt_id
        ```

        (b) Blokkolási kapcsolatok:

        ```sql
        SELECT blocking_session_id AS BlockingSessionID,
               session_id AS VictimSessionID,
               wait_time/1000 AS WaitDurationSecond
        FROM sys.dm_exec_requests
        WHERE blocking_session_id > 0
        ```

        Az SQL Server holtpont-monitora detektálja a ciklust, és **megszakítja az egyik tranzakciót (a holtpont áldozatát)**, visszagörgetvén minden módosítását. Az alkalmazásnak el kell kapnia ezt a hibát, és **újra kell próbálkoznia**. A holtpontok valószínűsége csökkenthető, ha mindig **ugyanolyan sorrendben** zárolunk erőforrásokat.

---

## Önellenőrző kérdések – AI-promptok témakörönként

Az alábbi promptokat főleg önálló tanuláskor, szóbeli kérdések szimulálásához használd.

### SQL és MSSQL

```
Kérdezz ki engem az alábbi MSSQL témákból:
- JOIN típusok (INNER, LEFT, RIGHT, FULL OUTER)
- GROUP BY + HAVING különbsége WHERE-rel
- Tárolt eljárások és triggerek
- Indexek szerepe és típusai

Tegyél fel egymás után 3 kérdést, és minden válaszom után értékeld azt!
```

### Entity Framework

```
Kérdezz ki az Entity Framework Core következő témáiból:
- Code-First megközelítés és migrációk
- Navigation property-k és Include() szerepe
- Change tracker és SaveChanges() működése
- N+1 query probléma és hogyan kerülhető el

Tegyél fel egymás után 3 kérdést!
```

### MongoDB

```
Kérdezz ki a MongoDB következő témáiból:
- Dokumentum- vs. relációs adatmodell különbségei
- Beágyazás vs. referencia – mikor melyiket?
- Aggregációs pipeline főbb stage-jei ($match, $group, $sort, $lookup)
- Indexek MongoDB-ben

Tegyél fel egymás után 3 kérdést!
```

### Tranzakciókezelés

```
Kérdezz ki a tranzakciókezelés következő témáiból:
- ACID tulajdonságok magyarázata
- Izolációs szintek és a köztük lévő anomáliák (dirty read, non-repeatable read, phantom read)
- Holtpontok és megelőzésük
- Optimista vs. pesszimista konkurenciakezelés

Tegyél fel egymás után 3 kérdést!
```

---

## Tippek a hatékony felkészüléshez

!!! tip "Vizsganapon"
    - Olvasd végig a feladatot kétszer, mielőtt jelölnél bármit.
    - Egyszerűbb megoldásokat keress – vizsgán a bonyolult trükk helyett az olvasható és helyes kód a cél.
    - Ha sszükséges, készíts előszőr vázlatot és a végső optimalizált megoldást vidd fel a feladatlapra.
    - Ha SQL-feladatban bizonytalan vagy, gondold végig a `FROM` → `WHERE` → `GROUP BY` → `HAVING` → `SELECT` sorrend modellt.
