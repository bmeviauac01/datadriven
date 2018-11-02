# Függőséginjektálás ASP.NET Core környezetben

Remove this line

## Szerzői jogok
Benedek Zoltán, 2018

Jelen dokumentum a BME Villamosmérnöki és Informatikai Kar hallgatói számára készített elektronikus jegyzet. A dokumentumot az Adatvezérelt rendszerek c. tantárgyat felvevő hallgatók jogosultak használni, és saját céljukra egy példányban kinyomtatni. A dokumentum módosítása, bármely eljárással részben vagy egészben történő másolása tilos, illetve csak a szerző előzetes engedélyével történhet.
A
## Definíció

Függőséginjektálás (angolul __Dependency Injection__, röiden DI) egy tervezési minta. A fejlesztőket segíti abban, hogy az alkalmazás egyes részei laza csatolással kerüljenek kialakításra.
__A függőséginjektálás egy mechanizmus arra, hogy az osztály függőségi gráfjainak létrehozását függetlenítsük az osztály definíciójától.__

Célok
* könnyebb bővíthetőség
* unit tesztelhetőség

Természetesen a fenti defínícóból önmagában nem derül ki, pontosan milyen problémákat milyen módon old meg a DI. A következő fejezetekben egy példa segítségével helyezzük kontextusba a problémakört, illetve példa keretében megísmerkedünk az ASP.NET Core beépített DI szolgáltatának alapjaival.


## Példa, 1. fázis - szolgáltatás osztály beégetett függőségekkel

A példánkban egy teendőlista (TODO) kezelő alkalmazás e-mail értesítéseket küldő részeibe tekintünk bele, kódrészletek segítségével. Megjegyzés: a kód a tömörség érdekében minimalisztikus, funkcionálisan nem működik.

A példánk "belépési pontja" a `ToDoService` osztály `SendReminderIfNeeded` művelete. 

```csharp

// Teendők kezeléséle szolgáló osztály
public class ToDoService
{
    const string smtpAddress = "smtp.myserver.com";

    // Megvizsgálja a paraméterként kapott todoItem objektumot, és ha szükséges,
    // e-mail értesítést küld a teendőről a teendőben szereplő kontakt személynek.
    public void SendReminderIfNeeded(TodoItem todoItem)
    {
        if (checkIfTodoReminderIsToBeSent(todoItem))
        { 
            NotificationService notificationService = new NotificationService(smtpAddress);
            notificationService.SendEmailReminder(todoItem.LinkedContactId, todoItem.Name);
        }
    }

    bool checkIfTodoReminderIsToBeSent(TodoItem todoItem)
    {
        bool send = true; 
        /* ... */
        return send;
    }
    // ...
}

// Entitásosztály, egy végrehajtandó feladat adatait zárja egységbe
public class TodoItem
{
    // Adatbázis kulcs
    public long Id { get; set; }
    // Teendő neve/leírása
    public string Name { get; set; }
    // Jelzi, hogy a teendő elvégésre került-e
    public bool IsComplete { get; set; }
    // Egy teendőhöz lehetőség van kontakt személy hozzárendeléséhez:  ha -1, nincs
    // kontakt személy hozzárendelve, egyébként pedig a kontakt személy azonosítója.
    public int LinkedContactId { get; set; } = -1;
}
```

A fenti kódban (`ToDoService.SendReminderIfNeeded`) azt látjuk, hogy az e-mail küldés lényegi logikáját `NotificationService` osztályban kell keresnünk. Valóban, vizsgálódásunk központjába ez az osztály kerül. A következő kódrészlet ezen osztály kódját, valamint a függőségeit mutatja be:

```csharp
// Értesítések küldésére szolgáló osztály
class NotificationService 
{
    // Az osztály függőségei
    EMailSender _emailSender;
    Logger _logger;
    ContactRepository _contactRepository;

    public NotificationService(string smtpAddress)
    {
        _logger = new Logger();
        _emailSender = new EMailSender(_logger, smtpAddress);
        _contactRepository = new ContactRepository();
    }

    // E-mail értesítést küld az adott azonosítójú kontakt személynek (a contactId
    // egy kulcs a Contacts táblában)
    public void SendEmailReminder(int contactId, string todoMessage)
    {
        string emailTo = _contactRepository.GetContactEMailAddress(contactId);
        string emailSubject = "TODO reminder";
        string emailMessage = "Reminder about the following todo item: " + todoMessage;
        _emailSender.SendMail(emailTo, emailSubject, emailMessage);
    }
}

// Naplózást támogató osztály
public class Logger
{
    public void LogInformation(string text) { /* ...*/ }
    public void LogError(string text) { /* ...*/ }
}

// E-mail küldésre szolgáló osztály
public class EMailSender
{
    Logger _logger;
    string _smtpAddress;

    public EMailSender(Logger logger, string smtpAddress)
    {
        _logger = logger;
        _smtpAddress = smtpAddress;
    }
    public void SendMail(string to, string subject, string message)
    {
        _logger.LogInformation($"Sendding e-mail. To: {to} Subject: {subject} Body: {message}");

        // ...
    }
}

// Contact-ok perzisztens kezelésére szolgáló osztály
public class ContactRepository
{
    public string GetContactEMailAddress(int contactId)
    {
        // ...
    }
    // ...
}
```

Pár általános gondolat:

* A `NotificationService` osztály több függőséggel rendelkezik (`EMailSender`, `Logger`, `ContactRepository` osztályok), ezen osztályokra építve valósítja meg a szolgáltatásait. 
* A függőség osztályoknak lehetnek további függőségeik: az `EMailSender` remek példa erre, épít a `Logger` osztélyra. 
* Megjegyzés: a `NotificationService`, `EMailSender`, `Logger`, `ContactRepository` osztályokat **szolgáltatásosztályoknak** tekintjük, mert tényleges logikát is tartalmaznak, nem csak adatokat zárnak egységbe, mint pl. a `TodoItem`.
  
Tekintsük át a megoldás legfontosabb jellemzőit:

* Az osztály a függőségeit maga példányosítja
* Az osztály a függőségei konkrét típusától függ (nem interfészektől, "absztrakcióktól")

Ez a megközelítés több súlyos negatívummal bír:

1. **Rugalmatlanság, nehéz bővíthetőség**. A `NotificationService` (módosítás nélkül) nem tud más levélküldő, naplózó és contact repository implementációkkal együtt működni, csak a beégetett `EMailSender`, `Logger` és `ContactRepository` osztályokkal. Vagyis pl. nem tudjuk más naplózó komponenssel, vagy olyan contact repository-vek használni, amely más forrásból dolgozik.
2. **Unit tesztelhetőség hiánya**. A `NotificationService` (módosítás nélkül) **nem unit tesztelhető**. Ehhez ugyanis le kell cserélni az `EMailSender`, `Logger` és `ContactRepository` függőségeit olyanokra, melyek (tesztelést segítő) egyszerű/rögzített válaszokat viselkedést mutatnak. Ne feledjük: a unit tesztelés lényege, hogy egy osztály viselkedését önmagában teszteljük (pl. az adatbázist használó ContactRepository helyett egy olyan ContactRepository-ra van szükség, mely gyorsan, memóriából szolgálja ki a kéréseket, a teszt előfeltetételeinek megfelelően).
3. Kellemetlen, hogy a `NotificationService`-nek a függőségei paramétereit is át kell adni (smtpAddress, pedig ehhez a NotificationService-nek elvileg semmi köze, ez csak az `EMailSender`-re tartozik).

A következő lépésben úgy alakítjuk át a megoldásunkat, hogy a negatívumok többségétől meg tudjunk szabadulni.

## Példa, 2. fázis - szolgáltatás osztály manuális függőség injektálással

