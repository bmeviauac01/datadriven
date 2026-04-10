# Vizsgafelkészülés és AI-asszisztált gyakorlás

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

Az alábbi feladatok a `lecture-notes/mssql/sql.md` és az `lecture-notes/transactions/index.md` témakörökhöz, valamint a tranzakciókezeléses és MSSQL gyakorlatokhoz kapcsolódnak.

??? example "1. feladat – Szűrés, rendezés"
    **Feladat:** Listázd ki azon termékek nevét és árát, amelyek ára 1000 és 5000 Ft közé esik, és van leírásuk! Az eredményt ár szerint csökkenő sorrendben jelenítsd meg!

    ??? success "Megoldás"
        ```sql
        SELECT Name, Price
        FROM Product
        WHERE Price BETWEEN 1000 AND 5000
          AND Description IS NOT NULL
        ORDER BY Price DESC
        ```

??? example "2. feladat – Aggregáció, csoportosítás"
    **Feladat:** Kategóriánként add meg a termékek átlagárát és darabszámát! Csak azokat a kategóriákat listázd, amelyekben legalább 3 termék van!

    ??? success "Megoldás"
        ```sql
        SELECT c.Name, COUNT(p.ID) AS TermekSzam, AVG(p.Price) AS AtlagAr
        FROM Category c
             INNER JOIN Product p ON p.CategoryID = c.ID
        GROUP BY c.Name
        HAVING COUNT(p.ID) >= 3
        ```

??? example "3. feladat – Összekapcsolás, al-lekérdezés"
    **Feladat:** Melyek azok a megrendelési tételek, ahol a tétel rögzített ára eltér a termék aktuális árától?

    ??? success "Megoldás"
        ```sql
        SELECT oi.ID, p.Name, oi.Price AS RogzitettAr, p.Price AS AktualisAr
        FROM OrderItem oi
             INNER JOIN Product p ON oi.ProductID = p.ID
        WHERE oi.Price <> p.Price
        ```

??? example "4. feladat – Outer join"
    **Feladat:** Listázd ki az összes fizetési módot, és mellé írd, hány megrendelés érkezett azzal a fizetési móddal! Azok is szerepeljenek, amelyekhez még nem érkezett rendelés!

    ??? success "Megoldás"
        ```sql
        SELECT pm.Method, COUNT(o.ID) AS MegrendelesekSzama
        FROM PaymentMethod pm
             LEFT OUTER JOIN [Order] o ON o.PaymentMethodID = pm.ID
        GROUP BY pm.Method
        ```

??? example "5. feladat – Tárolt eljárás írása"
    **Feladat:** Írj tárolt eljárást, amely paraméterként vár egy vevő-azonosítót, és visszaadja az adott vevő összes megrendelésének összértékét (az `OrderItem.Amount * OrderItem.Price` szorzatok összege)!

    ??? success "Megoldás"
        ```sql
        CREATE OR ALTER PROCEDURE GetCustomerOrderTotal
            @CustomerID INT
        AS
        BEGIN
            SELECT SUM(oi.Amount * oi.Price) AS OsszesenFt
            FROM [Order] o
                 INNER JOIN OrderItem oi ON oi.OrderID = o.ID
            WHERE o.CustomerID = @CustomerID
        END
        ```

---

### LINQ lekérdezések

Az alábbi feladatok a `lecture-notes/linq/index.md` anyagához kapcsolódnak.

??? example "1. feladat – Szűrés és vetítés"
    **Feladat:** LINQ segítségével kérd le azon termékek nevét és árát, amelyek ára kevesebb, mint 2000 Ft!

    ??? success "Megoldás"
        ```csharp
        // Fluent szintaktika
        var eredmeny = products.Where(p => p.Price < 2000)
                               .Select(p => new { p.Name, p.Price });

        // Query szintaktika
        var eredmeny = from p in products
                       where p.Price < 2000
                       select new { p.Name, p.Price };
        ```

??? example "2. feladat – Join"
    **Feladat:** Kapcsold össze a `products` és `vat` listákat, és jelenítsd meg minden termék neve mellé az ÁFA-százalékot!

    ??? success "Megoldás"
        ```csharp
        var eredmeny = from p in products
                       join v in vat on p.VATID equals v.ID
                       select new { p.Name, v.Percentage };
        ```

