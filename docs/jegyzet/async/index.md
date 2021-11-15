# Aszinkron kérések és DTO-k (példa WebAPI alkalmazás)

Az alábbi jegyzet a **kiszolgáló oldali aszinkronitást** és a **DTO-k (Data Transfer Object-ek) használatát** mutatja be egy példán keresztül. Az alkalmazás egy ASP.NET Core WebApi kiszolgáló Entity Framework adateléréssel, amely egy webshop kosár kezelését valósítja meg.

!!! quote "Szerző"
    Az alábbi jegyzet Zergi Máté munkája.

## Aszinkronitás

A webes alkalmazásaink legnagyobb része valamilyen módon kommunikál adatbázissal. Erről a kommunikációról **nem feltételezhetjük**, hogy:

* elérhető az adatbázis szerver,
* gyors a kapcsolat a kliens és a kiszolgáló között,
* az adatokat az adatbázis szerver gyorsan állítja elő.

Ezért fel kell készülni arra, hogy a kiszolgálás során kért adatokra várni kell. Erre kínál megoldást az **aszinkronitás**, amely hatékonyan használja ki a kiszolgáló erőforrásait - például nem foglal a web kiszolgálón erőforrást, amíg az az adatbázisra vár.

!!! warning "Aszinkronitás és párhuzamosság"
    Az aszinkronitás **nem azonos** a párhuzamossággal. Egy webes kiszolgáló a bejövő kéréseket párhuzamosan szolgálja ki. Az aszinkronitás egy kérés kiszolgálása során alkalmazott módszer, amely az I/O műveletek (pl. adatbázis elérés, fájl elérés, hálózati kommunikáció) során hatékonyan kezeli a kiszolgáló szálait.

## Az alkalmazás adatbázis modellje

Az alkalmazásunk a tárgy minta adatbázisához hasonló, de egyszerűbb adatbázissal dolgozik, az ER diagramja itt látható:

![Az alkalmazás adatbázis diagramja](images/dbdiagram.png)

!!! note ""
    Az egyszerűség kedvéért a kosarakban a _UserID_ nem idegen kulcsként szerepel egy _Users_ táblára, hanem egy statikus, 1-es ID-jű felhasználóval dolgozunk. A valóságban több felhasználó van, így a _UserID_ idegen kulcs lenne.

A _Products_ tábla reprezentál termékeket, a _Manufacturers_ tábla gyártókat (ez gyorsíthatja a gyártónkként szűrést), az _OrderItems_ pedig kosárban levő termékeket.

## Kiszolgáló alkalmazás felépítése

ASP.NET Core WebApi és Entity Framework segítségével szeretnénk a fenti adatbázis adatait REST-kompatibilis szolgáltatáson keresztül elérhetővé tenni. Az adatbázis modellezéséhez, valamint a WebAPI kiszolgáláshoz alapvetően három (esetleg négy) feladatot kell elvégezni:

1. Adatbázis táblát/táblákat modellező osztály/osztályok létrehozása,
1. Adatbázis kontextus létrehozása,
1. Data Transfer Object létrehozása, ha szükség van az adatok átalakítására a kliens számára,
1. WebAPI Kontroller létrehozása

Menjünk végig ezeken a lépéseken!

### Adatbázist modellező osztályok létrehozása

Az adatbázist modellező osztályokat ASP.NET Core platformon egy _Models_ mappába szokás szervezni, és azon belül az általuk reprezentált tábla nevével létrehozni őket.

A _Products_ táblát reprezentáló C# osztály:

```csharp
namespace WebshopApi.Models
{
    public class Product
    {
        public string Name { get; set; }
        public int ManufacturerID { get; set; }
        public int Price { get; set; }
        public int ID { get; set; }
    }
}
```

A _Manufacturers_ táblát reprezentáló C# osztály:

```csharp
namespace WebshopApi.Models
{
    public class Manufacturer
    {
        public string Name { get; set; }
        public int ID { get; set; }
    }
}
```

Az _OrderItems_ táblát reprezentáló C# osztály:

```csharp
namespace WebshopApi.Models
{
    public class OrderItem
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int CartID { get; set; }
        public int Pieces { get; set; }
    }
}
```

A _Carts_ táblát reprezentáló C# osztály:

```csharp
namespace WebshopApi.Models
{
    public class Cart
    {
        public int ID { get; set; }
        public int UserID { get; set; }
    }
}
```

Megfigyelhető, hogy ezek az osztályok csak adatok tárolására alkalmasak, amiket az egyes C# property-ken keresztül tudunk majd elérni.

### Adatbázis kontextus létrehozása

Miután létrehoztuk a táblákat modellező osztályokat, el tudjuk készíteni az adatbázis egészét modellező _DbContext_ osztályt. Ehhez a saját osztályunkat le kell származtatni az Entity Framework Core _DbContext_ osztályából.

```csharp
namespace WebshopApi.Models
{
    public class WebshopContext : DbContext
    {
        public WebshopContext(DbContextOptions<WebshopContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
```

Az adatbázisban szereplő egyes táblákat _DbSet_-ek definiálásával tudjuk elérni. A _DbSet_-eknek meg kell adni, hogy milyen entitásokat tárolnak (pl. a `DbSet<Products>` egy `Products` entitásokat tároló `DbSet`), valamint a nevüket.

A konstruktorban szereplő `DbContextOptions`-t a kontextus konfigurálására lehet használni. Erre egy példa, az alkalmazás `Startup` osztályában (lásd [REST API & ASP.NET Web API gyakorlat](../../gyakorlat/rest/index.md)) a következő konfiguráció:

```csharp
public class Startup
{
    // ...

    // Ezt a metódust a runtime hívja meg. Ezt a metódust használjuk servicek DI konténerhez való hozzáadásához
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<WebshopContext>(opt => 
                opt.UseSqlServer(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=Webshop;Integrated Security=True"));
        // ...
    }
}
```

### Data Transfer Object létrehozása

Az előző két lépés eredményeként rendelkezésünkre áll az adatbázis és tábláinak leképzése. Következő lépésként gondoljunk bele abba, hogyan néz ki egy webshopban a kosár: egy kosárban egyszerre több termék jelenik meg. Tehát míg az `OrderItem` osztályunk egy megrendelt terméket tud reprezentálni, a teljes kosarat ilyen elemek listája jelenti. Ez a termék lista egy ún. _Data Transfer Object_ használatával írható le: ez egy olyan osztály, ami a **kliens számára használható** adatot gyűjt össze az adatbázisból.

!!! abstract "Definíció: Data Transfer Object"
    Egy olyan objektum, ami adatot szállít futó alkalmazások (itt: kliens és szerver) között.

DTO-k használatával nem csak kényelmesebb lesz az adatok küldése, hanem gyorsítjuk is az alkalmazásunkat:

* Csak olyan adatot küldünk a kliensnek, amire annak szüksége van.
* Továbbá, a DTO-ba összegyűjthetjük az összetartozó adatokat és egyben küldjük el a kliensnek.

Gondoljuk végig, hogyan érdemes összegyűjteni az adatokat, ha a kliens a kosárban levő összes terméket, azok darabszámát, valamint az összes termék darabszámát szeretnénk megjeleníteni?

1. Az `OrderItem`-ben a kliens szempontjából felesleges adat a `CartID`, valamint az `ID`. Ezeket kiszűrve és a többi property-t meghagyva már más osztállyal dolgozunk, mint az `OrderItem`, viszont még mindig csak **egy** kosár cikket reprezentálunk. Az így keletkezett osztály neve legyen `CartItem`.

1. Vegyük észre, hogy a `CartItem`-ben tárolt `Product` termékreprezentációra ugyanez a gondolatmenet érvényes, azzal az eltéréssel, hogy a `Product` osztályt **bővíteni** kell a Manufacturer **nevével**, a `ManufacturerID` helyett (hiszen a felhasználói felületen ezt a nevet akarjuk megjeleníteni). Tehát hozzunk létre egy új `Product` osztályt is, és tároljuk el ezt a `CartItem`-ben!

