# Feladat: REST API Web API technológiával

A házi feladat teljesítésével **4 pont és 3 iMsc pont** szerezhető.

GitHub Classroom segítségével hozz létre magadnak egy repository-t. A **meghívó URL-t Moodle-ben találod**. Klónozd le az így elkészült repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. A feladatok elkészítése után kommitold és pushold a megoldásod.

A megoldáshoz szükséges szoftvereket és eszközöket lásd [itt](../index.md#szukseges-eszkozok).

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Egyszerű lekérdezés és OpenAPI dokumentáció (2 pont)

A létrehozott és klónozott repository-ban megtalálható a kiinduló kód váz. Nyisd meg Visual Studio-val és indítsd el. Egy konzol alkalmazásnak kell elindulnia, amely hosztolja a web alkalmazást. Próbáld ki (miközben fut a program): böngészőben nyisd meg a <http://localhost:5000/api/product> oldalt, ahol a termékek listáját kell lásd JSON formában.

Nézd meg a rendelkezésre álló kódot.

- A `Startup.cs` inicializálja az alkalmazást. Ez egy ASP.NET Core webalkalmazás.
- Az alkalmazásban nincs adatbázis elérés az egyszerűség végett. A `ProductRepository` osztály teszteléshez használandó adatokat ad.
- A `ProductsController` _dependency injection_ segítségével példányosítja az `IProductRepository`-t.

Feladatok:

### Egyszerű lekérdezés

1. A `DAL.ProductRepository` osztályban a `Neptun` nevű mező értékében cseréld le a Neptun kódod. A string értéke a Neptun kódod 6 karaktere legyen.

    !!! warning "FONTOS"
        Az így módosított adatokról kell képernyőképet készíteni, így ez a lépés fontos.

1. Készíts egy olyan API végpontot, amivel ellenőrizhető, hogy létezik-e egy adott id-jú termék. A lekérdezéshez egy `HEAD` típusú HTTP kérést fogunk küldeni a `/api/product/{id}` URL-re. A válasz HTTP 200 vagy 404 legyen (extra tartalom/body nélkül, csak a válaszkód szükséges).

### OpenAPI dokumentáció

Az OpenAPI (korábbi nevén Swagger) egy REST API dokumentációs eszköz. Célja hasonló a Web Service-ek esetében használt WSDL-hez: leírni az API szolgáltatásait egy standardizált formában. A korábbi feladatok megoldása után készíts [OpenAPI specifikációt és dokumentációt](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger) a REST API leírásához.

1. A megoldáshoz kövesd a Microsoft hivatalos dokumentációját: <https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle>

    - Mindenképpen a **Swashbuckle** opciót használd.
    - A `swagger.json`-t az alkalmazás maga generálja (nem kézzel kell megírnod), és a `/swagger/v1/swagger.json` címen legyen elérhető.
    - Állítsd be a _Swagger UI_-t is, ez a `/neptun` címen legyen elérhető. Ezt a `UseSwaggerUI` beállításánál a `RoutePrefix` konfigurálásával fogod tudni elérni. A saját Neptun kódod legyen a prefix **csupa kisbetűvel**.
    - (A "Customize and extend" résszel és egyéb testreszabással nem kell foglalkoznod.)

1. Indítsd el a webalkalmazást, és nézd meg a `swagger.json`-t <http://localhost:5000/swagger/v1/swagger.json> címen, és próbáld ki a SwaggerUI-t a <http://localhost:5000/neptun> címen.

1. Próbáld ki a SwaggerUI "Try it out" szolgáltatását: tényleg kiküldi a kérést a webalkalmazásnak, és látod a valódi választ.

    ![SwaggerUI Try it out](swaggerui-try.png)
    
1. Készítd el azt az API végpontot, ami vissza is adja a kívánt terméket (`Product`) az id-ja alapján; a kérés `GET` típusú legyen a `/api/product/{id}` címre, és a válasz vagy 200 legyen az adattal, vagy 404, ha nincs ilyen elem. Ellenőrizd a SwaggerUI segítségével.

!!! example "BEADANDÓ"
    A módosított forráskódot töltsd fel. Ügyelj rá, hogy a `csproj` fájl is módosult a hozzáadott NuGet csomaggal!

    Készíts egy képernyőképet a böngészőben megjelenő Swagger UI-ról. Ügyelj rá, hogy az URL-ben látható legyen, hogy a SwaggerUI-t a `/neptun` címen szolgálja ki a rendszer a saját Neptun kódoddal. A képet `f1.png` néven mentsd el és add be a megoldásod részeként!

## Feladat 2: Termék műveletek (2 pont)

A termékekkel kapcsolatos leggyakoribb adatbázisműveletek az új beszúrása, meglévő termék lekérdezése, módosítása vagy törlése, vagyis a CRUD (create, read, update és delete) műveletek. Ezekhez dedikált végpontokat készítünk, amiken keresztül a műveletek végrehajtását el tudja végezni az API használója. Ebben a feladatban a leggyakoribb végpontokat kell implementálni a már meglévő lekérdezés mellé.

1. Készíts egy olyan API végpontot, ami beszúr egy új terméket (`Product`) az id-ja alapján; a kérés `POST` típusú legyen a `/api/product` címre, a kérés törzsében várja az új `Product` értéket, és a válasz vagy 201 legyen, vagy 409, ha már van ilyen elem.

1. Készíts egy olyan API végpontot, ami módosít egy terméket (`Product`) az id-ja alapján; a kérés `PUT` típusú legyen a `/api/product/{id}` címre, a kérés törzsében várja a változtatott `Product` értéket, és a válasz vagy 204 legyen tartalom nélkül, vagy 404, ha nincs ilyen elem.

1. Készíts egy olyan API végpontot, ami töröl egy terméket (`Product`) az id-ja alapján; a kérés `DELETE` típusú legyen a `/api/product/{id}` címre, és a válasz vagy 204 legyen tartalom nélkül, vagy 404, ha nincs ilyen elem.

1. Készíts egy olyan API végpontot, amivel lekérdezhető, hány féle termék van összesen. (Például a lapozást elősegítendő kiszámolhatja a frontend, hogy hány lap lesz.) Ez is egy `GET` típusú kérés legyen a `/api/product/-/count` címre. A visszaadott adat a `CountResult` osztály példánya legyen kitöltve a darabszámmal (természetesen JSON formában).

    ??? question "Miért van a `/-` rész az URL-ben?"
        Ahhoz, hogy ezt megértsük, gondoljuk át, mi lehetne az URL: termékek darabszámára vagyunk kíváncsiak, tehát `/api/product/`, de utána mi? Lehetne `/api/product/count`. Viszont ez "összekeveredik" a `/api/product/123` jellegű URL-lel, ami egy konkrét termék lekérdezésére szolgál. A gyakorlatban a két URL együtt tudna működni, mert a termék azonosító most szám, így a keretrendszer felismeri, hogy ha `/123` az URL vége, akkor a termék ID-t váró végpontot kell végrehajtani, ha pedig `/count` az URL vége, akkor a számosságot megadót. De ez csak akkor működik, ha az ID int. Ha szöveg lenne a termék azonosítója, probléma lenne. Ilyen esetekben olyan URL-t kell "kitalálni", ami nem ütközik. A `/-` rész azt jelzi, hogy ott _nem_ termék azonosító utazik.

        Megjegyzés: az URL - controller metódus azonosítás a fent leírtaknál bonyolultabb a valóságban. Az ASP.NET Core keretrendszer prioritás sorrendben illeszti a controller metódusokat a beérkező kérések URL-jeire. Ezt a prioritást lehetőségünk van befolyásolni a [`[Http*]` attribútumok `Order` tulajdonságával](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.httpgetattribute).

!!! example "BEADANDÓ"
    A módosított forráskódot töltsd fel.

    Emellett készíts egy képernyőképet Postman-ből (vagy más teszteléshez használt eszközből), amely egy sikeres termék lekérés eredményét mutatja. A képen legyen látható a kérés és a válasz minden részlete (kérés típusa, URL, válasz kódja, válasz tartalma). A válaszban a névben szerepelnie kell a **Neptun kódodnak**. A képet `f2.png` néven mentsd el és add be a megoldásod részeként!



## Feladat 3: Termék részleges frissítése (3 iMsc pont)

!!! note ""
    Az iMsc pont megszerzésére az első két feladat megoldásával együtt van lehetőség.

A termékek módosítása esetén az eddig használt `PUT` hívásnak számos hátránya van. A `PUT` a teljes erőforrás frissítésére lett kitalálva, azaz egy termék módosításához a teljes terméket el kell küldeni. Ez egyrészt kevéssé hatékony (például hálózaton le kell kérni és átküldeni minden meglévő tulajdonságát, akármilyen nagyok is azok), illetve ezek feldolgozása is plusz feladatokat jelenthet. A `PATCH` ige arra lett kitalálva, hogy részleges frissítési lehetőséget biztosítson, azaz elég legyen elküldeni azokat a tulajdonságokat amiket módosítani szeretnénk.

Ebben a feladatban létre kell hoznod egy végpontot, ami biztosítja a termékek részleges frissítését:

1. A kérés `PATCH` típusú legyen a `/api/product/{id}` címre, és a válasz vagy 204 legyen, ha sikerül, vagy 404, ha nincs ilyen elem.

1. A `ProductController` osztályban valósítsd meg a végpontot, ami elvégzi a részleges frissítést.
A végpont által kapott paraméter típusa `JsonPatchDocument` erősen típusos változata legyen.
Tesztelés során figyelj rá, hogy csak a küldött értékek változzanak meg (például, ha nincs felküldött objektumban raktárkészlet, az ne változzon).

!!! tip "JsonPatchDocument"
    A `JsonPatchDocument` az ASP.NET Core által nyújtott osztály és tartozik hozzá beépített mechanizmus is.

!!! example "BEADANDÓ"
    A módosított forráskódot töltsd fel.

    Emellett készíts egy képernyőképet Postman-ből (vagy más teszteléshez használt eszközből), amely egy sikeres részleges módosítás eredményét mutatja. A képen legyen látható a kérés és a válasz minden részlete (kérés típusa, URL, válasz kódja, válasz tartalma). A válaszban a névben szerepelnie kell a **Neptun kódodnak**. A képet `f3.png` néven mentsd el és add be a megoldásod részeként!
