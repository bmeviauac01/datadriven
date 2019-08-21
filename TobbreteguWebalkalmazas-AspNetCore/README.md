# Többrétegű ASP.NET Core webalkalmazás
Teljes forráskód elérhető: <https://github.com/bmeviauac01/jegyzetek/tree/master/TobbreteguWebalkalmazas-AspNetCore/src>

Többrétegű alkalmazás web réteggel és REST API-val

* Szokásos adatbázis szükséges hozzá. Connection stringet a `Webshop.Web` / `appsettings.json`-ben kell átírni.
* Unit teszt futtatás: fordítás utan Tests / Test Explorer ablakban.
* Web alkalmazás: Visual Studio-ban `Webshop.Web` projektre jobb kattintás "Set as Startup Project" után **F5**, automatikusan indul a böngesző (http://localhost:2773/)
* REST API: Visual Studio-ból inditás, böngészőben vagy [Postman](https://www.getpostman.com/) segítségével (http://localhost:2773/api/vevo)

# Alkalmazás felépítése
Az alkalmazás a következő elemekből épül fel:

* `Webshop.Web`: ASP.NET Core webalkalmazás, az alkalmazás belépési pontja
  * Tartalmazza az MVC webalkalmazást
  * Kiszolgálja a REST api kéréseket is
* `Webshop.BL`: az összetettebb adatmódosítási feladatokat valósítja meg
* `Webshop.DAL`: az alkalmazás adatelérési rétege
  * Enity Framework Core-t használ az adateléréshez
  * Nem közvetlenül a `DbContext`-et teszi elérhetővé, megvalósítja a _Repository_ tervezési mintát
* `Webshop.Test`: a unit teszteket tartalmazó projekt

# Elkészítés
A következőkben az alkalmazás elkészítéséhez szükséges lépéseket mutatjuk be.

## ASP.NET Core MVC webalkalmazás
Első lépésként az MVC webalkalmazást készítjük el.

### 1. Webalkalmazás – keret
Hozzunk létre Visual Studioban egy "ASP.NET Core Web Application" projektet, `Webshop.Web` néven. A cél legyen .NET Core, ASP.NET Core 2.1. A sablonok közül válasszuk ki a "Web Application (Model-View-Controller)" sablont, a "Configure for HTTPS" opcióra nem lesz szükségünk.

Első lépésként írjuk át a portszámot a `launchSettings.json` fájlban **2773**-ra.

```json
"iisExpress": {
  "applicationUrl": "http://localhost:2773",
  "sslPort": 0
}
```
Ezután **F5** segítségével már el is tudjuk indítani a webalkalmazásunk keretét. Ahhoz, hogy lássunk saját adatokat, a következő feladatunk az adatelérési- és az üzleti logikai réteg elkezdése.

### 2. Adatelérési réteg – Entity Framework Core
Hozzunk létre Visual Studioban egy "Class Library (.NET Core)" projektet `Webshop.DAL` néven, és telepítsük a `Microsoft.EntityFrameworkCore.SqlServer` NuGet csomagot.

A Visual Studio Tools / NuGet Package Manager menüből válasszuk ki a Package Manager Console menüpontot, majd pedig a legördülő menüből válasszuk ki a `Webshop.DAL` projektet. A konzolban adjuk ki a kövezkező utasítást, ami az adatbázis alapján legenerálja nekünk a szükséges osztályokat az adateléréshez.

```powershell
Scaffold-DbContext "Server=localhost\SQLEXPRESS;Database=AAF-database;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir EF -Context WebshopDb -Tables Telephely,Vevo
```

Sajnos az Entity Framework által generált kód nem tökéletes, ki kell benne javítanunk néhány dolgot.

* A `Telephely` osztályban 
  * Nincs szükség a `VevoNavigation` propertyre

```csharp
public partial class Telephely
{
    public Telephely()
    {
    }

    public int Id { get; set; }
    public string Ir { get; set; }
    public string Varos { get; set; }
    public string Utca { get; set; }
    public string Tel { get; set; }
    public string Fax { get; set; }
    public int? VevoId { get; set; }

    public Vevo Vevo { get; set; }
}
```

* A `Vevo` osztályban
  * A `KozpontiTelephely`-et át kell nevezni `KoztpontiTelephelyId`-re
  * A `KozpontyTelephelyNavigation` neve pedig lehet `KozpontiTelephely`
  * A `Telephely` esetében használjunk többesszámot

```csharp
public partial class Vevo
{
    public Vevo()
    {
        Telephely = new HashSet<Telephely>();
    }

    public int Id { get; set; }
    public string Nev { get; set; }
    public string Szamlaszam { get; set; }
    public string Login { get; set; }
    public string Jelszo { get; set; }
    public string Email { get; set; }
    public int? KozpontiTelephelyId { get; set; }

    public Telephely KozpontiTelephely { get; set; }
    public ICollection<Telephely> Telephelyek { get; set; }
}
```

* A `WebshopDb` osztályban
  * Meg kell javítanunk a mappingeket
  * Ki kell törölni a constraintek nevét, hisz ez adatbázisrendszer függő adat
  * Töröljük innen a connection stringet, és átmásoljuk az `appsettings.json` fájlba

```csharp
modelBuilder.Entity<Telephely>(entity =>
{
    // ...

    entity.HasOne(d => d.Vevo)
        .WithMany(p => p.Telephelyek)
        .HasForeignKey(d => d.VevoId);
});
```

```csharp
modelBuilder.Entity<Vevo>(entity =>
{
    // ...

    entity.Property(d => d.KozpontiTelephelyId).HasColumnName("KozpontiTelephely");

    entity.HasOne(d => d.KozpontiTelephely)
        .WithOne()
        .HasForeignKey<Vevo>(d => d.KozpontiTelephelyId);
});
```

```json
"ConnectionStrings": {
  "WebshopDb": "Server=localhost\\SQLEXPRESS;Database=AAF-database;Trusted_Connection=True;"
}
```

* A `Webshop.Web` projektben a `Startup` osztályban felvesszük a dependenciák közé a `WebshopDb` adatbáziskontextust

```csharp
services.AddDbContext<WebshopDb>(options => options.UseSqlServer(Configuration.GetConnectionString("WebshopDb")));
```

### 3. Adatelérési réteg – Repository minta
Alkalmazásunkban nem szeretnénk, hogy az Entity Framework által generált osztályok kiszivárogjanak az alkalmazás további rétegeibe, így megvalósítjuk a Repository tervezési mintát. Ehhez először létrehozunk egy saját `Vevo` entitás osztályt, majd pedig magát a repositoryt.

```csharp
public class Vevo
{
    public readonly string Nev;
    public readonly string Email;

    public Vevo(string nev, string email)
    {
        this.Nev = nev;
        this.Email = email;
    }
}
```

```csharp
public interface IVevoRepository
{
    Task<IEnumerable<Vevo>> ListVevok();
}
```

```csharp
public class VevoRepository : IVevoRepository
{
    private readonly WebshopDb db;

    public VevoRepository(WebshopDb db)
    {
        this.db = db;
    }

    public async Task<IEnumerable<Vevo>> ListVevok()
    {
        return await db.Vevo
                       .GetVevok();
    }
}
```

```csharp
internal static class VevoExtensions
{
    public static async Task<IEnumerable<Vevo>> GetVevok(this IQueryable<EF.Vevo> vevok)
    {
        return await vevok.Select(dbVevo => dbVevo.GetVevo())
                          .ToArrayAsync();
    }

    public static Vevo GetVevo(this EF.Vevo dbVevo) => new Vevo(dbVevo.Nev, dbVevo.Email);
}
```

### 4. Üzleti logikai réteg
Jelenleg az üzleti logikai rétegünk nem tartalmaz valódi üzleti logikát, csupán közvetlenül továbbhív a repositoryba. Hozzunk létre Visual Studioban egy újabb "Class Library (.NET Core)" projektet `Webshop.BL` néven, majd adjunk hozzá egy új osztályt: `VevoManager`.

```csharp
public class VevoManager
{
    private readonly IVevoRepository vevoRepository;

    public VevoManager(IVevoRepository vevoRepository)
    {
        this.vevoRepository = vevoRepository;
    }

    public async Task<IEnumerable<Vevo>> ListVevok() => await vevoRepository.ListVevok();
}
```

### 5. Webalkalmazás
Az első lépésünk visszatérve a `Webshop.Web` projektbe, hogy a `Startup` osztályban felvesszük a megfelelő dependenciákat (lásd [dependency injection témakör](../Dependency-Injection/)).

```csharp
services.AddTransient<IVevoRepository, VevoRepository>();
services.AddTransient<VevoManager>();
```

Ezután a `HomeController` osztály tartalmát a következőre módosítjuk.

```csharp
public class HomeController : Controller
{
    private readonly VevoManager vevoManager;

    public HomeController(VevoManager vevoManager) =>  this.vevoManager = vevoManager;

    public async Task<IActionResult> Index() => View(await vevoManager.ListVevok());
}
```

Végül pedig a Views / Home mappában az `Index.cshtml` fájl tartalmát a következőre írjuk át.

```razor
@model IEnumerable<Webshop.DAL.Vevo>

@{
    ViewData["Title"] = "Home Page";
}

<h2>Vevők</h2>

<table class="table">
    <tr>
        <th>Név</th>
        <th>Email cím</th>
    </tr>

    @foreach (var vevo in Model)
    {
        <tr>
            <td>@vevo.Nev</td>
            <td>@vevo.Email</td>
        </tr>
    }
</table>
```

## REST API
Második lépésként a REST API-t fogjuk elkészíteni.

### 1. Adatelérési réteg
Jelenleg az adatelérési rétegünk képes a vevők listázására. A REST API-hoz szükséges funkcionalitás megvalósításához még szükségünk van a következő függvényekre: egy vevő lekérdezése és egy vevő törlése. Ehhez a következőképpen kell kiegészítenünk az eddig elkészült osztályokat.

```csharp
public interface IVevoRepository
{
    Task<IEnumerable<Vevo>> ListVevok();
    Task<Vevo> GetVevoOrNull(int vevoId);
    Task DeleteVevo(int vevoId);
}
```

```csharp
public class VevoRepository : IVevoRepository
{
    // ...

    public async Task<Vevo> GetVevoOrNull(int vevoId)
    {
        var dbVevo = await db.Vevo
                             .GetByIdOrNull(vevoId);

        return dbVevo?.GetVevo();
    }

    public async Task DeleteVevo(int vevoId)
    {
        var retries = 3;
        while (true)
        {
            var dbVevo = await db.Vevo
                                 .Telephellyel()
                                 .GetByIdOrNull(vevoId);

            if (dbVevo == null)
                return;

            db.Vevo.Remove(dbVevo);

            try
            {
                await db.SaveChangesAsync();
                return;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (--retries < 0)
                    throw;

                foreach (var e in ex.Entries)
                    await e.ReloadAsync();
            }
        }
    }
}
```

```csharp
internal static class VevoExtensions
{
    // ...

    public static async Task<EF.Vevo> GetByIdOrNull(this IQueryable<EF.Vevo> vevok, int vevoId)
        => await vevok.SingleOrDefaultAsync(dbVevo => dbVevo.Id == vevoId);

    public static IQueryable<EF.Vevo> Telephellyel(this IQueryable<EF.Vevo> vevok) 
        => vevok.Include(dbVevo => dbVevo.Telephelyek);
}
```

### 2. Üzleti logika
Ebben az esetbven az üzleti logikánk már kissé összetettebb lesz, ugyanis egy vevő törlésénél azt is figyelembe szertnénk venni, hogy olyan vevőt ne törölhessünk, akinek van megrendelése. Ezt természetesen a konkurrenciahibák elkerülése végett tranzakcióban végezzük. A következő függvények kerülnek megvalósításra a `VevoManager` osztályban.

```csharp
public class VevoManager
{
    private readonly IVevoRepository vevoRepository;
    private readonly IMegrendelesRepository megrendelesRepository;

    public VevoManager(IVevoRepository vevoRepository, IMegrendelesRepositorymegrendelesRepository)
    {
        this.vevoRepository = vevoRepository;
        this.megrendelesRepository = megrendelesRepository;
    }

    public async Task<IEnumerable<Vevo>> ListVevok() => await vevoRepository.ListVevok();

    public async Task<Vevo> GetVevoOrNull(int vevoId) => await vevoRepository.GetVevoOrNul(vevoId);

    public async Task<bool> TryDeleteVevo(int vevoId)
    {
        using (var tran = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead },
            TransactionScopeAsyncFlowOption.Enabled))
        {
            var vevo = await vevoRepository.GetVevoOrNull(vevoId);
            if (vevo == null)
                return false;

            var vanMegrendelese = (await megrendelesRepository.ListVevoMegrendelesei(vevoId)).Any();
            if (vanMegrendelese)
                return false;

            await vevoRepository.DeleteVevo(vevoId);

            tran.Complete();
            return true;
        }
    }
}
```

Ahhoz, hogy ez működjön, fel kellett vennünk egy repository interfészt és osztályt a megrendeléseknek is, azonban ennek a megvalósítása jelenleg csupán egy üres `IEnumerable<object>`-et adt vissza. Fontos, hogy ne felejtsük el felvenni a dependenciák közé ezt a repository-t is!

### 3. REST API
A REST API megvalósításához egy újabb `Controller` osztályt kell felvennünk a `Webshop.Web` projektünkbe. Ennek a neve legyen `VevoController`. A tartalma a következő.

```csharp
[Route("api/[controller]")]
[ApiController]
public class VevoController : ControllerBase
{
    private readonly VevoManager vevoManager;

    public VevoController(VevoManager vevoManager) => this.vevoManager = vevoManager;

    [HttpGet]
    public async Task<IEnumerable<Vevo>> Get() => await vevoManager.ListVevok();

    [HttpGet("{vevoId}")]
    public async Task<IActionResult> Get(int vevoId)
    {
        var vevo = await vevoManager.GetVevoOrNull(vevoId);
        if (vevo == null)
            return NotFound();
        else
            return Ok(vevo);
    }

    [HttpDelete("{vevoId}")]
    public async Task<IActionResult> Delete(int vevoId)
    {
        var vevo = await vevoManager.GetVevoOrNull(vevoId);
        if (vevo == null)
            return NotFound();
        else if (await vevoManager.TryDeleteVevo(vevoId))
            return Ok();
        else
            return Conflict();
    }
}
```

Ennek működését ki tudjuk próbálni a [Postman](https://www.getpostman.com/) szoftver segítségével.

## Tesztelés
Utolsó lépésként egy unit tesztet fogunk létrehozni az üzleti logikai rétegünkhöz. Ehhez Visual Studioban hozzunk létre egy "MSTest Test Project (.NET Core)" projektet `Webshop.Test` néven. Szükségünk lesz még a `Moq` NuGet csomagra.

A teszt forráskódja pedig a következő.

```csharp
[TestClass]
public class VevoManagerTest
{
    [TestMethod]
    public async Task TestTorolNemletezoVevo()
    {
        // Arrange
        var vevoRepository = new Mock<IVevoRepository>();
        vevoRepository
            .Setup(repo => repo.GetVevoOrNull(It.IsAny<int>()))
            .ReturnsAsync((Vevo)null);

        var megrendelesRepository = new Mock<IMegrendelesRepository>();
        megrendelesRepository
            .Setup(repo => repo.ListVevoMegrendelesei(It.IsAny<int>()))
            .ReturnsAsync(Enumerable.Empty<object>());

        // Act
        var vevoManager = new VevoManager(vevoRepository.Object, megrendelesRepository.Object);
        var result = await vevoManager.TryDeleteVevo(123);
        
        // Assert
        Assert.IsFalse(result);
    }
}
```
