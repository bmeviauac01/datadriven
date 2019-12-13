# Opcionális házi feladat 5: REST API Web API technológiával

A házi feladat opcionális. A teljesítéssel **2 pluszpont és 2 iMsc pont** szerezhető.

GitHub Classroom segítségével a <https://classroom.github.com/a/akiVKQ0u> linken keresztül hozz létre egy repository-t. Klónozd le a repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. A feladatok elkészítése után kommitold és pushold a megoldásod.

## Szükséges eszközök

- Microsoft Visual Studio 2017/2019 [az itt található beállításokkal](VisualStudio-install.md)
- [Postman](https://www.getpostman.com/)

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: HEAD kérés és URL "túlterhelés" (2 pont)

A létrehozott és klónozott repository-ban megtalálható a kiinduló kód váz. Nyitsd meg Visual Studio-val és indítsd el. Egy konzol alkalmazásnak kell elindulnia, amely hosztolja a web alkalmazást. Próbáld ki (miközben fut a program): böngészőben nyitsd meg a <http://localhost:5000/api/product> oldalt, ahol a termékek listáját kell lásd JSON formában.

Nézd meg a rendelkezésre álló kódot.

- A `Startup.cs` inicializálja az alkalmazást. Ez egy ASP.NET Core webalkalmazás.
- Az alkalmazásban nincs adatbázis elérés az egyszerűség végett. A `TermekRepository` osztály teszteléshez használandó adatokat ad.
- A `TermekController` _dependency injection_ segítségével példányosítja az `ITermekRepository`-t.

Feladatok:

1. Készíts egy olyan API végpontot, amivel ellenőrizhető, hogy létezik-e egy adott id-jú termék. A lekérdezéshez egy `HEAD` típusú HTTP kérést fogunk küldeni a `/api/product/{id}` URL-re. A válasz HTTP 200 vagy 404 legyen (extra tartalom/body nélkül, csak a válaszkód szükséges).

1. Készíts egy olyan API végpontot, ami egy terméket ad vissza az id-ja alapján; a kérés GET típusú legyen a `/api/product/{id}` címre, és a válasz vagy 200 legyen az adattal, vagy 404, ha nincs ilyen elem.

1. Készíts egy olyan API végpontot, amivel lekérdezhető, hány féle termék van összesen. (Például a lapozást elősegítendő kiszámolhatja a frontend, hogy hány lap lesz.) Ez is egy GET típusú kérés legyen a `/api/product/count` címre. A visszaadott adat a `CountResult` osztály példánya legyen kitöltve a darabszámmal (természetesen JSON formában).

   Vedd észre, hogy ez az URL, és az előbbi (az ID alapú termék lekérdezés) URL-je nagyon hasonlít. A feladat úgy elkészíteni ez utóbbi kérést, hogy az ne rontsa el az előbbit. Ehhez kihasználjuk, hogy az attribútum alapú route megadásnál lehetőségünk van az [illeszkedés sorrendjét definiálni](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-2.1#ordering-attribute-routes). Azt szeretnénk tehát elérni, hogy a `api/product/count` URL-re a darabszámot visszaadó API végpont illeszkedjen, és ne az ID alapú lekérdezés. Használd ehhez az [Order](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.httpgetattribute?view=aspnetcore-2.1#properties) tulajdonságot.

1. Készíts egy képernyőképet (screenshot), amin látszódik

   - a fejlesztéséhez használt eszköz (pl. Visual Studio),
   - a gép és a felhasználó neve, amin a fejlesztést végezted (pl. konzolban add ki a `whoami` parancsot és ezt a konzolt is rakd a képernyőképre),
   - az aktuális dátum (pl. az óra a tálcán)
   - a _controller_ osztály kódja,
   - valamint Postman-ben (vagy alternatív eszközben) kiadott sikeres teszt kérés.

   [Itt egy példa](img/img-screenshot-pl-vs.png), körülbelül ilyesmit várunk. A Postman kerüljön a Visual Studio ablaka elé, a kód elég, ha részletében látható a háttérben.

   > A képet `f1.png` néven mentsd el és add be a megoldásod részeként!

A megoldásodat Postman segítségével tudod tesztelni.

## Feladat 2: OpenAPI dokumentáció (2 iMsc pont)

> Az iMsc pont megszerzésére a első feladat megoldásával együtt van lehetőség.

Az OpenAPI (korábbi nevén Swagger) egy REST API dokumentációs eszköz. Célja hasonló a Web Service-ek esetében használt WSDL-hez: leírni az API szolgáltatásait egy standardizált formában. A korábbi feladatok megoldása után készíts [OpenAPI specifikációt és dokumentációt](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-2.1) a REST API leírásához.

1. A megoldáshoz kövesd a Microsoft hivatalos dokumentációját: <https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-2.1&tabs=visual-studio>

   - Mindenképpen a Swashbuckle opciót használd.
   - A `swagger.json`-t az alkalmazás maga generálja (nem kézzel kell megírnod), és a `/swagger/v1/swagger.json` címen legyen elérhető.
   - Állítsd be a _Swagger UI_-t is a `/swagger` címen.
   - (Az "XML comments" résszel és egyéb testreszabással nem kell foglalkoznod.)

1. Indítsd el a webalkalmazást, és nézd meg a `swagger.json`-t <http://localhost:5000/swagger/v1/swagger.json> címen, és próbáld ki a SwaggerUI-t a <http://localhost:5000/swagger> címen.

1. Készíts egy képernyőképet a fent leírtak szerint a böngészőben megjelenő Swagger UI-ról. Az idő és a gép/felhasználónév mellett ezen a böngésző látszódjon!

   > A képet `f2.png` néven mentsd el és add be a megoldásod részeként!

1. Próbáld ki a SwaggerUI "Try it out" szolgáltatását: tényleg kiküldi a kérést a webalkalmazásnak, és látod a valódi választ.

   ![SwaggerUI Try it out](img/img-swaggerui-try.png)
