# REST API & ASP.NET Web API

A gyakorlat célja, hogy a hallgatók gyakorolják a REST API-k tervezését, és megismerjék a .NET Web API technológiáját.

## Előfeltételek

A labor elvégzéséhez szükséges eszközök:

- Microsoft Visual Studio 2022 (_nem_ VS Code)
- Microsoft SQL Server (LocalDB vagy Express edition)
- SQL Server Management Studio
- Postman: <https://www.getpostman.com/downloads/>
- Adatbázis létrehozó script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/adatvezerelt/master/docs/db/mssql.sql)
- Kiinduló alkalmazás kódja: <https://github.com/bmeviauac01/gyakorlat-rest-kiindulo>

Amit érdemes átnézned:

- C# nyelv
- Entity Framework és Linq
- REST API és Web API előadás

## Gyakorlat menete

A gyakorlat végig vezetett, a gyakorlatvezető utasításai szerint haladjunk. Egy-egy részfeladatot próbáljunk meg először önállóan megoldani, utána beszéljük meg a megoldást közösen. Az utolsó és utolsó előtti feladat opcionális, ha belefér az időbe.

!!! info ""
    Emlékeztetőként a megoldások is megtalálhatóak az útmutatóban is. Előbb azonban próbáljuk magunk megoldani a feladatot!

## 0. Feladat: Adatbázis létrehozása, ellenőrzése

Az adatbázis az adott géphez kötött, ezért nem biztos, hogy a korábban létrehozott adatbázis most is létezik. Ezért először ellenőrizzük, és ha nem találjuk, akkor hozzuk létre újra az adatbázist. (Ennek mikéntjét lásd az [első gyakorlat anyagában](../transactions/index.md).)

## 1. Feladat: Projekt megnyitása

1. Töltsük le a méréshez tartozó projekt vázat!

    - Nyissunk egy _command prompt_-ot
    - Navigáljunk el egy tetszőleges mappába, például `c:\work\NEPTUN`
    - Adjuk ki a következő parancsot: `git clone --depth 1 https://github.com/bmeviauac01/gyakorlat-rest-kiindulo.git`

1. Nyissuk meg a leklónozott könyvtár alatti _sln_ fájlt Visual Studio-val.

1. Vizsgáljuk meg a projektet.

    - Ez egy ASP.NET Core Web API projekt. Kifejezetten REST API-k kiszolgálásához készült. Ha F5-tel elindítjuk, akkor magában tartalmaz egy webszervert a kérések kiszolgálásához.
    - Nézzük meg a `Program.cs` tartalmát. Lényegében két részből áll:
      - Létrehoz egy `WebApplicationBuilder` objektumot, amelynek a `Services` tulajdonságán keresztül tudjuk konfigurálni a Dependency Injection konténert.
      - `Build` után az ASP.NET Core middleware pipeline-t tudjuk konfigurálni, ahol jelenleg csak a controllerek támogatását találhatjuk. Majd futtatjuk ezt az alkalmazást egy beágyazott webszerver (Kestrel) segítségével.
    - Az adatbázisunk Entity Framework leképzése (_Code First_ modellel) megtalálható a `Dal` mappában. Az `DataDrivenDbContext` lesz az elérés központi osztálya. - A _connection string_ az alkalmazás konfigurációs állományában az `appsettings.json`-ben található.
    - A `Controllers` mappában már van egy teszt controller. Nyissuk meg és vizsgáljuk meg. Vegyük észre az `[ApiController]` és `[Route]` attribútumokat, valamint a leszármazást. Ettől lesz egy osztály _Web API controller_. Minden további automatikusan működik, a controller metódusai a megadott kérésekre (az útvonal és http metódus függvényében) meg fognak hívódni (tehát nincs további konfigurációra szükség).

1. Írjuk át az `appsettings.json` állományban az adatbázisunk nevét a connection string-ben a neptun kódunkra.

1. Indítsuk el az alkalmazást. Fordítás után egy konzol alkalmazás indul el (böngészőt most nem indít automatikusan), ahol látjuk a logokat. Nyissunk egy böngészőt, és a <http://localhost:5000/api/values> címet írjuk be. Kapnunk kell egy JSON választ. Állítsuk le az alkalmazást: vagy _Ctrl-C_ a konzol alkalmazásban, vagy Visual Studio-ban állítsuk le.

## 2. Feladat: Első Controller és metódus, tesztelés Postmannel