A korábbi megoldásunkat alakítjuk át, a funkcinális követelmények változatlanok. Az átalakítás legfontosabb irányelvei: a függőségeket "interfész alapokra" helyezzük, és az osztályok nem maguk példányosítják a függőségeiket.

```csharp
    public class ToDoService
    {
        const string smtpAddress = "smtp.myserver.com";

        // Megvizsgálja a paraméterként kapott todoItem objektumot, és ha szükséges,
        // e-mail értesítést küld a teendőről a teendőben szereplő kontakt személynek.
        public void SendReminderIfNeeded(TodoItem todoItem)
        {
            if (checkIfTodoReminderIsToBeSent(todoItem))
            {
                var logger = new Logger();
                var emailSender = new EMailSender(logger, smtpAddress);
                var contactRepository = new ContactRepository();

                NotificationService notificationService
                    = new NotificationService(logger, emailSender, contactRepository);
                notificationService.SendEmailReminder(todoItem.LinkedContactId,
                    todoItem.Name);
            }
        }

        bool checkIfTodoReminderIsToBeSent(TodoItem todoItem)
        {
            bool send = true;
            /* ... */
            return send;
        }
    }

// Értesítések küldésére szolgáló osztály
class NotificationService 
{
    // Az osztály függőségei
    IEMailSender _emailSender;
    ILogger _logger;
    IContactRepository _contactRepository;

    public NotificationService(ILogger logger, IEMailSender emailSender, 
        IContactRepository contactRepository)
    {
        _logger = logger;
        _emailSender = emailSender;
        _contactRepository = contactRepository;
    }

    // E-mail értesítést küld az adott azonosítójú kontakt személynek (a contactId
    // egy kulcs a Contacts táblában)
    public void SendEmailReminder(int contactId, string todoMessage)
    {
        string emailTo = _contactRepository.GetContactEMailAddress(contactId);
        string emailSubject = "TODO reminder";
        string emailMessage = "Reminder about the following todo item: " + todoMessage;
        _emailSender.SendMail(emailTo, emailSubject, emailMessage);
    }
}

#region Contracts (abstractions)

// Naplózást támogató interfész
public interface ILogger
{
    void LogInformation(string text);
    void LogError(string text);
}

// E-mail küldésre szolgáló interfész
public interface IEMailSender
{
    void SendMail(string to, string subject, string message);
}

// Contact-ok perzisztens kezelésére szolgáló interfész
public interface IContactRepository
{
    string GetContactEMailAddress(int contactId);
}

#endregion

#region Implementations

// Naplózást támogató osztály
public class Logger: ILogger
{
    public void LogInformation(string text) { /* ...*/  }
    public void LogError(string text) {  /* ...*/  }
}

// E-mail küldésre szolgáló osztály
public class EMailSender: IEMailSender
{
    ILogger _logger;
    string _smtpAddress;

    public EMailSender(ILogger logger, string smtpAddress)
    {
        _logger = logger;
        _smtpAddress = smtpAddress;
    }
    public void SendMail(string to, string subject, string message)
    {
        _logger.LogInformation($"Sendding e-mail. To: {to} Subject: {subject} Body: {message}");

        // ...
    }
}

// Contact-ok perzisztens kezelésére szolgáló osztály
public class ContactRepository: IContactRepository
{
    public string GetContactEMailAddress(int contactId)
    {
        // ...
    }
    // ...
}

#endregion
```

A korábbi megoldást a következő pontokban fejlesztettük tovább:

* A `NotificationService` osztály már nem maga példányosítja a függőségeit, hanem konstruktor  paraméterekben kapja meg.
* Interfészeket (absztrakciókat) vezettünk be a függőségek kezelésére
* A `NotificationService` osztály a függőségeit interfészek formájában kapja meg. Azt, amikor egy osztály a függőségeit kívülről kapja meg, **DEPENDENCY INJECTION**-nek (DI) vagyis függőséginjektálásnak nevezzük.
* Esetünkben konstruktor paraméterekben kapta meg az osztály függőségeit, ez **CONSTRUCTOR INJECTION**-nek (konktruktor injektálás) nevezzük. Ez a függőséginjektálás legyakoribb - és leginkább javasolt módja (alternatíva pl. a property injection, amikor gy publikus property setterének segítségével állítjuk be az osztály adott függőségét).

