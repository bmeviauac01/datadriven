# OpenAPI 3 és a Swagger

A REST API-ról tudjuk, hogy nem szabvány, hanem "architekturális stílus". Azaz nem szigorú szabályokat követ, hanem irányelveket és szokásokat. Felmerül a kérdés, hogy amennyiben egy REST API-t integrációs eszköznek kívánunk használni, hogyan tudjuk minél szabványosabb módon dokumentálni? Web Service-ek esetén a _SOAP_ szabvány, _WSDL_, _DTD_ és _XSD_ leírók valók erre a célra. Egy REST API-nál pedig az **OpenAPI**.

## OpenAPI 3.0

Az OpenAPI tehát egy API leíró, amely egy REST API-t publikáló webalkalmazásban egy fájlként érhető el a webszerverről. Ez a specifikáció tartalmazza az API műveleteit, adattípusait, dokumentációját. Nézzük a tartalmát.

!!! warning ""
    OpenAPI specifikációt a legritkább esetben írunk kézzel, helyette generáljuk a REST API-t megvalósító rendszerben technológiaspecifikus eszközökkel. Az alábbiakban a példák tehát csak szemléltetik, hogyan néz ki egy OpenAPI specifikáció és milyen koncepciókkal dolgozhatunk.

### Meta információk

A metainformációk szakaszban adhatunk meg információkat az API-ról általánosan. Ebben a szakaszban olyan információkat lehet megadni, mint például, hogy mit csinál az API, mi az API alap URL címe, milyen webes protokollt követ, és milyen autentikációs megoldásokat támogat.

```yaml
openapi: 3.0.0
info:
  version: 1.0.0
  title: Simple Artist API
  description: A simple API to illustrate OpenAPI concepts

servers:
  - url: https://example.io/v1

# Basic authentication
components:
  securitySchemes:
    BasicAuth:
      type: http
      scheme: basic
security:
  - BasicAuth: []

paths: {}
...
```

### API végpontok

Az általunk készített API végpontjait adjuk meg a `paths` alatt. Az alap URL-hez képes relatív elérési úton érhetjük el őket, jelen esetben `/artist` útvonalon. Specifikálhatunk bennük HTTP igéket, melyeket a végpontok használnak, ilyen például a `get`.

```yaml
...
paths:
  /artists:
    get:
     description: Returns a list of artists 
...
```

Ha ASP.NET controllert képzelünk a végpont mögé, akkor a controller metódus valahogy így néz ki:

```csharp
[HttpGet("artists")]
public IEnumerable<Artist> GetAll(){
    return artistRepository.List();
}
```

A végpontokban definiálnunk kell a válaszokat is, amiket a kliens kaphat. Ezeket a HTTP igék leírása alatt a `responses` tulajdonsághoz kell írnunk. Ehhez szükségünk van egy HTTP státusz kódra, illetve egy leírásra, amely a válasz sémáját ismerteti.

```yaml
paths:
  /artists:
    get:
      responses:
        '200':
          description: Successfully returned a list of artists
          content:
            application/json:
              schema:
                type: array
                items:
                  type: object
                  required:
                    - username
                  properties:
                    artist_name:
                      type: string
                    artist_genre:
                      type: string
                    albums_recorded:
                      type: integer
                    username:
                      type: string

        '400':
          description: Invalid request
          content:
            application/json:
              schema:
                type: object
                properties:   
                  message:
                    type: string
...
```

A fenti 200-as státusz kód esetén visszaadott lista egy eleme C#-ban az alábbiak szerint nézhet ki.

```csharp
class Artist
{
  [Required]
  [JsonPropertyName("username")]
  public string Username { get; set; }

  [JsonPropertyName("artist_name")]
  public string ArtistName { get; set; }

  [JsonPropertyName("artist_genre")]
  public string ArtistGenre { get; set; }

  [JsonPropertyName("albums_recorded")]
  public int AlbumsRecorded { get; set; }
}
```

### Paraméterek

Lehetőségünk van számtalan paraméter megadására, mellyel a kliensek és a szerver közötti kommunikáció specifikusságát tudjuk növelni. Ilyen paraméter például a lekérdezés paraméter (query parameter). Ahhoz, hogy egy például egy `GET http://example.com/v1/artists?limit=20&offset=3` kérést ki tudjunk szolgálni a fenti kódot az alábbi sorokkal kell bővítenünk:

```csharp
[HttpGet("artists")]
public IEnumerable<Artist> GetByLimOffs(int limit, int offset){
  return artistRepository.List(limit, offset);
}
```

Ennek az OpenAPI leírása pedig az alábbi lesz:

```yaml
paths:
  /artists:
    get:
      parameters:
        - name: limit
          in: query
          description: Limits the number of items on a page
          schema:
            type: integer
        - name: offset
          in: query
          description: Specifies the page number of the artists to be displayed
...
```

Az query string mellett az URL path részében is szállítható paraméter, például a `GET http://example.com/v1/artists/{username}` formájában. Ekkor a C# kód így néz ki:

```csharp
[HttpGet("artists/{username}")]
public IEnumerable<Artist> GetByUsername(string username){
    return artistRepository.GetByUname(username);
}
```

Ehhez pedig az alábbi OpenAPI specifikáció részlet tartozik.

```yaml
paths:
  /artists/{username}:
    get:
      description: Obtain information about an artist from his or her unique username
      parameters:
        - name: username
          in: path
          required: true
          schema:
            type: string
          
      responses:
        '200':
          description: Successfully returned an artist
          content:
            application/json:
              schema:
                type: object
                properties:
                  artist_name:
                    type: string
                  artist_genre:
                    type: string
                  albums_recorded:
                    type: integer
                
        '400':
          description: Invalid request
          content:
            application/json:
              schema:
                type: object 
                properties:           
                  message:
                    type: string
```

Példakódjaink egy alap API kezdetleges változatát állították elő. Jól látható, hogy ez a kód egy komolyabb API leírásnál hatalmas mennyiséget is elérhet. Fontos látni tehát, hogy az API leírásához előbb a logikailag összerakott adatbázist és egy azt reprezentáló modellt kell alkotnunk, mert csak az után képzelhető el az API modellje - amit ráadásul az előbbiekből fogunk tipikusan generálni.

## Polimorfizmus és öröklés OpenAPI 3.0 alatt

Az öröklés és polimorfizmus is objektumorientált fogalmak. Az OpenAPI viszont nem objektumorientált - hiszen nem programozási nyelv. Mégis, az OpenAPI-ban is felmerül a kérdés, hogyan lehet ismétlődő adatrészeket "kiemelni", azaz, hogy a "közös" sémát ne kell sokszor ismételni. Objektumorientált nyelvben erre természetesen a leszármazás a megoldás. Az OpenAPI specifikációja is lehetőséget biztosít arra, hogy a közös tulajdonságok ne alkossanak sormintát, ezzel csúnyává téve az amúgy is hosszú OpenAPI definíciót. Nézzünk erre egy példát.

Tegyük fel, hogy két féle hibaüzenetet tudunk visszaadni, az egyik "részhalmaza" a másiknak. C#-ben mindez így nézne ki:

```csharp
class BasicErrorModel
{
  public string Message { get; set; }
  public int Code { get; set; }
}
class ExtendedErrorModel : BasicErrorModel
{
  public string RootCause { get; set; }
}
```

Ez OpenApi-ban az `allOf` kulcsszó használatával írható le, és így néz ki:

```yaml
    components:
      schemas:
        BasicErrorModel:
          type: object
          required:
            - message
            - code
          properties:
            message:
              type: string
            code:
              type: integer
              minimum: 100
              maximum: 600
        ExtendedErrorModel:
          allOf:     # "átveszi" a hivatkozott modell összes elemét és kiegészíti továbbiakkal
            - $ref: '#/components/schemas/BasicErrorModel'
            - type: object
              required:
                - rootCause
              properties:
                rootCause:
                  type: string
```

Az `allOf` segít abban, hogy egy relatív elérési út megadásával egyesítsük az általunk készített "leszármazott" attribútumait az elérési út túloldalán álló modell attribútumaival. Van egy szabály, amelyet ajánlott követnünk az `allOf` parancsnál, ez pedig nem más mint,hogy ne használjunk azonos attribútum neveket különböző adattípusokkal. Ha ilyen előfordul a kódban az számtalan hibához vezethet.

Másik, objektumorientált világban gyakran használt eszköz a _polimorfizmus_, azaz amikor a konkrét példány egy leszármazási hierarchia bármely típusa lehet. A polimorfizmus megoldásához az `allOf`-hoz hasonló `oneOf` kulcsszót tudjuk használni. Az alábbi példában a `oneOf` parancs lehetővé teszi, hogy az adat `simpleObject` vagy `complexObject` sémát is tartalmazhasson, de midig csak az egyiket.

```yaml
...
    components:
      responses:
        sampleObjectResponse:
          content:
            application/json:
              schema:
                oneOf:
                  - $ref: '#/components/schemas/simpleObject'
                  - $ref: '#/components/schemas/complexObject'
     ...
    components:
      schemas:
        simpleObject:
          ...
        complexObject:
          ...
```