Készítsünk egy új Web API controllert, ami visszaad egy üdvözlő szöveget. Próbáljuk ki a működést Postman használatával.

1. Töröljük ki a `ValuesController` osztályt. Adjuk hozzá helyette egy új _Api Controller_-t üresen `HelloController` néven: a _Solution Explorer_-ben a _Controllers_ mappára jobb egérrel kattintva _Add / Controller... / API Controller - Empty_. A `HelloController` a `/api/hello` url alatt legyen elérhető.
1. Készítsünk egy `GET` kérésre válaszoló metódust, ami egy szöveggel tér vissza. Próbáljuk ki Postman-nel: a GET kérést <http://localhost:5000/api/hello> címre kell küldenünk.
1. Módosítsuk a REST kérést kiszolgáló metódust úgy, hogy opcionálisan fogadjon el egy nevet _query paraméterben_, azaz az urlben, és ha kap ilyet, akkor a válasza legyen "Hello" + a kapott név. Próbáljuk ki ezt is Postmannel: Ha adunk nevet, akkor azt a <http://localhost:5000/api/hello?name=alma> url-je küldjük.
1. Végül készítsünk egy _új_ REST Api végpontot (új függvényt), ami a <http://localhost:5000/api/hello/alma> url-en fog válaszolni pont úgy, ahogy az előző is tette (csak most a név a _path_ része).

??? example "Megoldás"
    ```csharp
    [Route("api/[controller]")]
    [ApiController]
    public class HelloController
    {
        // 2. alfeladat
        //[HttpGet]
        //public string Hello()
        //{
        //    return "Hello!";
        //}

        // 3. alfeladat
        [HttpGet]
        public string Hello([FromQuery] string name)
        {
            return string.IsNullOrEmpty(name)
                ? "Hello noname!"
                : $"Hello {name}";
        }

        // 4. alfeladat
        [HttpGet("{personName}")] // a route-ban a {} közötti név meg kell egyezzen a paraméter nevével
        public string HelloRoute(string personName)
        {
            return "Hello route " + personName;
        }
    }
    ```

    Foglaljuk össze, mi kell ahhoz, hogy egy WebAPI végpontot készítsünk:

    - Leszármazni a `ControllerBase`-ből és az `[ApiController]` attribútumot rátenni az osztályra.
    - Megadni, milyen http kérésre válaszol a végpont a megfelelő `[Http*]` attribútummal.
    - Megadni a route-ot, akár az osztályon, akár a metóduson (vagy mindkettőn) a `[Route]` vagy a `[HttpXXX]` attribútummal.
    - Megfelelő formájú metódust készíteni (pl. visszatérési érték, paraméterek).

## 3. Feladat: Termékek keresése API

Egy valódi API természetesen nem konstansokat ad vissza. Készítsünk API-t a webshopban árult termékek közötti kereséshez.

- Készítsünk ehhez egy új controller-t.
- Lehessen listázni a termékeket, de csak lapozva (max 5 elem minden lapon).
- Lehessen keresni termék névre.
- A visszaadott termék entitás _ne_ az adatbázis leképzésből jövő entitás legyen, hanem készítsünk egy új, ún. _DTO_ (data transfer object) `record` osztályt egy új, `Dtos` mappában.

### DTO-k használata

A visszaadott termék entitás _ne_ az adatbázis leképzésből jövő entitás legyen, hanem készítsünk egy új, ún. _DTO_ (data transfer object) osztályt egy új, `Dtos` mappában. Készítsünk `Product` néven egy rekord osztályt a DTO számára.

!!! tip "Rekordok C#-ban"
    A `record` kulcsszó egy olyan típust reprezentál (alapértelmezetten class), ami a fejlécben meghatározott konstruktorral és [`init` only setterrel](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/init) rendelkező tulajdonságokkal rendelkezik. Ezáltal egy record immutable viselkedéssel bír, ami jobban illeszkedik egy DTO viselkedéséhez. A rekordok ezen kívül egyéb kényelmi szolgáltatásokkal is rendelkeznek ([lásd bővebben](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)), de ezeket mi nem fogjuk itt kihasználni.

??? example "Megoldás"

    ```csharp title="Dtos/Product.cs"
    namespace Bme.DataDriven.Rest.Dtos;

    public record Product(int Id, string Name, double? Price, int? Stock);
    ```
### Listázó végpont készítése

Készítsük el a követelményeknek megfelelő végpontot egy új `ProductController` osztályban, majd próbáljuk ki az alkalmazást.