??? example "3. feladat – Csoportosítás"
    **Feladat:** Csoportosítsd a termékeket ÁFA-azonosító szerint, és adj meg minden csoporthoz termékszámot és átlagárat!

    ??? success "Megoldás"
        ```csharp
        var eredmeny = products
            .GroupBy(p => p.VATID)
            .Select(g => new
            {
                VatID    = g.Key,
                Darab    = g.Count(),
                AtlagAr  = g.Average(p => p.Price)
            });
        ```

---

### Entity Framework Core

Az alábbi feladatok a `lecture-notes/ef/index.md` és az EF-gyakorlat anyagához kapcsolódnak.

??? example "1. feladat – Lekérdezés navigation property-vel"
    **Feladat:** EF Core segítségével kérd le azon megrendelések listáját (beleértve a megrendelő nevét), amelyek státusza „Delivered"!

    ??? success "Megoldás"
        ```csharp
        using (var db = new AdatvezDbContext())
        {
            var megrendelesek = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Status)
                .Where(o => o.Status.Name == "Delivered")
                .Select(o => new { o.Customer.Name, o.Date })
                .ToList();
        }
        ```

??? example "2. feladat – Módosítás"
    **Feladat:** EF Core-ral növeld meg 10%-kal azon termékek árát, amelyek raktárkészlete 0!

    ??? success "Megoldás"
        ```csharp
        using (var db = new AdatvezDbContext())
        {
            var termekek = db.Products.Where(p => p.Stock == 0).ToList();
            foreach (var t in termekek)
                t.Price = (int)(t.Price * 1.1);
            db.SaveChanges();
        }
        ```

??? example "3. feladat – Új rekord felvitele"
    **Feladat:** Végy fel egy új kategóriát „Elektronika" névvel, amely nem alkategória (nincs szülőkategóriája)!

    ??? success "Megoldás"
        ```csharp
        using (var db = new AdatvezDbContext())
        {
            db.Categories.Add(new Category { Name = "Elektronika", ParentCategoryID = null });
            db.SaveChanges();
        }
        ```

---

### MongoDB műveletek

Az alábbi feladatok a `lecture-notes/mongodb/index.md` és a MongoDB-gyakorlat anyagához kapcsolódnak. A minták a MongoDB .NET Driver-t és a MongoDB Shell szintaktikát egyaránt megmutatják.

??? example "1. feladat – Szűrés"
    **Feladat:** Kérd le azon termékeket, amelyek ára nagyobb, mint 1000, és kategóriájuk neve „Könyvek"!

    ??? success "Megoldás (MongoDB Shell)"
        ```javascript
        db.products.find({
            price: { $gt: 1000 },
            "category.name": "Könyvek"
        })
        ```

    ??? success "Megoldás (.NET Driver)"
        ```csharp
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Gt(p => p.Price, 1000),
            Builders<Product>.Filter.Eq(p => p.Category.Name, "Könyvek")
        );
        var termekek = collection.Find(filter).ToList();
        ```

??? example "2. feladat – Frissítés"
    **Feladat:** Növeld meg 500 Ft-tal azon dokumentumok árát, amelyekben a `Stock` értéke 0!

    ??? success "Megoldás (MongoDB Shell)"
        ```javascript
        db.products.updateMany(
            { stock: 0 },
            { $inc: { price: 500 } }
        )
        ```

    ??? success "Megoldás (.NET Driver)"
        ```csharp
        var filter = Builders<Product>.Filter.Eq(p => p.Stock, 0);
        var update = Builders<Product>.Update.Inc(p => p.Price, 500);
        collection.UpdateMany(filter, update);
        ```

??? example "3. feladat – Aggregációs pipeline"
    **Feladat:** Kategóriánként számold meg a termékeket, és listázd ki csökkenő sorrendben!

    ??? success "Megoldás (MongoDB Shell)"
        ```javascript
        db.products.aggregate([
            { $group: { _id: "$category.name", darab: { $sum: 1 } } },
            { $sort:  { darab: -1 } }
        ])
        ```

---

### Tranzakciókezelés

Az alábbi feladatok a `lecture-notes/transactions/index.md` anyagához kapcsolódnak.