1. Ilyen `CartItem` objektumokat **gyűjtsünk össze** egy listába, valamint számoljuk, hogy hány termék van összesen a kosárban. Legyen ez egy `UserCart` osztály. Ilyen `UserCart` példányokat küldünk majd a kliensnek.

A DTO-kat érdemes külön szervezni az adatbázis entitásoktól. Szervezzük őket egy _DTOs_ mappába! A fenti megoldással továbbmenve, nézzük meg, hogyan fognak kinézni ezek az osztályok!

`CartItem` osztály, ami kiszűri az `OrderItem`-ből a kliens számára felesleges adatokat, valamint egy kliens oldali, kosárban levő árucikket reprezentál:

```csharp
namespace WebshopApi.DTOs
{
    public class CartItem
    {
        public Product Product { get; set; } // Olyan product, amiben már nem ManufacturerID szerepel
        public int Amount { get; set; } // A rendelt mennyiség
    }
}
```

És a hozzá tartozó Product osztály:

```csharp
namespace WebshopApi.DTOs
{
    public class Product
    {
        public string ProductName { get; set; } // A termék neve, pl AB123 Full HD TV
        public string Manufacturer { get; set; } // A termék gyártójának !!neve!!, pl BMETV
        public int Price { get; set; } // A termék ára
        public int ID { get; set; } // A termék azonosítója
    }
}
```

!!! note "Miért szerepel itt az ID?"
    Jogosan merülhet fel a kérdés, hogy itt miért szerepel az `ID`. Végiggondolva, egy kosárelem áll egy termékből, valamint abból, hogy hány darab van az adott termékből. Megjelenítésnél ezért a kosár elemet a termék azonosítja, hiszen a termék azonosítója alapján tudjuk megjeleníteni a termékhez tartozó esetleges információkat. Ezt a problémát meg lehetne oldani úgy is, hogy a kosár elemnek van azonosítója, és a terméknek nincs, esetleg mindkettőnek lehet, ebben a példában a termék azonosítóját használjuk.

A `UserCart` osztály, ami összegyűjti az egyes rendelt cikkeket, és számolja, hogy hány terméket rendeltünk összesen:

```csharp
namespace WebshopApi.DTOs
{
    public class UserCart
    {
        public List <CartItem> CartPieces { get; set; }
        public int NumberOfItems { get; set; }
    }
}
```

!!! info ""
    Azzal, hogy a `CartItem`-eket listában tároljuk, könnyen kezelhetővé válnak, mind szerver, mind kliens oldalon, hiszen a kifele menő JSON objektumban a lista majd egy tömbként fog szerepelni, amin könnyen végig lehet iterálni.

Ezt a `UserCart` objektumot küldjük a kliens felé a kontrollerből, miután összegyűjtöttük a hozzá tartozó cikkeket és összeszámoltuk, hogy összesen hány termék van a kosárban.

### Controller osztály létrehozása

A kontroller osztályunkat érdemes egy _Controllers_ mappába szervezni, majd a [REST API & ASP.NET Web API gyakorlaton](../../gyakorlat/rest/index.md) alapján hozzunk létre egy Controller osztályt. Ez tartalmazni fog egy `WebshopContext`-et, valamint a HTTP kérések kiszolgálásához létrehozott végpontokat.

Itt szembesülünk először az aszinkronitással. Nézzük egy példán keresztül:

A GET kérés, ami lekérdezi az összes kosarat:

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Cart>>> GetCarts()
{
    var carts = await _context.Carts.ToListAsync();
    return carts;
}
```

Figyeljük meg a függvény deklarációjában az `async` kulcsszót és a `Task` típust, valamint a függvény törzsében a "párját", az `await`-et. Ezekre **async-await**-ként szoktunk hivatkozni. Értelmezzük tehát:

1. A függvény `Cart` példányok listáját, `IEnumerable<Cart>` ad vissza,
1. Amelyet a WebAPI kontrollernek megfelelően egy `ActionResult`-ba csomagolunk,
1. És az egészet még egy `Task`-ba is tesszük. Ez az aszinkronitás miatt van.

A fentiek mindegyike más miatt kell, de így, együtt adják a teljes megoldást. Nézzük ebből is az aszinkronitást, azaz a `Task` típust és az `await` kulcsszót. Ezzel a definícióval egy ún. "promise"-t adunk vissza (más nyelvekben szokták így hívni), amely egy jövőben elvégzendő feladat eredményét (fogja) tartalmazni.

Miért jó ez? Azért, mert a kontroller metódus elvégzése így lesz aszinkron. Amikor a rendszer egy `await` utasításhoz ér, a szál, ami eddig a feldolgozást végezte, abbahagyja ennek a kérésnek a kiszolgálását, és egy másik kérés végrehajtásával folytatja a munkát. Miért is? Mert az `await` "mögötti" feladatról tudjuk, hogy időigényes: az adatbázisra és a hálózatra várunk. Feleslegesen várakoztatnánk a kiszolgáló szálat, ha az itt "megállna" és bevárná az eredményt. Ehelyett a feladatot kiadjuk egy háttér rendszernek (az operációs rendszer és a .NET aszinkron I/O alrendszerének - ebbe azonban nem megyünk bele), és arról kérünk értesítést, amikor az itt várt végeredmény elkészült. Amint ez megtörténik, a korábban felfüggesztett kérés kiszolgálása folytatódik tovább.

Másként megfogalmazva a mi alkalmazásunk kiszolgáló száljai mindig aktívan munkát fognak végezni, nem várakoznak. A várakozás helyett más feladatok végrehajtására lesznek képesek. Ez összességében azt jelenti, hogy kevesebb operációs rendszer szálat veszünk igénybe és ezzel több kérést tudunk kiszolgálni. Ettől az alkalmazásunk hatékonyabb lesz.

Az előbb bemutatott függvényt szintaktikailag tovább egyszerűsíthetjük ha elhagyjuk a lokális változót és közvetlenül visszaadjuk a `Task` eredményt. Funkcionálisan az alábbi implementáció megegyezik a fentivel, azonban a magyarázatot a fenti részletesebb kiírás jobban szemlélteti.

```csharp
[HttpGet]
public Task<ActionResult<IEnumerable<Carts>>> GetCarts()
{
    return _context.Carts.ToListAsync(); // nincs await, és a deklarációban sincs async
}
```

!!! info "A ***Async függvények"
    A tárgyban eddig látott, lekérdezéseket kiértékelő függvényeknek (`ToList`, `First`, `All`, `Find`, stb...) mind van `...Async` párja, és hasonlóan kell őket használni a szinkron párjukhoz. Ezen függvények az alapjai az aszinkron működének.

    A működés részteleibe ennél tovább nem megyünk. Annyit jegyezzünk meg, hogy ahhoz, hogy a kontrollerünk aszinkron legyen, kell, hogy legyen "alatta" (itt: az Entity Framework-ben) támogatás az aszinkronitásra.

Nézzünk egy bonyolultabb példát: egy kosár tartalmának összegyűjtése, majd elküldése, a kosár rekord megkeresése `FindAsync` segítségével:

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<UserCart>> GetCart(int id)
{
    // aszinkron kérés az id által azonosított kosár megtalálására
    var cartRecord = await _context.Carts.FindAsync(id); 

    if (cartRecord == null)
        return NotFound();

    // lekérdezés felépítése
    var productsquery =
        from p1 in _context.Products
        join m1 in _context.Manufacturers on p1.ManufacturerID equals m1.ID
        select new Product(p1.ID, m1.Name, p1.Name, p1.Price); // felépítjuk a DTO Product-okat
    // aszinkron kiértékelés
    var products = await productsquery.ToListAsync().ConfigureAwait(false);

    // aszinkron kérés az order itemsekre
    var orderitemsquery = from oi in _context.OrderItems
                          where oi.CartID == cartRecord.ID
                          select oi;
    var orderitems = await orderitemsquery.ToListAsync().ConfigureAwait(false);

    // a továbbiakban szinkron a művelet, mert már memóriában van a két eredmény
    
    // Keressük meg azokat a termékeket, amik a kosárban vannak, 
    // joinoljuk az OrderItems rekordokkal, majd hozzunk létre a kettőből egy CartItem DTO objektumot
    var cartitems = products.Join(orderitems, p => p.ID, oi => oi.ProductID,
                                  (p, v) => new CartItem(p, v.Pieces)).ToList();

    // UserCart DTO előállítása
    return new UserCart()
    {
        CartPieces = cartitems,
        NumberOfItems = cartitems.Count()
    }
}
```