??? example "Megoldás"

    ```csharp
    using Microsoft.AspNetCore.Mvc;

    namespace Bme.DataDriven.Rest.Controllers;

    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly Dal.DataDrivenDbContext _dbContext;

        // Az adatbazist igy kaphatjuk meg. A kornyezet adja a Dependency Injection szolgaltatast.
        // A DbContext automatikusan megszunik a keres veges (DI beallitas).
        public ProductController(Dal.DataDrivenDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public List<Dtos.Product> List([FromQuery] string search = null, [FromQuery] int from = 0)
        {
            var filteredList = string.IsNullOrEmpty(search)
                ? _dbContext.Product // ha nincs nev alapu kereses, az osszes termek
                : _dbContext.Product.Where(p => p.Name.Contains(search)); // nev alapjan kereses

            return filteredList
                .Skip(from) // lapozashoz: hanyadik termektol kezdve
                .Take(5) // egy lapon max 5 termek
                .Select(p => new Dtos.Product(p.Id, p.Name, p.Price, p.Stock)) // adatbazis entitas -> DTO
                .ToList(); // a fenti IQueryable kiertekelesesen kieroltetese, kulonben hibara futnank
        }
    }
    ```

Az adatbázis kontextust DI-on keresztük konstruktor paraméterként kérhetjük el egyszerűen.

Vegyük észre, hogy a JSON sorosítással nem kellett foglalkoznunk. Az API csak DTO-t ad vissza, a sorosításról automatikusan gondoskodik a keretrendszer.

Lapozást azért érdemes beiktatni, hogy korlátozzuk a visszaadott választ (ahogy a felhasználói felületeken is szokás lapozni). Erre tipikus megoldás ez a "-tól" jellegű megoldás.

!!! note "Lapozás másképpen"
    Lapozást sok fajta módon tervezhetjük a REST API-k esetében. A fenti megoldás a legegyszerűbb, de elképzelhető olyan megközelítés is, hogy a kliens meghatározhassa a lapméretet és az abszolút `from` offset helyett a kért lap indexét adja meg a kérésben.

A metódus eredménye a `ToList`-et megelőzően egy `IQueryable<T>`. Emlékezzünk arra, hogy az `IQueryable<T>` nem tartalmazza az eredményt, az csak egy leíró.

!!! warning "`IQueryable<T> `visszatérési érték és `DbContext` életciklus"
    Ha nem lenne a végén `ToList`, akkor hibára futna az alkalmazás, mert amikor a JSON sorosítás elkezdené iterálni a gyűjteményt, már egy megszűnt adatbázis kapcsolaton próbálna dolgozni. A WebAPI végpontokból **soha ne** adjunk emiatt `IQueryable` visszatérési értéket!

    Az okok arra vezethetőek vissza, hogy alapértelmezetten a `DbContext` típusok `Scoped` életciklussal kerülnek beregisztrálásra a DI konténerbe, és ASP.NET Core esetében alapértelmezetten egy HTTP kérés során keletkezik egy scope. Viszont a sorosítás már kívül esne ezen a scope-on.

## 4. Feladat: Termékek adatainak szerkesztés API

Egészítsük ki a termékek kereséséhez született API-t az alábbi funkciókkal:

- Lehessen egy adott termék adatait lekérdezni a termék id-ja alapján a `/api/products/id` url-en.
- Tudjunk módosítani meglevő terméket (nevet, árat, raktárkészletet).
- Lehessen felvenni új terméket (ehhez készítsünk egy új DTO osztályt, amiben csak a név, raktárkészlet és ár van).
- Lehessen törölni egy terméket az id-ja alapján.

Mindegyik végpontot teszteljük!