??? example "1. feladat – Izolációs szint megértése"
    **Feladat:** Mi a különbség a `READ COMMITTED` és a `REPEATABLE READ` izolációs szint között? Mikor melyiket érdemes használni?

    ??? success "Megoldás"
        - **READ COMMITTED**: egy tranzakció csak a már véglegesített adatokat látja. Előfordulhat *non-repeatable read*: ugyanannak a sornak kétszeri olvasása eltérő eredményt adhat, ha egy másik tranzakció közben módosított.
        - **REPEATABLE READ**: ha egy tranzakció egyszer már olvasott egy sort, azt a sort más tranzakció nem módosíthatja addig, amíg az első be nem fejeződik. Megakadályozza a *non-repeatable read*-et, de a *phantom read* még előfordulhat.

        **Mikor melyiket?** Ha az üzleti logika megköveteli, hogy egy tranzakció futása alatt az általa olvasott sorok ne változhassanak (pl. kétlépéses ellenőrzés-módosítás), akkor `REPEATABLE READ`-et vagy `SERIALIZABLE`-t érdemes alkalmazni.

??? example "2. feladat – Holtpont azonosítása"
    **Feladat:** Két párhuzamos tranzakciót látsz:

    - T1: zárol A táblát, majd B táblát kér
    - T2: zárol B táblát, majd A táblát kér

    Mi történik? Hogyan lehet megelőzni?

    ??? success "Megoldás"
        Ez klasszikus **holtpont (deadlock)**: T1 és T2 egymás által tartott erőforrásra vár, így egyik sem tud haladni.

        **Megelőzés:** mindig azonos sorrendben zárolj erőforrásokat (pl. minden tranzakció előbb A-t, aztán B-t kérje). Az MSSQL szerver automatikusan detektálja a holtpontot és az egyik tranzakciót visszagörgeti (`deadlock victim`).

---

## Önellenőrző kérdések – AI-promptok témakörönként

Az alábbi promptokat főleg önálló tanuláskor, szóbeli kérdések szimulálásához használd.

### SQL és MSSQL

```
Kérdezz ki engem szóban az alábbi MSSQL témákból:
- JOIN típusok (INNER, LEFT, RIGHT, FULL OUTER)
- GROUP BY + HAVING különbsége WHERE-rel
- Tárolt eljárások és triggerek
- Indexek szerepe és típusai

Tegyél fel egymás után 3 kérdést, és minden válaszom után értékeld azt!
```

### Entity Framework

```
Kérdezz ki az Entity Framework Core következő témáiból szóban:
- Code-First megközelítés és migrációk
- Navigation property-k és Include() szerepe
- Change tracker és SaveChanges() működése
- N+1 query probléma és hogyan kerülhető el

Tegyél fel egymás után 3 kérdést!
```

### MongoDB

```
Kérdezz ki a MongoDB következő témáiból szóban:
- Dokumentum- vs. relációs adatmodell különbségei
- Beágyazás vs. referencia – mikor melyiket?
- Aggregációs pipeline főbb stage-jei ($match, $group, $sort, $lookup)
- Indexek MongoDB-ben

Tegyél fel egymás után 3 kérdést!
```

### Tranzakciókezelés

```
Kérdezz ki a tranzakciókezelés következő témáiból szóban:
- ACID tulajdonságok magyarázata
- Izolációs szintek és a köztük lévő anomáliák (dirty read, non-repeatable read, phantom read)
- Holtpontok és megelőzésük
- Optimista vs. pesszimista konkurenciakezelés

Tegyél fel egymás után 3 kérdést!
```

---

## Tippek a hatékony felkészüléshez

!!! tip "Napi rutinhoz"
    1. **Rövid, ismétlő munkamenetek**: inkább napi 30 perc, mint egyetlen maratoni éjszaka.
    2. **Futtasd le a kódot**: minden feladatot próbálj ki a saját gépen – az MSSQL-eseket SQL Server Management Studio-ban, a .NET-eseket Visual Studio-ban, a MongoDB-seket Compass-ban vagy shell-ben.
    3. **Hibakeresés AI-jal**: ha hibás a kód, másold be az AI-ba a hibaüzenettel együtt – pár másodperc alatt magyarázatot kapsz.
    4. **Magyarázd el hangosan**: az "explain it to me" technika nagyon hatékony – kérd az AI-t, hogy magyarázza el a témát egyszerűen, majd te is próbáld meg visszamondani.

!!! tip "Vizsganapon"
    - Olvasd végig a feladatot kétszer, mielőtt jelölnél bármit.
    - Egyszerűbb megoldásokat keress – vizsgán a bonyolult trükk helyett az olvasható és helyes kód a cél.
    - Ha SQL-feladatban bizonytalan vagy, gondold végig a `FROM` → `WHERE` → `GROUP BY` → `HAVING` → `SELECT` sorrend modellt.