A megoldásunkban a `NotificationService` függőségeit az osztály (közvetlen) FELHASZNÁLÓJA példányosítja (`ToDoService` osztály). Bár a kódból nem látszik, az elveinknek megfelelően ez nem csak ezeknél az osztályoknál áll fent, hanem az alkalmazásban számos helyen. Elsődlegesen ebből eredően a következő problémák állnak még fent:
1. A `NotificationService` felhasználója, vagyis a `ToDoService.SendReminderIfNeeded` még mindig függ a konkrét típusoktól (hiszen neki szükséges példányosítania a `Logger`, `EMailSender` és `ContactRepository` osztályokat)
2. Ha több helyen használjuk a `Logger`, `EMailSender` és `ContactRepository` osztályokat, mindenhol külön-külön példányosítani kell őket. A célunk ezzel szemben az lenne, hogy EGYETLEN KÖZPONTI HELYEN határozzuk meg hogy milyen interfész típus esetén milyen implementációt kell MINDENOL használni az alkalmazásban (pl. ILogger->Logger, IMailSender->EMailSender).

## Példa, 3. fázis - függőségek injektálása .NET Core Dependency Injection alapokon

 Az előző fejezetben zárógondolatként megfogalmazott két probléma megoldására már némi extra segítságre van szükségünk: egy Inversion of Control (IoC) konténerre:

1. **REGISTER**: Az alkalmazás indulásakor egyszer, **KÖZPONTOSÍTVA** egy Inversion of Control (IoC) konténerbe beregisztráljuk a függőségi leképezéseket (pl. ILogger->Logger, IMailSender->EMailSender). Ez a DI folyamat **REGISTER** lépése.
    * Megjegyzés: ezzel megoldottuk az előző fejezeben felvezetett 2. problémát.
2. **RESOLVE** Amikor az alkalmazás futásakor szükségünk van egy implementációs objektumra, a konténertől az interfészt megadva kérünk egy implementációt (pl. ILoggert megadva egy Logger objektumot kapunk). Ez a DI folyamat  (függőségfeloldás) lépése.
    * A resolve lépést az alkalmazás "belépési pontjában" tesszük meg (pl. WebApi esetén az egyes API kérések beérkezésekor). A feloldást a konténertől csak a "**ROOT OBJECT**"-re (pl. WebApi esetén a megfelelő Controller osztályra) kérjük explicit módon: ez legyártja a root objectet, illetve annak valamennyi függőségét, és valamennyi közvetett függőségét: előáll egy objektumgráf.  Ez az **AUTOWIRING** folyamata.
    * Megjegyzés: Web API esetén a Resolve lépést a keretrendszer végzi el: mi csak annyit tapasztalunk, hogy a Controller osztályunk automatikusan példányosítódik, és valamennyi kontruktor paramétere automatikusan kitöltésre kerül (a REGISTER lépés regisztrációi alapján).

Szerencsére a .NET Core rendelkezik IoC Container alapú dependency injection szolgáltatással.

A következőkben a továbbfejlesztett e-mail értesítő megoldásunkat példaként használva világítjuk meg jobban a mechanizmust. 

### 1) REGISTER lépés (függőségek beregisztrálása)

Asp.Net Core környezetben a függőségek beregisztrálása a `Startup` osztályunk `ConfigureServices(IServiceCollection services)` műveletében történik, mégpedig a IServiceCollection **AddSingleton**, **AddTransient** és **AddScoped** műveleteivel. Első lépésben fókuszáljunk a `ConfigureServices` számunka legizgalmasabb részeire:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ...
        services.AddSingleton<ILogger, Logger>();
        services.AddTransient<INotificationService, NotificationService>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddSingleton<IEMailSender, EMailSender>( sp => new EMailSender(sp.GetRequiredService<ILogger>(), "smtp.myserver.com") );
        // ...
    }