??? tip "REST API tervezési konvenciók cheatsheet"

    REST API-k esetében minden URL (path része) egy-egy erőforrást reprezentál, amelyeken HTTP igékkel tudunk műveleteket végezni, a szerver pedig HTTP státuszkódok és DTO-k formájában válaszol.

    **Tipikus CRUD erőforrások, és műveleteik**

    | Ige        | URL                    | Sikeres válaszkód | Leírás                                      |
    |------------|------------------------|-------------------|---------------------------------------------|
    | GET        | /api/product           | 200 OK            | erőforrások listája                         |
    | GET        | /api/product?name=Test | 200 OK            | erőforrások listája (szűrt)                 |
    | POST       | /api/product           | 201 Created       | listába beszúrás                            |
    | GET        | /api/product/1         | 200 OK            | egy adott azonosítójú erőforrás lekérdezése |
    | PUT, PATCH | /api/product/1         | 200 OK            | egy adott azonosítójú erőforrás módosítása  |
    | DELETE     | /api/product/1         | 204 NoContent     | egy adott azonosítójú erőforrás törlése     |

    **Tipikus hibaági válaszkódok REST API-k esetében**

    | Ki hibázott | Válaszkód                 | Leírás                                              |
    |-------------|---------------------------|-----------------------------------------------------|
    | Kliens      | 400 Bad Request           | Kliens szemantikailag hibás adatokat küldött        |
    | Kliens      | 401 Unauthorized          | Bejelentkezés szükséges                             |
    | Kliens      | 403 Forbidden             | Van bejelentkezett user, de nincs joga a művelethez |
    | Kliens      | 404 Not Found             | Erőforrás nem található                             |
    | Szerver     | 500 Internal Server Error | Nem várt hiba történt                               |

### Lekérés ID szerint

A lekérés során gondoljuk arra is, ha a kérésben olyan ID érkezik, amely nem létezik az adatbázisban. Ilyenkor `404 Not Found` HTTP státuszkóddal térjünk vissza. Ehhez használjuk az `ActionResult<T>` visszatérési értéket, és a `ControllerBase`-ben lévő segédfüggvényeket.

??? example "Megoldás"

    ```csharp
    [HttpGet("{id}")]
    public ActionResult<Dtos.Product> Get(int id)
    {
        var dbProduct = _dbContext.Product.SingleOrDefault(p => p.Id == id);
        return dbProduct != null
            ? Ok(new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)) // siker eseten visszaadjuk az adatot magat
            : NotFound(); // 404 http valasz, ha nem talalhato a keresett elem
    }
    ```

!!! tip "ActionResult<T> alapértelmezett módon"
    A válaszkód testreszabása az `ActionResult<T>` osztály és segédfüggvényei segítségével egyértelmű. Viszont gondoljunk bele, hogy az előző feladatokban csak DTO-val tértünk vissza, ahol a keretrendszer a 200 OK alapértelmezéssel élt, így nem volt fontos explicit `ActionResult<T>`-vel visszatérni.

    Még egy egyszerűsítést ad a keretrendszer, mégpedig akkor is visszatérhetünk a natúr DTO-val, ha a controller action visszatérési értéke `ActionResult<T>` pl.:

    ```csharp hl_lines=2
    return dbProduct != null
        ? new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)
        : NotFound();
    ```

### Új termék beszúrása

- Készítsük el a szerver irányába érkező DTO osztályt rekordként, és a beszúró végpontot.
- A beszúrás tipikusan a listás erőforrás URL-jére küldött POST kérés
- Válaszként térjünk vissza a beszúrt adatokkal és a `Location` headerben a beszúrt erőforrás URL-jével. Ehhez a `CreatedAtAction` metódus lesz segítségünkre.

??? example "Megoldás"

    ```csharp
    namespace Bme.DataDriven.Rest.Dtos;

    public record NewProduct(string Name, double? Price, int? Stock);
    ```

    ```csharp
    [HttpPost]
    public ActionResult<Dtos.Product> Add([FromBody] Dtos.NewProduct newProduct)
    {
        var dbProduct = new Dal.Product()
        {
            Name = newProduct.Name,
            Price = newProduct.Price,
            Stock = newProduct.Stock,
            CategoryId = 1, // nem szep, ideiglenes megoldas
            VatId = 1 // nem szep, ideiglenes megoldas
        };

        // mentes az adatbazisba
        _dbContext.Product.Add(dbProduct);
        _dbContext.SaveChanges();

        // igy mondjuk meg, hol kerdezheto le a beszurt elem
        return CreatedAtAction(
            nameof(Get),
            new { id = dbProduct.Id },
            new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)); 
    }
    ```

Új termék beszúrásához Postman-ben az alábbi beállításokra lesz szükség:

- POST kérés a helyes URL-re
- A _Body_ fül alatt a `raw` és jobb oldalon a `JSON` kiválasztása
- Az alábbi _body_ json:

    ```json
    {
        "name": "BME-s kardigán",
        "price": 8900,
        "stock": 100
    }
    ```

