# OpenAPI 3 és a Swagger

## OpenAPI vs Swagger, mik is a különbségek?
2017-ben az OpenAPI 3.0 megjelenése elég nagy mérföldkőnek számított. Ez volt az első hivatalos kiadás 2015 óta, amikor is a SmartBear Software az OpenAPI Intiative-nek ajándékozta a jogokat, illetve megtörtént a Swagger Specification -> OpenAPI Specification névváltás.

A különbség legegyszerűbben így érthető meg: 
- **OpenAPI** = specifikáció
- **Swagger** = eszközök a specifikáció megvalósításához

Az OpenAPI a specifikáció hivatalos neve. A specifikáció fejlesztését az OpenAPI Initiative segíti, amelyben több mint 30 szervezet vesz részt az IT világ különböző területeiről - köztük a Microsoft, a Google, az IBM és a CapitalOne. A Swagger eszközök fejlesztését vezető Smartbear Software szintén tagja az OpenAPI Initiative-nek, és segíti a specifikáció fejlődését.A Swagger eszköztár nyílt forráskódú, ingyenes és kereskedelmi eszközök keverékét tartalmazza, amelyek az API életciklus különböző szakaszaiban használhatók:

 - **Swagger Editor**:  lehetővé teszi az OpenAPI-specifikációk YAML-ben történő szerkesztését a böngészőben, valamint a dokumentációk valós idejű előnézetét.
 - **Swagger UI**: egy HTML, Javascript és CSS eszközökből álló gyűjtemény, amely dinamikusan gyönyörű dokumentációt generál egy OAS-kompatibilis API-ból.
 - **Swagger Codegen**: Lehetővé teszi az API klienskönyvtárak (SDK generálása), szerver csonkok és dokumentáció automatikus generálását egy OpenAPI Spec alapján.
 - **Swagger Parser**: Önálló könyvtár az OpenAPI definíciók Java-ból történő elemzésére.
 - **Swagger Core**: Java-alapú könyvtárak az OpenAPI-definíciók létrehozásához, fogyasztásához és az azokkal való munkához.
 - **Swagger Inspector** (ingyenes): API-tesztelő eszköz, amely lehetővé teszi az API-k validálását és OpenAPI-definíciók generálását egy meglévő API-ból.
 - **SwaggerHub** (ingyenes és kereskedelmi): API-tervezés és dokumentáció, az OpenAPI-val dolgozó csapatok számára készült.
 
A Swagger számtalan lehetőséget nyújt a fejlesztők számára a leírások elkészítéséhez. Ezeket a leírásokat általában JSON vagy YAML nyelveken készítjük. A hivatalos források mind a YAML nyelvet ajánlják, ugyanis könnyebben olvasható és gyorsabban megérthető. 
 
Mélyedjünk el egy kicsit az OpenAPI 3.0 használatában.

 ## OpenAPI 3.0
 ### _Meta információk:_
 A metainformációk szakaszban adhatsz meg információkat az általános API-ról. Ebben a szakaszban olyan információkat lehet megadni, mint például, hogy mit csinál az API, mi az API alap URL címe és milyen webes protokollt követ.
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

 ### _Útvonal elemek (Path items)_
Az általunk készített API végpontjai. Az alap URL-hez képes relatív elérési úton érhetjük el őket, jelen esetben `/artist` útvonalon. Specifikálhatunk bennük HTTP igéket, melyeket a végpontok használnak, ilyen például a `get`
 ```cs
 ASP.NET megfelelő:
 [HttpGet("-/artists")]
 ```