```

A `Startup.ConfigureServices`-t a kererendszer hívja az alkamazás indulásakor. Paraméterben egy `IServiceCollection` services objektumot kapunk számunkra ez reprezentálja a kerenrendszer által már előre példányosított IoC konténert, ebbe tudjuk a sajét függőségeinket beregisztrálni. A

```csharp
services.AddSingleton<ILogger, Logger>(); 
```

sorral `ILogger` típusként a `Logger` implementációs típust regisztáljuk be (ILogger->Logger leképzés), mégpedig az **AddSingleton** művelet hatására **singleton**ként. Ez azt jelenti, hogy ha később a konténertől egy `ILogger` objektumot kérünk (resolve), a konténertől egy `Logger` objektumot kapunk, mégpedig mindig **ugyanazt**. A

```csharp
services.AddTransient<INotificationService, NotificationService>();
```

sorral `INotificationService` típusként a `NotificationService` implementációs típust regisztáljuk be (IContactRepository->ContactRepository leképzés), mégpedig az **AddTransient** művelet hatására **tranziens** módon. Ez azt jelenti, hogy ha később a konténertől egy `INotificationService` objektumot kérünk (resolve), a konténertől egy `NotificationService` objektumot kapunk, mégpedig minden lekérdezéskor egy **újonnan létrehozottat**. A

```csharp
services.AddScoped<IContactRepository, ContactRepository>();
```

sor `IContactRepository` típusként a `ContactRepository` implementációs típust regisztál be (IContactRepository->ContactRepository leképzés), mégpedig az **AddScoped** művelet hatására **scope-olt** módon. Ez azt jelenti, hogy ha később a konténertől `IContactRepository` objektumot kérünk (resolve),  `ContactRepository` objektumot kapunk, mégpedig **adott hatókörön belül ugyanazt**, eltérő hatókörökben másokat. A Web API alkalmazásoknál egy-egy API kérés kiszolgálása számít egy-egy megfelelő hatókörnek: vagyis a konténertől egy kérés kiszolgálása során ugyanazt az objektumpéldányt, eltérő kérések esetén másokat kapunk.

A `Startup.ConfigureServices`-ben további regisztrációkkal is találkozunk, ezekre később térünk vissza.

### 2) RESOLVE lépés (függőségek feloldása)

#### Alapok

Jelen pillanatban ott tartunk, hogy az alkalmazás indulásakor beregisztráltuk a szolgáltatás típusok függőségi leképezéseit az ASP.NET Core IoC konténerébe. Ezt követően, amikor szükségünk van egy adott implementációs típusra, a konténertől a regisztrációs típus alapján kérhetünk egy példányt. Ennek során a konténert egy `IServiceProvider` hivatkozás formájában kapjuk meg, és a `GetService` művelet különböző formáit használjuk. Pl.:
```csharp
void SimpleResolve(IServiceProvider sp)
{
    // Mivel az ILogger típushoz a Logger osztályt regisztráltuk,
    // egy Logger példánnyal tér vissza.
    var logger1 = sp.GetService(typeof(ILogger));

    // A típus generikus paraméterben is megadhatjuk, kényelmesebb, ezt szoktuk  használni.
    // Ehhez szükség van a Microsoft.Extensions.DependencyInjection névtér using-olására, 
    // mert ez a GetService forma ott definiált extension methodként.
    // Mivel az ILogger típushoz a Logger osztályt regisztráltuk,
    // egy Logger példánnyal tér vissza.
    var logger2 = sp.GetService<ILogger>();
    // Míg a GetService null-t ad vissza, ha nem sikerül feloldani a konténer alapján a hivatkozást, 
    // a GetRequiredService kivételt dob.
    var logger3 = sp.GetRequiredService<ILogger>();
    // ...
}
```

A példában kódkommentek részletesen elmagyarázzák a viselkedést. Minden esetben a lényeg az, hogy vagy a typeof operátorral, vagy generikus paraméterben megadunk egy típust, és a `GetService` egy az ahhoz beregisztrált implementációs típussal tér vissza.

#### Objektumgráf feloldása, autowiring

Az előző példánkban a konténer a feloldás során komolyabb "fejtörés" nélkül tudta a `Logger` osztályt példányosítani, ugyanis annak nincsenek további függőségei: egyetlen default konstruktorral rendelkezik.
Tekintsük most az `INotificationService` feloldását:

```csharp
public void ObjectGraphResolve(IServiceProvider sp)
{
    var notifService = sp.GetService<INotificationService>();
    // ...
}
```

A feloldás (GetService hívás) során a konténernek egy NotificationService objektumot kell létrehoznia. Ennek során feloldja az osztály közvetlen és közvetett függőségeit, rekurzívan:

* A NotificationService osztály egy háromparaméteres konstruktorral rendelkezik (vagyis három függősége is van):  `NotificationService(ILogger logger, IEMailSender emailSender,  IContactRepository contactRepository)`. A konstruktorparamétereket egyesével feloldásra kerülnek a regiszrációk alapján:
   * `ILogger` logger: egy `Logger` objektum lesz, mindig ugyanaz (mert singleton)
   * `IEMailSender` emailSender: `EMailSender` objektum lesz, minden alkalommal más (mert transient)
     * Ennek van egy `ILogger` logger konstruktor parammétere, amit fel kell oldani:
        * Egy `Logger` objektum lesz, mindig ugyanaz (mert singleton)
   * `IContactRepository` contactRepository: `ContactRepository` objektum lesz, hatókörönként - Web API estenén API hívásonként - más (mert scoped)
A feloldás végére - vagyis amikor visszatér a fenti `GetService<INotificationService>()` hívás - előáll a teljesen felparaméterezett `NotificationService` objektum, valamennyi közvetlen és közvetett függőségével: egy **objektumgráf**ot kapunk. 

A DI keretrendszer/IoC kontéren azon tulajdonságát, hogy az objektumok függőségeinek felderítésével a beregisztrált absztrakció->implementáció leképezések alapján képen objektumgráfokat előállítani **autowiring**-nek nevezzük.

#### IServiceProvider megszerzése

...


#### Függőségfeloldás ASP.NET Web API osztályok esetén

Az előző két fejezetben feltettük, hogy a `GetService` hívásához egy `IServiceProvider` objektum rendelkezésre áll. Ha mi magunk hozunk létre egy konténert, akkor ez így is van. Azonban csak a legritkább esetben szoktunk konténert létrehozni. Egy tipikus ASP.NET Web API alkalmazás esetén a konténert a keretrendszer hozza létre, és számunra közvetlenül nem is hozzáférhető. Ennek következtében `IServiceProvider`hez - pár induláskori konfigurációs és kiterjesztési pontot eltekintve - hozzáférést nem is kapunk. A jó hír az, hogy erre nincs is szükség. **A DI alapkoncepciójába ugyanis az is beletartozik, hogy a függőségfeloldást csak az alkalmazás belépési pontjában a "root object"-re (gyökérobjektum) végezzük el.**  Web API esetében a belépési pontot az egyes API kérések kiszolgálása jelenti. Amikor beérkezik egy kérés, akkor az Url és a rooting szabályok alapján a keretrendszer meghatározza, mely Controller/ControllerBase leszármazott osztályt kell példányosítani, és azt létre is hozza. Amennyiben a contoller osztálynak vannak függőségek (kontruktor paraméterek), azok is feloldásra kerülnek a beregisztrált leképezések alapján, beleértve a közvetett függőségeket is. Előálla teljes objektumgráf, a root object maga a contoller osztály. 

Nézzük ezt a gyakorlatban a korábbi példánk továbbfejleszésével, melyet egy TodoController osztállyal egészítettünk ki:

```csharp
[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
    // A TodoController osztály függőségei
    private readonly TodoContext _context; // ez egy DbContext
    private readonly INotificationService _notificationService;
    
    // A függőségeket konstruktor paraméterben kapja meg.
    public TodoController(TodoContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;

        // Fill wit some initial data
        if (_context.TodoItems.Count() == 0)
        {
            _context.TodoItems.Add(new TodoItem { Name = "Item1" });
            _context.TodoItems.Add(new TodoItem { Name = "Item2", LinkedContactId = 2});
            _context.SaveChanges();
        }
    }

    // API kezelőfüggvény e-mail emlékeztető értesítés kiküldésére.
    // Példa: http post erre a címre (pl. PostMan-nel): http://localhost:58922/api/todo/2/reminder
    // Ez a 2-es azonosítójú todo item kontakt személyének értesítést küld a todo itemről.
    [HttpPost("{id}/reminder")]
    public IActionResult ReminderMessageToLinkedContact(long id)
    {
        // Todo item kikeresése, használja a _context DbContext objektumot
        var item = _context.TodoItems.Find(id);
        if (item == null)
            return NotFound();

        // Emlékeztető e-mail kiküldése
        _notificationService.SendEmailReminder(item.LinkedContactId, item.Name);

        // Valójában nem hozunk létre semmit, egyszerű OK a válasz
        return Ok();
    }

    // ... további műveletek
}
```

A http://<gépcím>/api/todo url alá beeső kéréseket a routing szabályok alapján a `TodoController` osztály kapja meg. Az értesítés kiküldését triggerelő http://<gépcím>/api/todo/<todo id>/reminder címre érkező post kérést pedig a `TodoController.ReminderMessageToLinkedContact` művelete. A TodoController-t a keretrendszer példányosítja, minden kéréshez új objektumot hoz létre. Az osztálynak két függősége van, melyeket konstruktor paraméterben kap meg. Az első egy `TodoContext` objektum, ami egy DbContext leszármazott. A másik a már jól ismert INotificationService. A DI keretrendszerez ezeket is példányosítja a regisztrált leképezések alapján (az összes közvetett függőségeikkel), paraméterként átagja TodoController kontruktornak, ahol ezeket tagváltozókban eltároljuk. Így ezek a beérkező kéréseket kiszolgáló műveletekben, mint pl. a  `ReminderMessageToLinkedContact`-ben  már rendelkezésre állnak.

## DbContext regisztráció és feloldás

Gyakori, hogy vagy a controller, vagy a repository osztályok esetén szükség van DbContext objektumra (Entity Framework alapú adathozzáférés). Ez esetben célszerű megközelítés lehet, ha a DbContext-re mint egy osztályok között megosztott unit of work-re gondolunk. Ennek az a lényege, hogy a bejövő kérés során egy DbContext objektumot hozunk létre, hogy ezt megosztott módon beinjektáljuk az erre építő osztályoknak.  Ennek megvalósítására pedig remek kézre eső beépített DI alapú megoldást nyújt az ASP.NET Core.




## Haladó(bb) témakörök

### Függőségek regisztrálása, további példák

Térjünk ki a Startup.ConfigureServices korrábban nem ismertetett részeire.

Az `EMailSender` beregisztrálása első ránézésre egészen trükkösnek tűnik:

```csharp
 services.AddSingleton<IEMailSender, EMailSender>( sp => new EMailSender(sp.GetRequiredService<ILogger>(), "smtp.myserver.com") );