A tesztelés során nézzük meg a kapott válasz _Header_-jeit is! A beszúrás esetén keressük meg benne a `Location` kulcsot. Itt adja vissza a rendszer, hol kérdezhető le az eredmény. Emellett általában a POST kérés a válaszban is vissza szokta adni a beszúrt adatokat.

### Termék módosítása

- A módosítást tipikusan a PUT ige reprezentálja.
- Nem létező erőforrás módosítása 404-es hibakódot eredményezzen.
- A módosítás megvalósítása során használjuk a meglévő `Product` DTO-t és validáljuk le, hogy azonos-e a path-ba és a body-ban kapott ID. Ehhez a `ModelState` tulajdonságot és a `BadRequest` függvényeket tudjuk használni.
- A módosítás szokásos módon EF-en keresztül zajlik.
- A módosító függvény is tipikusan vissza szokott térni a módosított adatokkal.

??? example "Megoldás"

    ```csharp
    [HttpPut("{id}")]
    public ActionResult<Dtos.Product> Modify([FromRoute]int id, [FromBody]Dtos.Product updated)
    {
        if (id != updated.Id)
        {
            ModelState.AddModelError(nameof(id), "Nem megfelelő a kapott ID");
            return BadRequest(ModelState);
        }

        var dbProduct = _dbContext.Product.SingleOrDefault(p => p.Id == id);
        if (dbProduct == null)
            return NotFound();

        // modositasok elvegzese
        dbProduct.Name = updated.Name;
        dbProduct.Price = updated.Price;
        dbProduct.Stock = updated.Stock;

        // mentes az adatbazisban
        _dbContext.SaveChanges();

        return new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock);
    }
    ```

A módosítás teszteléséhez az alábbi beállításokra lesz szükség:

- PUT kérés a helyes URL-re
- A _Body_ fül alatt a `raw` és jobb oldalon a `JSON` kiválasztása
- Az alábbi _body_ json:

    ```json
    {
        "id": 10,
        "name": "Egy óra csend",
        "price": 440,
        "stock": 10
    }
    ```

![Postman PUT kérés](images/postman-put-query.png)

Próbáljuk ki a kérést úgyis, hogy nem egyezik a path-ban és a body-ban lévő két ID. Ilyenkor 400-as Bad Requestet kell kapjunk a hiba részleteivel.