```yaml
...
paths:
  /artists:
    get:
     description: Returns a list of artists 
...
```

 ### _Válaszok_
 A végpontokban definiálnunk kell a válaszokat amiket a kliens kaphat, ezeket a HTTP igék leírása alatt a `responses` tulajdonsághoz kell írnunk. Ehhez szükségünk van egy HTTP státusz kódra, illetve egy leírásra, amely a válasz sémáját ismerteti. 
 ```cs
 ASP.NET contorller:
 [HttpGet("-/artists")]
 public IEnumerable<Artist> GetAll(){
     return artistRepository.List();
 }
 ```
 ```yaml
...
paths:
    ...
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
 
 ## _Paraméterek_
 Lehetőségünk van számtalan paraméter megadására, mellyel a kliensek és a szerver közötti kommunikáció specifikusságát tudjuk növelni. Ilyen paraméter például a lekérdezés paraméter (query parameter). Ahhoz, hogy egy ilyen :`GET http://example.com/v1/artists?limit=20&offset=3` kérést ki tudjunk szolgálni a fenti kódot az alábbi sorokkal kell bővítenünk:
  ```cs
 ASP.NET contorller:
 [HttpGet("-/artists")]
 public IEnumerable<Artist> GetByLimOffs(int limit, int offset){
     return artistRepository.List(limit,offset);
 }
 ```
 ```yaml
 ...
    get:
    ...
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
 
 A lekérdező paramétereken túl még fontos a felhasználói jogkörök paraméterezése. Az alábbi példában egy olyan végpontot készítünk, melyben az artist visszakapja a felhasználó nevéhez kapcsolódó adatokat : `/artists/{username}`
   ```cs
 ASP.NET contorller:
 [HttpGet("-/artists/{username}")]
 public IEnumerable<Artist> GetByUsername(string username){
     return artistRepository.GetByUname(username);
 }
 ```
 ```yaml
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

Példakódjaink egy alap API kezdetleges változatát állították elő. Jól látható, hogy ez a kód egy komolyabb API leírásnál hatalmas mennyiséget is elérhet. Fontos tisztázni azt, hogy az API leírásához egy már logikailag összerakott adatbázist és egy azt reprezentáló modellt kell alkotnunk így megkönnyítve a dolgunk. 


 ## Polimorfizmus és öröklés OpenAPI 3.0 alatt
Az OpenAPI specifikációja lehetőséget biztosít arra, hogy a közös tulajdonságok ne alkossanak sormintát, ezzel csúnyává téve az amúgy is elég nagy kódot. Nézzünk erre egy példát: 

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
          allOf:     # Combines the BasicErrorModel and the inline model
            - $ref: '#/components/schemas/BasicErrorModel'
            - type: object
              required:
                - rootCause
              properties:
                rootCause:
                  type: string
```

Itt a legfontosabb elem az `allOf` kulcsszó. Az `allOf` segít abban, hogy egy relatív elérési út megadásával egyesítsük az általunk készített leszármazott attribútumait az elérési út túloldalán álló model attribútumaival.  Van egy szabály, amellyet ajánlott követnünk az `allOf` parancsnál, ez pedig nem más mint,hogy ne használjunk azonos attribútum neveket különböző adattípusokkal. Ha ilyen előfordul a kódban az számtalan hibához vezethet. 

A polimorfizmus megoldásához az `allOf`-hoz hasonló `oneOf` és `anyOf` kulcsszavakat tudjuk használni. 
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
Használatát tekintve a `oneOf` parancs lehetővé teszi, hogy a válaszban érkezett csomagok tartalmazhatnak `simpleObject` és `complexObject` objektum sémát is, de csak egyet. Az `anyOf` parancs ettől eltérően több objektum séma detektálását is biztosítja.

A szakirodalom véleménye megoszlik a a polimorfizmusról mint lehetséges `edge-case`-ről. Az OpenAPI 3.0 kiadásával megjelentek a már fent említett parancsok, viszont még 2019-ben is számtalan olyan szolgáltatás volt, amely nem támogatta ezek használatát. Ha tehetjük ne használjunk polimorfizmust az ilyen problémák elkerülése végett. 


A jegyzet elkészítéséhez a hivatalos dokumentáció volt a segítségemre:
 - https://swagger.io/docs/specification/data-models/oneof-anyof-allof-not/
 - https://swagger.io/docs/specification/data-models/inheritance-and-polymorphism/
 - https://support.smartbear.com/swaggerhub/docs/tutorials/writing-swagger-definitions.html
 - https://support.smartbear.com/swaggerhub/docs/tutorials/openapi-3-tutorial.html