Amennyiben ezt C#-ban próbáljuk elképzelni, akkor `SimpleObject` és `ComplexObject` is osztályok, amelyek tipikusan (de nem szükségszerűen) rendelkeznek egy közös őssel.

!!! note ""
    Azon túl, hogy a polimorfizmust a fentiekben a sémában kifejeztük, valóban még nem vagyunk készen. Az OpenAPI ugyanis csak a sémáról szól. Arról nem, hogy a C#/Java/stb. kódban a tényleges adatból hogyan keletkezhet objektum. Hiszen ez nem az OpenAPI felelőssége - ez a sorosításra tartozik. A deszerializálás (JSON -> objektum) során kell majd valójában eldönteni, hogy az adat, ami érkezik, az melyik típusnak felel meg. Ezt a tipikus JSON sorosító könyvtárak csak külön konfiguráció árán tudják elvégezni. Ezzel tehát érdemes vizsgázni, az OpenAPI csak a probléma felét oldotta meg.

    A szakirodalom véleménye ezért is megoszlik a a polimorfizmusról mint lehetséges `edge-case`-ről. Az OpenAPI 3.0 kiadásával megjelentek a már fent említett parancsok, viszont még 2019-ben is számtalan olyan szolgáltatás volt, amely nem támogatta ezek használatát.

## OpenAPI vs Swagger, mik is a különbségek?

Az OpenAPI és a Swagger a szakmában többnyire szinonimaként jelenik meg, de mást jelentenek hivatalosan. Az OpenAPI a "Swaggerből jött létre", annak szabványosított változata, míg a Swagger egy szoftvercsomag. 2017-ben az OpenAPI 3.0 megjelenése elég nagy mérföldkőnek számított. Ez volt az első hivatalos kiadás 2015 óta, amikor is a _SmartBear Software_ az _OpenAPI Initiative_-nek ajándékozta a jogokat, illetve megtörtént a _Swagger Specification_ -> _OpenAPI Specification_ névváltás.

A különbség legegyszerűbben így érthető meg:

- **OpenAPI** = specifikáció
- **Swagger** = eszközök a specifikáció megvalósításához

Az **OpenAPI** a specifikáció hivatalos neve. A specifikáció fejlesztését az _OpenAPI Initiative_ segíti, amelyben több mint 30 szervezet vesz részt az IT világ különböző területeiről - köztük a Microsoft, a Google, az IBM és a CapitalOne. A Swagger eszközök fejlesztését vezető Smartbear Software szintén tagja az OpenAPI Initiative-nek, és segíti a specifikáció fejlődését.A Swagger eszköztár nyílt forráskódú, ingyenes és kereskedelmi eszközök keverékét tartalmazza, amelyek az API életciklus különböző szakaszaiban használhatók:

- **Swagger Editor**:  lehetővé teszi az OpenAPI-specifikációk YAML-ben történő szerkesztését a böngészőben, valamint a dokumentációk valós idejű előnézetét.
- **Swagger UI**: egy HTML, JavaScript és CSS eszközökből álló gyűjtemény, amely dinamikusan gyönyörű dokumentációt generál egy OAS-kompatibilis API-ból.
- **Swagger Codegen**: Lehetővé teszi az API klienskönyvtárak (SDK generálása), szerver csonkok és dokumentáció automatikus generálását egy OpenAPI Spec alapján.
- **Swagger Parser**: Önálló könyvtár az OpenAPI definíciók Java-ból történő elemzésére.
- **Swagger Core**: Java-alapú könyvtárak az OpenAPI-definíciók létrehozásához, fogyasztásához és az azokkal való munkához.
- **Swagger Inspector** (ingyenes): API-tesztelő eszköz, amely lehetővé teszi az API-k validálását és OpenAPI-definíciók generálását egy meglévő API-ból.
- **SwaggerHub** (ingyenes és kereskedelmi): API-tervezés és dokumentáció, az OpenAPI-val dolgozó csapatok számára készült.

A Swagger számtalan lehetőséget nyújt a fejlesztők számára a leírások elkészítéséhez. Ezeket a leírásokat általában JSON vagy YAML nyelveken készítjük. A hivatalos források mind a YAML nyelvet ajánlják, ugyanis könnyebben olvasható és gyorsabban megérthető.

## Források

A jegyzet elkészítéséhez a hivatalos dokumentáció szolgált forrásként:

- <https://swagger.io/docs/specification/data-models/oneof-anyof-allof-not/>
- <https://swagger.io/docs/specification/data-models/inheritance-and-polymorphism/>
- <https://support.smartbear.com/swaggerhub/docs/tutorials/writing-swagger-definitions.html>
- <https://support.smartbear.com/swaggerhub/docs/tutorials/openapi-3-tutorial.html>
