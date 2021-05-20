# Feladat: REST API Web API technológiával

A házi feladat opcionális. A teljesítéssel **2 pluszpont és 2 iMsc pont** szerezhető.

GitHub Classroom segítségével hozz létre magadnak egy repository-t. A **meghívó URL-t Moodle-ben találod**. Klónozd le az így elkészült repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. A feladatok elkészítése után kommitold és pushold a megoldásod.

## Szükséges eszközök

- Windows, Linux vagy MacOS: Minden szükséges program platform független, vagy van platformfüggetlen alternatívája.
- [Postman](https://www.getpostman.com/)
- GitHub account és egy git kliens
- Microsoft Visual Studio 2019 [az itt található beállításokkal](../VisualStudio.md)
    - Linux és MacOS esetén Visual Studio Code és a .NET Core SDK-val települő [dotnet CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/) használható.
- [.NET Core **3.1** SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)

    !!! warning ".NET Core 3.1"
        A feladat megoldásához **3.1**-es .NET Core SDK telepítése szükséges.

        Windows-on Visual Studio verzió függvényében lehet, hogy telepítve van (lásd [itt](../VisualStudio.md#net-core-sdk-ellenorzese-es-telepitese) az ellenőrzés módját); ha nem, akkor a fenti linkről kell telepíteni (az SDK-t és _nem_ a runtime-ot.) Linux és MacOS esetén telepíteni szükséges.

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Termék műveletek (2 pluszpont)

A létrehozott és klónozott repository-ban megtalálható a kiinduló kód váz. Nyitsd meg Visual Studio-val és indítsd el. Egy konzol alkalmazásnak kell elindulnia, amely hosztolja a web alkalmazást. Próbáld ki (miközben fut a program): böngészőben nyitsd meg a <http://localhost:5000/api/product> oldalt, ahol a termékek listáját kell lásd JSON formában.

Nézd meg a rendelkezésre álló kódot.

- A `Startup.cs` inicializálja az alkalmazást. Ez egy ASP.NET Core webalkalmazás.
- Az alkalmazásban nincs adatbázis elérés az egyszerűség végett. A `ProductRepository` osztály teszteléshez használandó adatokat ad.
- A `ProductsController` _dependency injection_ segítségével példányosítja az `IProductRepository`-t.

Feladatok:

1. A `DAL.ProductRepository` osztályban a `Neptun` nevű mező értékében cseréld le a Neptun kódod. A string értéke a Neptun kódod 6 karaktere legyen.

    !!! warning "FONTOS"
        Az így módosított adatokról kell képernyőképet készíteni, így ez a lépés fontos.

1. Készíts egy olyan API végpontot, amivel ellenőrizhető, hogy létezik-e egy adott id-jú termék. A lekérdezéshez egy `HEAD` típusú HTTP kérést fogunk küldeni a `/api/product/{id}` URL-re. A válasz HTTP 200 vagy 404 legyen (extra tartalom/body nélkül, csak a válaszkód szükséges).

1. Készíts egy olyan API végpontot, ami egy terméket (`Product`) ad vissza az id-ja alapján; a kérés GET típusú legyen a `/api/product/{id}` címre, és a válasz vagy 200 legyen az adattal, vagy 404, ha nincs ilyen elem.

1. Készíts egy olyan API végpontot, amivel lekérdezhető, hány féle termék van összesen. (Például a lapozást elősegítendő kiszámolhatja a frontend, hogy hány lap lesz.) Ez is egy GET típusú kérés legyen a `/api/product/-/count` címre. A visszaadott adat a `CountResult` osztály példánya legyen kitöltve a darabszámmal (természetesen JSON formában).

    ??? question "Miért van a `/-` rész az URL-ben?"
        Ahhoz, hogy ezt megértsük, gondoljuk át, mi lehene az URL: termékek darabszámára vagyunk kíváncsiak, tehát `/api/product/`, de utána mi? Lehetne `/api/product/count`. Viszont ez "összekeveredik" a `/api/product/123` jellegű URL-lel, ami egy konkrét termék lekérdezésére szolgál. A gyakorlatban a két URL együtt tudna működni, mert a termék azonosító most szám, így a keretrendszer felismeri, hogy ha `/123` az URL vége, akkor a termék ID-t váró végpontot kell végrehajtani, ha pedig `/count` az URL vége, akkor a számosságot megadót. De ez csak akkor működik, ha az ID int. Ha szöveg lenne a termék azonosítója, probléma lenne. Ilyen esetekben olyan URL-t kell "kitalálni", ami nem ütközik. A `/-` rész azt jelzi, hogy ott _nem_ termék azonosító utazik.

        Megjegyzés: az URL - controller metódus azonosítás a fent leírtaknál bonyolultabb a valóságban. Az ASP.NET Core keretrendszer prioritás sorrendben illeszti a controller metódusokat a beérkező kérések URL-jeire. Ezt a prioritást lehetőségünk van befolyásolni a [`[Http*]` attribútumok `Order` tulajdonságával](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.httpgetattribute?view=aspnetcore-3.1#properties).

!!! example "BEADANDÓ"
    A módosított forráskódot töltsd fel.

    Emellett készíts egy képernyőképet Postman-ből (vagy más teszteléshez használt eszközből), amely egy sikeres termék lekérés eredményét mutatja. A képen legyen látható a kérés és a válasz minden részlete (kérés típusa, URL, válasz kódja, válasz tartalma). A válaszban a névben szerepelnie kell a **Neptun kódodnak**. A képet `f1.png` néven mentsd el és add be a megoldásod részeként!

## Feladat 2: OpenAPI dokumentáció (2 iMsc pont)

!!! note ""
    Az iMsc pont megszerzésére az első feladat megoldásával együtt van lehetőség.

Az OpenAPI (korábbi nevén Swagger) egy REST API dokumentációs eszköz. Célja hasonló a Web Service-ek esetében használt WSDL-hez: leírni az API szolgáltatásait egy standardizált formában. A korábbi feladatok megoldása után készíts [OpenAPI specifikációt és dokumentációt](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-3.1) a REST API leírásához.

1. A megoldáshoz kövesd a Microsoft hivatalos dokumentációját: <https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-3.1&tabs=visual-studio>

    - Mindenképpen a **Swashbuckle** opciót használd.
    - A `swagger.json`-t az alkalmazás maga generálja (nem kézzel kell megírnod), és a `/swagger/v1/swagger.json` címen legyen elérhető.
    - Állítsd be a _Swagger UI_-t is, ez a `/neptun` címen legyen elérhető. Ezt a `UseSwaggerUI` beállításánál a `RoutePrefix` konfigurálásával fogod tudni elérni. A saját Neptun kódod legyen a prefix **csupa kisbetűvel**.
    - (A "Customize and extend" résszel és egyéb testreszabással nem kell foglalkoznod.)

1. Indítsd el a webalkalmazást, és nézd meg a `swagger.json`-t <http://localhost:5000/swagger/v1/swagger.json> címen, és próbáld ki a SwaggerUI-t a <http://localhost:5000/neptun> címen.

1. Próbáld ki a SwaggerUI "Try it out" szolgáltatását: tényleg kiküldi a kérést a webalkalmazásnak, és látod a valódi választ.

    ![SwaggerUI Try it out](swaggerui-try.png)

!!! example "BEADANDÓ"
    A módosított forráskódot töltsd fel. Ügyelj rá, hogy a `csproj` fájl is módosult a hozzáadott NuGet csomaggal!

    Készíts egy képernyőképet a böngészőben megjelenő Swagger UI-ról. Ügyelj rá, hogy az URL-ben látható legyen, hogy a SwaggerUI-t a `/neptun` címen szolgálja ki a rendszer a saját Neptun kódoddal. A képet `f2.png` néven mentsd el és add be a megoldásod részeként!
