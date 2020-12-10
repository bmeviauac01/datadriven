# Aszinkron kérések és Data Transfer Object

Az aszinkronitást egy Webshop kosár kezelésének példáján keresztül fogjuk bemutatni, hiszen egy webshopnál valószínűleg sok felhasználó van, sok kosárral, és ezért az egyes kosarak összeállítása, megkeresése az adatbázisban időigényes folyamat lesz. 

!!! info "Aszinkronitás"
	Ma már a webes alkalmazásaink legnagyobb része valamilyen módon kommunikál adatbázissal. Erről a kommunikációról **nem** feltételezhetjük, hogy: 
	**elérhető a szerver**, 
	**gyors a kapcsolat a kliens és a host között**,
	**az adatokat a szerver gyorsan állítja elő.**
	Ezért fel kell készülni arra, hogy a kért adatokra várni kell. Erre kínál megoldást az aszinkronitás.

### Az alkalmazás adatbázis modellje

Az alkalmazásunk a tárgy minta adatbázisához hasonló, de egyszerűbb adatbázissal dolgozik, az ER diagramja itt látható:
![Az alkalmazás ER diagramja](images/dbdiagram.png)

!!! note ""
	Az egyszerűség kedvéért a kosarakban a UserID nem idegen kulcsként szerepel egy Users táblára, hanem egy statikus, 1-es ID-jű userrel dolgozunk. A valóságban több felhasználó van, így a UserID idegen kulcs lenne.

A Products tábla reprezentál termékeket, a Manufacturers tábla gyártókat (ez gyorsíthatja a gyártónként szűrést), az OrderItems pedig kosárban levő termékeket.

### Adatbázis modellezése Entity Framework segítségével

Entity Frameworkben adatbázisok modellezéséhez, valamint azok eléréséhez alapvetően három (esetleg négy) feladatot kell elvégezni:

* Adatbázis táblát/táblákat modellező osztály/osztályok létrehozása,
* Adatbázis kontextus létrehozása,
* (Opcionális, de ebben a jegyzetben foglalkozunk vele) Data Transfer Object létrehozása, ha szükség van az adatok átalakítására a kliens számára,
* Kontroller létrehozása

Menjünk végig ezeken a lépéseken!

### Adatbázist modellező osztályok létrehozása

Az adatbázist modellező osztályokat érdemes egy Models mappába szervezni, és azon belül az általuk reprezentált tábla nevével létrehozni őket.

A Products táblát reprezentáló C# osztály:
```csharp
namespace WebshopApi.Models
{
    public class Products
    {
        public string Name { get; set; }
        public int ManufacturerID { get; set; }
        public int Price { get; set; }
        public int ID { get; set; }
    }
}
```
A Manufacturers táblát reprezentáló C# osztály:
```csharp
namespace WebshopApi.Models
{
    public class Manufacturers
    {
        public string Name { get; set; }
        public int ID { get; set; }
    }
}
```
Az OrderItems táblát reprezentáló C# osztály:
```csharp
namespace WebshopApi.Models
{
    public class OrderItems
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int CartID { get; set; }
        public int Pieces { get; set; }
    }
}
```
A Carts táblát reprezentáló C# osztály:
```csharp
namespace WebshopApi.Models
{
    public class Carts
    {
        public int ID { get; set; }
        public int UserID { get; set; }
    }
}
```

Megfigyelhető, hogy ezek az osztályok csak adatok tárolására alkalmasak, amiket az egyes propertyken keresztül tudunk majd elérni.

### Adatbázis kontextus létrehozása

Miután létrehoztuk a táblákat modellező osztályokat, el tudjuk készíteni az adatbázis egészét modellező DbContext osztályt. Ehhez a saját osztályunkat le kell származtatni az EntityFrameworkCore DbContext osztályából. Nézzük meg, hogyan!

```csharp
namespace WebshopApi.Models
{
    //Webshop adatbázist reprezentáló osztály
    public class WebshopContext : DbContext
    {
        public WebshopContext(DbContextOptions<WebshopContext> options) //opciók, például: connection string
            : base(options)
        {
        }

        public DbSet<Products> Products { get; set; } //Products tábla
        public DbSet<Manufacturers> Manufacturers { get; set; } //Manufacturers tábla
        public DbSet<Carts> Carts { get; set; } //Carts tábla
        public DbSet<OrderItems> OrderItems { get; set; } //OrderItems tábla
    }
}
```