```

A jobb megértés érdekében nézzük meg az EMailSender konstruktorát:

 ```csharp
 public EMailSender(ILogger logger, string smtpAddress)
{
    _logger = logger;
    _smtpAddress = smtpAddress;
}
```

Az EMailSendert a konténernek kell majd a feloldás során példányosítania, ehhez a konstrukor paramétereket megfelelően meg kell tudni adnia. A logger paraméter tejlesen "rendben van", a kontéter ILogger->Logger regisztrációja alapján a konténer meg tudja tenni. Az smtpAddress paraméter értékét viszont nem tudja kitalálni. Az ASP.NET Core a probléma megoldására a keretrendszer "Options" mechanizmusát javasolja, mely leetővé teszi, hogy az értéket valamilyen konfigurációból olvassuk be. Ez számunkra egy messzire vezető szál lenne, így egyszerűsítésképpen más megoldáshoz folyamodtunk. Az AddSingleton (és a többi Add műveletnek) van olyan overloadja, melyben egy lambda kifejezést tudunk megadni. Ezt a lambdát a konténer a későbbiekben a resolve során (vagyis amikor egy IEMailSender alapján egy EMailSendert kérünk a konténertől) hívja, minden egyes példányosítás során: ebben mi magunkpéldányosítjuk az EMailSender objektumot, a konstruktor paramétereket az igényeink szerint meghatározva. Sőt, a konténer "van olyan kedves", hogy lambda paraméterben kapunk egy `IServiceCollection` objektumot (példánkban ez az `sp`), és ezzel a konténerben már meglévő regisztrációk alapján a GetRequiredService és GetService hívásokkal már tudunk típusokat feloldani. 





Megemlítendő:
* Vannak kivételek, nem mindig történik a regisztráció, feloldás és injektálás interfész alapokon: vagyis van, amikor az osztályt annak konkrét típusával regisztráljuk és oldjuk fel. Példa: MyClass-ként regisztrálás és MyClass-ként feloldás, ahol a MyClass osztály. Mely esetekben fordul elő:
    * Ilyen pl. a megfelelő Controller leszármazott osztály példányosítása Web API kérések beérkezésekor. Ez nem DI alapokon, hanem az ASP.NET Core routing szabályainak megfelelően történik. Ugyanakkor a Controller leszármazott osztály valamennyi függőségének feloldása már "szabványos" DI IoC Container segítségével történik.
    * A DbContext leszármazott osztályunk számára soha nem vezetünk be interfészt, hanem az osztályának a típusával kerül beregisztrálásra az IoC konténerbe (vagyis pl. TodoContext->TodoContext leképezés történik). A DbContext önmagában is számos perzisztencial providerrel (pl. MSSQL, Oracle, memóra, stb.) tud együtt működni, így alkalmazásfüggő, mennyire van értelme absztrahálni. Ha absztraháljuk az adathozzáférést, akkor nem a DbContext-hez vezetünk be interfészt, hanem a Repository tervezési mintát használjuk, és az egyes repository implementációkhoz vezetünk be interfészeket, valamint ezek vonatkozásában tölténik az IoC konténerben a leképezés (pl. ITodoRepository->TodoRepository). A repository osztályok pedig vagy maguk példányosítják a DbContext objektumokat, vagy konkruktor paraméterben kerül számukra beinjektálásra).
    Az illusztráció kedvéért a TodoApi alkalmazásunk ebben az értelemben vegyes megoldást alkalmaz: a Todo objektumok perzisztálásása a szolgáltatás osztályok közvetlen a DbContext-et használják, míg a Contact-ok kezelésére repository mintát használ.

## További témakörök

* Megfelelő konstruktor kiválasztása
* ServiceLocator antipattern
* ServiceProvider elérése
  * Startup.Configure, IApplicationBuilder paraméter ApplicationServices tulajdonságában. De itt nem vagyunk még kérés kiszolgálásában, a scope-olt szolgáltatások valószínűleg nem működnek.
  * A Web API kérések kiszolgálása során: HttpContext.RequestServices statikus tulajdonságban.
* Általánosságába egy alkalmazásban számos konténer lehet (pl. egy adott modul/komponens a kontextusában felépíthet egy konténert, amit a saját specifikus környezete számára elérhetővé tesz, pl. UI Site).
* Nem csak  ASP.NET Web API, hanem ASP.NET MVC esetén is
* TodoContext context beregisztrálása

## Kérdések

* Legyen-e a ContactRepository-nak DbContext paramétere