!!! note "DTO-k validációja"
    A DTO-kat egyéb validációknak is alávethetjük, amire használhatjuk az ASP.NET Core beépített [validációs attribútumait](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-7.0#validation-attributes) vagy akár egyéb külső osztálykönyvtárakat, mint a [FluentValidation](https://docs.fluentvalidation.net/en/latest/).

!!! note "PUT vs PATCH"
    A módosítás műveletre a PUT vagy a PATCH igéket szokás használni, amelyek között a fő különbség, hogy a PUT a teljes módosított erőforrást várja bemenetként, a PATCH viszont csak egy részleges adathalmazt (tipikusan kulcs érték párokat). .NET környezetben a PUT-ot egyszerűbb implementálni, de a PATCH-re is van [beépített támogatás](https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-7.0).

### Termék törlése

- A törléshez a DELETE HTTP igét használjuk, válaszként 204 No Content választ állítson elő sikeres ágon.
- Nem létező erőforrás itt is 404-et eredményezzen.

??? example "Megoldás"

    ```csharp
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var dbProduct = _dbContext.Product.SingleOrDefault(p => p.Id == id);
        if (dbProduct == null)
            return NotFound();

        _dbContext.Product.Remove(dbProduct);
        _dbContext.SaveChanges();

        return NoContent(); // a sikeres torlest 204 NoContent valasszal jelezzuk (lehetne meg 200 OK is, ha beletennenk an entitast)
    }
    ```

!!! note "Idempotens törlés művelet"
    Egy tipikus tervezői döntés szokott az lenni, hogy a törlés művelet legyen idempotens, tehát egymás után többször lefuttatva is azonos eredményt adjon. Ez a mi esetünkben nem lesz igaz, mert nem létező erőforrásra 404-et küldünk, míg létezőre 204-et. Ezt a műveletet úgy lehetne idempotenssé tenni, ha minden esetben 204-es státuszkóddal térnénk vissza, még akkor is, ha nem csináltunk semmit.

## 5. Feladat (opcionális): Új termék létrehozása: kategória és áfakulcs

Az új termék létrehozása során meg kellene adnunk még a kategóriát és az áfakulcsot is. Módosítsuk a fenti termék beszúrást úgy, hogy a kategória nevét és az áfakulcs számértékét is meg lehessen adni. A kapott adatok alapján keresd ki a megfelelő `VAT` és `Category` rekordokat az adatbázisból, vagy hozz létre újat, ha nem léteznek.

??? example "Megoldás"

    ```csharp title="NewProduct.cs" hl_lines="5-6"
    public record NewProduct(
        string Name,
        double? Price,
        int? Stock,
        int VatPercentage,
        string CategoryName);
    ```
    ```csharp title="ProductController.cs" hl_lines="4-10 17-18"
    [HttpPost]
    public ActionResult<Dtos.Product> Add([FromBody]Dtos.NewProduct newProduct)
    {
        var dbVat = _dbContext.Vat.SingleOrDefault(v => v.Percentage == newProduct.VatPercentage);
        if (dbVat == null)
            dbVat = new Dal.VAT() { Percentage = newProduct.VatPercentage };

        var dbCat = _dbContext.Category.SingleOrDefault(c => c.Name == newProduct.CategoryName);
        if (dbCat == null)
            dbCat = new Dal.Category() { Name = newProduct.CategoryName };

        var dbProduct = new Dal.Product()
        {
            Name = newProduct.Name,
            Price = newProduct.Price,
            Stock = newProduct.Stock,
            Category = dbCat,
            VAT = dbVat,
        };

        // mentes az adatbazisba
        _dbContext.Product.Add(dbProduct);
        _dbContext.SaveChanges();

        // igy mondjuk meg, hol kerdezheto le a beszurt elem
        return CreatedAtAction(
            nameof(Get),
            new { id = dbProduct.Id },
            new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)); 
    }
    ```

## Feladat 6 (opcionális): Aszinkron kontroller metódus

Az előbbi feladatot írjuk át [**aszinkronra**](../../jegyzet/async/index.md), azaz használjunk `async-await`-et. Az aszinkron végrehajtással a kiszolgáló hatékonyabban használja a rendelkezésre álló szálainkat amikor az adatbázis műveletekre várunk. Azért tudjuk ezt könnyedén megtenni, mert az Entity Framework alapból biztosít számunkra aszinkron végrehajtást, így a kontroller metódusunkban ezt fel tudjuk használni.

??? example "Megoldás"

    ```csharp hl_lines="2 4 8 23"
    [HttpPost]
    public async Task<ActionResult<Dtos.Product>> Add([FromBody]Dtos.NewProduct newProduct)
    {
        var dbVat = await _dbContext.Vat.SingleOrDefaultAsync(v => v.Percentage == newProduct.VatPercentage);
        if (dbVat == null)
            dbVat = new Dal.VAT() { Percentage = newProduct.VatPercentage };

        var dbCat = await _dbContext.Category.SingleOrDefaultAsync(c => c.Name == newProduct.CategoryName);
        if (dbCat == null)
            dbCat = new Dal.Category() { Name = newProduct.CategoryName };

        var dbProduct = new Dal.Product()
        {
            Name = newProduct.Name,
            Price = newProduct.Price,
            Stock = newProduct.Stock,
            Category = dbCat,
            VAT = dbVat,
        };

        // mentes az adatbazisba
        _dbContext.Product.Add(dbProduct);
        await _dbContext.SaveChangesAsync();

        // igy mondjuk meg, hol kerdezheto le a beszurt elem
        return CreatedAtAction(
            nameof(Get),
            new { id = dbProduct.Id },
            new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)); 
    }
    ```

Vegyük észre, mennyire egyszerű volt a dolgunk. Az Entity Framework által biztosított `...Async` metódusokat használjuk, mindegyiket `await`-elve, és a metódus szignatúráját kellett átírnunk `Task<T>` visszatérési értékűre (hogy kívülről bevárható legyen aszinkron) és ellátni `async` kulcsszúval (hogy `await`-et tudjunk benne használni). Minden másról továbbra is a keretrendszer gondoskodik.

!!! note "Aszinkronitás szerveralkalmazásokban"
    Az `async-await` .NET keretrendszer képesség, amelyet az ASP.NET Core és az Entity Framework is támogat. Számos más helyen is találkozhatunk azonban vele, például kliensalkalmazások esetében.

    Szerveralkalmazásoknál elsődleges célunk az áteresztőképesség növelése azáltal, hogy az aszinkron várakozás közben (esetünkben DB művelet), a kiszolgáló szál másik HTTP kéréssel is tudjon foglalkozni.