Az adatbázisban szereplő egyes táblákat DbSet-ek létrehozásával tudjuk majd elérni. A DbSet-eknek meg kell adni, hogy milyen entitásokat fognak majd tárolni (``` DbSet<Products> ``` egy ```Products``` entitásokat tároló ```DbSet```), valamint a nevüket.


A konstruktorban szereplő DbContextOptions-t a kontextus manuális konfigurálására lehet használni. Erre egy példa, az alkalmazás Startup osztályában (lásd REST API & ASP.NET Web API gyakorlat) a következő konfiguráció:

```csharp
public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Ezt a metódust a runtime hívja meg. Ezt a metódust használjuk servicek containerhez hozzáadásához.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<WebshopContext>(opt => 
                     opt.UseSqlServer(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=Webshop;Integrated Security=True"));
            //.
            //.
            //.
        }
        //.
        //.
        //.
    }
```

!!! note ""
    Itt az AddDbContext opcióiba az adatbázis connection stringjét másoltam be, hogy az alkalmazásunk tényleg elérje az adatbázist. Ez Visual Studioban az adatbázishoz csatlakozás után így érhető el:
    Jobb klikk az adatbázison -> Properties -> General fül alatt Connection string

!!! info "Kezdeti tesztelés"
    Alkalmazások kezdeti teszteléséhez használható az InMemory database, viszont ha InMemory adatbázist használunk, és a saját gépünkön tesztelünk, az kvázi szinkron kiszolgáláshoz fog vezetni. Vigyázzunk, hogy később nagy valószínűséggel át kell majd térnünk valamilyen perzisztens, nem memória alapú technológiára.

### Data Transfer Object létrehozása

Az előző két lépés eredményeként már reprezentálva van az adatbázis, valamint az adatbázis táblák is. Jobban átgondolva egy webshop alkalmazás kosarát, egy kosárban egyszerre több termék jelenik meg, az OrderItems osztályunk viszont egyszerre csak egy megrendelt terméket tud reprezentálni. Ezt a korlátozást tudjuk feloldani Data Transfer Objectek használatával, amik a kliens számára használható adatot alkotnak az adatbázis számára hasznos adatokból.

!!! info "Definíció: Data Transfer Object"
    Egy olyan objektum, ami adatot szállít futó alkalmazások között.

DTO-k használatával nem csak kényelmesebb lesz az adatok küldése, hanem gyorsítjuk is az alkalmazásunkat. Ezt azzal érjük el, hogy a DTO-ba összegyűjthetjük az összetartozó adatokat, amiket DTO nélkül több üzenetben kéne elküldeni a címzetthez.

!!! question "DTO osztályok: hogyan?"
    Gondoljuk végig, hogyan érdemes összegyűjteni az adatokat, ha a kliens a kosárban levő összes terméket, azok darabszámát, valamint az összes termék darabszámát szeretnénk kirajzolni!

??? example "Egy lehetséges megoldás"
    Az **OrderItems**-ben a kliens szempontjából felesleges adat a **CartID**, valamint az **ID**. Ezeket kiszűrve és a többi propertyt meghagyva már más osztállyal dolgozunk, mint az OrderItems, viszont még mindig csak **egy** kosár cikket reprezentálunk. Az így keletkezett osztály neve legyen ```CartItem```. (Vegyük észre, hogy a ```CartItem```-ben tárolt Products termékreprezentációra ugyanez a gondolatmenet lefuttatható, azzal az eltéréssel, hogy a Products osztályt **bővíteni** kell a Manufacturer **nevével**, a ManufacturerID helyett. Ezután hozzunk létre hasonló módon egy ```Product``` osztályt, és tároljuk el ezt a ```CartItem```-ben!) Ilyen ```CartItem``` objektumokat **gyűjtsünk össze** egy listába, valamint számoljuk, hogy hány termék van összesen a kosárban. Legyen ez egy ```UserCart``` osztály. Ilyen ```UserCart``` példányokat küldünk majd a kliensnek.

A DTO-kat érdemes külön szervezni az adatbázis modellezésétől. Szervezzük őket egy DTOs mappába! A fenti megoldással továbbmenve, nézzük meg, hogyan fognak kinézni ezek az osztályok!

CartItem osztály, ami kiszűri az OrderItemsből a kliens számára felesleges adatokat, valamint egy kliens oldali, kosárban levő árucikket reprezentál:

```csharp
namespace WebshopApi.DTOs
{
    public class CartItem
    {
        public Product product { get; set; } //Olyan product, amiben már nem ManufacturerID szerepel
        public int pieces { get; set; } //A rendelt mennyiség
    }
}
```