Figyeljük meg, hogy minden aszinkron műveletnél `await`-et kell használni! Viszont amikor már végeztünk azokkal a műveletekkel, amiknél az adatbázishoz kell fordulni, már használhatunk szinkron műveleteket!

!!! info "A `ConfigureAwait` metódus"
    A `ConfigureAwait(false)` további teljesítmény optimalizálásra ad lehetőséget. Ezzel a hívással azt jelezzük, hogy az `await`-elt eredmény megérkezése után _bármely_ szál folyathatja a további munkát, nem szükséges ugyanazon szálnak folytatnia a kiszolgálást, amely kezdte. Kiszolgáló oldali alkalmazások során ez általában a helyes viselkedés, de nem minden aszinkron világra igaz ez (például UI szálak használata esetén számít). Erről részletesebben lásd: <https://devblogs.microsoft.com/dotnet/configureawait-faq/>.

Végül nézzünk egy példát a `FirstOrDefaultAsync`-re, valamint egy kliens felől érkező POST kérésre, amiben a kliens által küldött három azonosító alapján megkeressük a megfelelő adatbázis rekordokat, és frissítjük őket (darabszám növelés/csökkentés):

```csharp
// Modell osztály a kérés argumentumainak kezelésére
namespace WebshopApi.Models
{
    public class PostCartItemArgs
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Amount { get; set; }
    }
}

//////////////////////////////////////////////////////////////////////////////

// Controller osztályban levő POST kérés
[HttpPost]
public async Task<IActionResult> PostCartItem([FromBody] PostCartItemArgs data)
{
    // kosár megkeresése az ID alapján
    var cart = await _context.Carts.FindAsync(data.CartId).ConfigureAwait(false);

    if (cart == null)
        return NotFound();

    // keressük meg azokat az order itemeket, amik ebbe a kosárba tartoznak, és ezt a terméket tárolják
    var orderitemquery = from oi in _context.OrderItems
                         where (oi.CartID == data.Id && oi.ProductID == data.ProductId)
                         select oi;
   
    // FirstOrDefault, hogy ha nem talál, az orderitem értéke null legyen
    var orderitem = await orderitemquery.FirstOrDefaultAsync().ConfigureAwait(false);

    if (orderitem == null)
    {
        // Ha eddig nem szerepelt a rendelt termékek között, új OrderItems létrehozása
        _context.OrderItems.Add(new OrderItems { CartID = data.Id, Amount = data.Amount, ProductID = data.ProductId });
    }
    else
    {
        // Ha már van ilyen termék a rendelésben
        orderitem.Amount += data.Amount;

        // Ha 0-ra csökkent a termékből rendelt darabszám, akkor töröljük a rekordot
        if (orderitem.Amount == 0)
            _context.OrderItems.Remove(orderitem);
    }

    await _context.SaveChangesAsync(); // ez is await-elt, hiszen megint az adatbázishoz fordulunk
    return NoContent();
}
```

!!! info "Ki várja meg a `Task` eredményét, ha az csak a jövőben fog elkészülni?"
    Minden `async` függvényt valahol `await`-elni kell. Jelen esetben a kontroller metódusunkat az ASP.NET Core keretrendszer fogja meghívni, és az fogja "megvárni" az eredményt a JSON sorosítás előtt.

## Teljes példakód

A teljes példakód (pár apró eltéréssel, valamint egy másik kontrollerrel) megtalálható a <https://github.com/mzergi/WebshopApi/> repository-ban.