És a hozzá tartozó Product osztály:

```csharp
namespace WebshopApi.DTOs
{
    public class Product
    {
        public string ProductName { get; set; } //A termék neve, pl AB123 Full HD TV
        public string Manufacturer { get; set; } //A termék gyártójának !!neve!!, pl BMETV
        public int Price { get; set; } //A termék ára
        public int ID { get; set; } //A termék azonosítója
    }
}
```

??? note "Miért szerepel itt az ID?"
    Jogosan merülhet fel a kérdés, hogy itt miért szerepel az ID. Végiggondolva, egy kosárelem áll egy termékből, valamint abból, hogy hány darab van az adott termékből. Megjelenítésnél ezért a kosár elemet a termék azonosítja, hiszen a termék azonosítója alapján tudjuk megjeleníteni a termékhez tartozó esetleges információkat. Ezt a problémát meg lehetne oldani úgy is, hogy a kosár elemnek van azonosítója, és a terméknek nincs, esetleg mindkettőnek lehet, ebben a példában a termék azonosítóját használjuk.

A UserCart osztály, ami összegyűjti az egyes rendelt cikkeket, és számolja, hogy hány terméket rendeltünk összesen:

```csharp
namespace WebshopApi.DTOs
{
    public class UserCart
    {
        public List <CartItem> cartpieces { get; set; }
        public int numberofitems { get; set; }
    }
}
```

!!! info ""
    Azzal, hogy a CartItem-eket listában tároljuk, könnyen kezelhetővé válnak, mind szerver, mind kliens oldalon, hiszen a kifele menő JSON objektumban a lista majd egy tömbként fog szerepelni, amin könnyen végig lehet iterálni.

Ezt a UserCart objektumot küldjük majd a szerver felé a kontrollerből, miután összegyűjtöttük a hozzá tartozó cikkeket és összeszámoltuk, hogy összesen hány termék van a kosárban. 


Nézzük meg, hogyan!

### Controller osztály létrehozása

A kontroller osztályunkat érdemes kiszervezni egy Controllers mappába, majd a REST API & ASP.NET Web API gyakorlaton látottak alapján hozzunk létre egy Controller osztályt.

Ez tartalmazni fog egy ```WebshopContext```-et, valamint a gyakorlaton látott módhoz hasonlóan létrehozott végpontokat.

A különbségek az aszinkronitásból fognak adódni, nézzük meg, hogyan!

Egy HttpGet, ami lekérdezi az összes kosarat:

```csharp
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Carts>>> GetCarts()
        {
            var carts = await _context.Carts.ToListAsync();

            return carts;
        }
```

Lehet látni a függvény előtti async kulcsszót, valamint a függvényben a párját, az await-et. Ezekre **async-await**-ként szoktunk hivatkozni. 

Mivel a ToListAsync egy kollekciót fog visszaadni, ezért szükség van az IEnumerable-re. Később látni fogjuk a FirstAsync-nál, hogy ez nem mindig van így.

!!! Warning "Fontos!"
    Megfigyelhető, hogy a gyakorlathoz képest eltérő típussal tér vissza a függvény. Ez az aszinkronitás miatt van. Ezzel a kéréssel egy _feladatot (Task)_ adunk az alkalmazásnak, amire majd ha elkészült azzal, válaszol. Az aszinkron műveletek elé await kerül, hogy megvárjuk az eredményt, **de nem blokkolja a fő szálat(!!)**. 
    
A Tasktól kezdve pedig a gyakorlaton látotthoz hasonló szintaxissal várunk egy ActionResult-ot, amiben IEnumerable lesz, amelyben pedig Carts példányok lesznek.

!!! info "Az eddig látott függvények Async párjai"
    A tárgyban eddig látott, lekérdezéseket kiértékelő függvényeknek (ToList, First, All, Find, stb...) mind van Async párja, és hasonlóan kell őket használni a szinkron párjukhoz.

Példa az első kosár lekérdezésére és a FirstAsync használatára:

```csharp
        //api/carts/firstcart
        [HttpGet ("firstcart")]
        public async Task<ActionResult<Carts>> GetFirstCart()
        {
            var carts = await _context.Carts.FirstAsync();

            return carts;
        }
```

Egy bonyolultabb példa, egy kosár tartalmának összegyűjtése, majd elküldése, a kosár rekord megkeresése FindAsync segítségével:

??? Example "Nézzük a GET kérést!"

    ```csharp
    // GET: api/carts/1
    [HttpGet("{id}")]
        public async Task<ActionResult<UserCart>> GetCart(int id)
        {
            var cartRecord = await _context.Carts.FindAsync(id); //aszinkron kérés az id által 
                                                                 //azonosított kosár megtalálására

            if (cartRecord == null)
            {
                return NotFound();
            }

            //query építés, majd aszinkron kiértékelés példa
            var productsquery =
                from p1 in _context.Products
                join m1 in _context.Manufacturers
                on p1.ManufacturerID equals m1.ID
                select new Product(p1.ID, m1.Name,p1.Name,p1.Price); //felépítjuk a DTO Productokat

            var products = await productsquery.ToListAsync().ConfigureAwait(false); //aszinkron kérés a termékekre

            var orderitemsquery = from oi in _context.OrderItems
                             where oi.CartID == cartRecord.ID
                             select oi;

            var orderitems = await orderitemsquery.ToListAsync().ConfigureAwait(false); //aszinkron kérés az order itemsekre


            //itt már lehet szinkron művelet, hiszen memóriában van a két eredmény
            
            //Keressük meg azokat a termékeket, amik a kosárban vannak, 
            //joinoljuk az OrderItems rekordokkal, majd hozzunk létre a kettőből egy CartItem DTO objektumot
            var cartitems = products.Join(orderitems, p => p.ID, oi => oi.ProductID, (p, v) => new CartItem(p, v.Pieces)).ToList();


            //UserCart feltöltése
            UserCart usercart = new UserCart();
            usercart.cartpieces = cartitems;
            usercart.numberofitems = 0;
            foreach (var c in usercart.cartpieces)
            {
                usercart.numberofitems += c.pieces;   
            }

            return usercart;
        }
    ```

    Figyeljük meg, hogy minden aszinkron műveletnél **await**-et kell használni! Viszont amikor már végeztünk azokkal a műveletekkel, amiknél az adatbázishoz kell fordulni, már használhatunk szinkron műveleteket!

??? info "A ConfigureAwait metódus"
    A ConfigureAwait(false) használatával gyorsíthatjuk alkalmazásunkat, valamint elkerülhetjük az esetleges deadlockra futásokat.

    <https://devblogs.microsoft.com/dotnet/configureawait-faq/>

Végül pedig példa a FirstOrDefaultAsync-re, valamint egy kliens felől érkező POST kérésre, amiben a kliens által küldött három azonosító alapján azonosítjuk a megfelelő adatbázis rekordokat, és frissítjük őket (darabszám növelés/csökkentés):


??? example "Nézzük a POST kérést!"
    ```csharp
    //Modell osztály a kérés argumentumainak kezelésére
    namespace WebshopApi.Models
    {
        public class PostCartItemArgs
        {
            public int cartid { get; set; }
            public int productid { get; set; }
            public int pieces { get; set; }
        }
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    //Controller osztályban levő POST kérés

        [HttpPost]
        public async Task<IActionResult> PostCartItem(PostCartItemArgs data)
        {
            int id = data.cartid;
            int productid = data.productid;
            int pieces = data.pieces;

            var cart = await _context.Carts.FindAsync(id).ConfigureAwait(false); //kosár megkeresése az ID alapján

            if (cart == null)
            {
                return NotFound();
            }

            //keressük meg azokat az order itemeket, amik ebbe a kosárba tartoznak, és ezt a terméket tárolják
            var orderitemquery = from oi in _context.OrderItems
                            where (oi.CartID == id && oi.ProductID == productid)
                            select oi;

            var orderitem = await orderitemquery.FirstOrDefaultAsync().ConfigureAwait(false);

            //FirstOrDefault, hogy ha nem talál, az orderitem értéke null legyen

            //Ha eddig nem szerepelt a rendelt termékek között
            if (orderitem == null)
            {
                //propertyk használata új OrderItems létrehozásakor, majd mentés az adatbázisba
                _context.OrderItems.Add(new OrderItems { CartID = id, Pieces = pieces, ProductID = productid });
                _context.SaveChanges();
            }

            //Ha már van ilyen termék a rendelésben
            else
            {
                orderitem.Pieces+=pieces;
                if(orderitem.Pieces == 0)
                {
                    //ha 0-ra csökkent a termékből rendelt darabszám, akkor töröljük a rekordot
                    _context.OrderItems.Remove(orderitem);
                }
                _context.SaveChanges();
            }

            return NoContent();
        }
    ```


### Példakód

A teljes példakód (pár apró eltéréssel, valamint egy másik kontrollerrel) megtalálható a <https://github.com/mzergi/WebshopApi/> repóban.