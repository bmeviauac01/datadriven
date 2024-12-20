# 6. REST API Spring Boot, Java és Maven technológiákkal

A házi feladat teljesítésével **4 pont** szerezhető.

GitHub Classroom segítségével hozz létre magadnak egy repository-t. A **meghívó URL-t Moodle-ben találod**. Klónozd le az így elkészült repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. Hozz létre egy `megoldas` nevű branchet, és **arra dolgozz**. A feladatok elkészítése után kommitold és pushold a megoldásod.

A megoldáshoz szükséges szoftvereket és eszközöket lásd [itt](../index.md#szukseges-eszkozok), illetve töltsd le az [IntelliJ IDEA-t](https://www.jetbrains.com/idea/download/) (Community Edition-t, ami ingyenes).

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Egyszerű lekérdezés és OpenAPI dokumentáció (2 pont)

A létrehozott és klónozott repository-ban megtalálható a kiinduló kód váz. Nyisd meg IntelliJ IDEA-val (Community) és indítsd el. Egy konzol alkalmazásnak kell elindulnia, amely hosztolja a web alkalmazást. Próbáld ki (miközben fut a program): böngészőben nyisd meg a <http://localhost:8080/api/product> oldalt, ahol nem kell látnod elsőnek semmit. Tudod hogy miért nem?

Nézd meg a rendelkezésre álló kódot.

- Érdemes minden nagyobb változtatás előtt (plusz dependency hozzáadás, végpont hozzáadás, entity-knél új property hozzáadásnál) egy `mvn clean`-t futtatni.
- A `AdatvezRestApiApplication.java` inicializálja az alkalmazást. Ez egy Spring Boot Java webalkalmazás, maven package managerrel.
- Az alkalmazásban nincs adatbázis elérés az egyszerűség végett. A `ProductRepository` osztály teszteléshez használandó adatokat ad.
- A `ProductsController` Spring Boot Bean-ekkel példányosítja az `IProductRepository`-t.
  Látod, hogy melyik kódsorok felelősök ezekért?

Feladatok:

### Egyszerű lekérdezés

1. Add hozzá azt a maven dependency-t, amivel folyamatosan futtatásban tartod az alkalmazást:

    ```xml
    <dependency>
        <groupId>org.springframework.boot</groupId>
        <artifactId>spring-boot-starter-web</artifactId>
    </dependency>
    ```

2. A `hu.bme.aut.adatvezrestapiplushf.persistence.repositories.ProductRepository` osztályban a `Neptun` nevű mező értékében cseréld le a Neptun kódod. A string értéke a Neptun kódod 6 karaktere legyen.

    !!! warning "FONTOS"
        Az így módosított adatokról kell képernyőképet készíteni, így ez a lépés fontos.

3. Készíts egy olyan API végpontot, amivel ellenőrizhető, hogy létezik-e egy adott id-jú termék. A lekérdezéshez egy `HEAD` típusú HTTP kérést fogunk küldeni a `/api/product/{id}` URL-re. A válasz HTTP 200 vagy 404 legyen (extra tartalom/body nélkül, csak a válaszkód szükséges).
    - **Tipp**: Használd a Spring Boot válasz osztályát (`ResponseEntity< *a válasz body-nak típusa* >, és return ResponseEntity.*válasz típus*.build(*body*)`).

### OpenAPI dokumentáció

Az OpenAPI (korábbi nevén Swagger) egy REST API dokumentációs eszköz. Célja hasonló a Web Service-ek esetében használt WSDL-hez: leírni az API szolgáltatásait egy standardizált formában. A korábbi feladatok megoldása után készíts OpenAPI specifikációt és dokumentációt a REST API leírásához.

1. A `swagger.json`-t az alkalmazás maga generálja (nem kézzel kell megírnod), és a `/swagger/v3/swagger.json` címen elérhető alapból. Ezt állítsd át a `/neptun_code/swagger.json` címre.

    - **Tipp**: Ezt a Spring Boot-tal és a maven-nel egyszerűen megteheted, hozzá kell adnod a megfelelő dependency-t:

    ```xml
        <dependency>
            <groupId>org.springdoc</groupId>
            <artifactId>springdoc-openapi-starter-webmvc-ui</artifactId>
            <version>${springdoc-openapi.version}</version>
        </dependency>
    ```

2. Állítsd be a Swagger UI-t is, ez a `/neptun_code` címen legyen elérhető. Ezt, ahogyan az előzőt, az `application.properties`-ben a megfelelő property beállításával tudod megtenni. Ez is egy alias (proxy) lesz az eredeti elérési útvonalra. A saját Neptun kódod legyen a proxy csupa kisbetűvel.

3. A Spring Boot alkalmazások alapból a 8080 porton mennek. Állítsd át a 8000-re.
    - **Tipp**: itt is csak az `application.properties`-t kell állítanod).

4. Indítsd el a webalkalmazást, és nézd meg a `swagger.json`-t [http://localhost:8000/neptun_code/swagger.json](http://localhost:8000/neptun_code/swagger.json) címen, és próbáld ki a Swagger UI-t [http://localhost:8000/neptun_code](http://localhost:8000/swagger-ui/index.html) címen.

5. Próbáld ki a SwaggerUI "Try it out" szolgáltatását: tényleg kiküldi a kérést a webalkalmazásnak, és látod a valódi választ.
    
6. Készítd el azt az API végpontot, ami vissza is adja a kívánt terméket (`Product`) az id-ja alapján; a kérés `GET` típusú legyen a `/api/product/{id}` címre, és a válasz vagy 200 legyen az adattal, vagy 404, ha nincs ilyen elem. Ellenőrizd a SwaggerUI segítségével vagy Postman-nel.

!!! example "BEADANDÓ"
    A módosított forráskódot töltsd fel. Ügyelj rá, hogy a `pom.xml` fájl is módosult a hozzáadott maven csomagokkal!

    Készíts egy képernyőképet a böngészőben megjelenő `swagger.json` -ról. Ügyelj rá, hogy az URL-ben látható legyen, hogy a `/neptun/swagger.json` címen szolgálja ki a rendszer a saját Neptun kódoddal. A képet `f1.png` néven mentsd el és add be a megoldásod részeként!

## Feladat 2: Termék műveletek (2 pont)

A termékekkel kapcsolatos leggyakoribb adatbázisműveletek az új beszúrása, meglévő termék lekérdezése, módosítása vagy törlése, vagyis a CRUD (create, read, update és delete) műveletek. Ezekhez dedikált végpontokat készítünk, amiken keresztül a műveletek végrehajtását el tudja végezni az API használója. Ebben a feladatban a leggyakoribb végpontokat kell implementálni a már meglévő lekérdezés mellé.

1. Készíts egy olyan API végpontot, ami beszúr egy új terméket (`Product`) az id-ja alapján; a kérés `POST` típusú legyen a `/api/product` címre, a kérés törzsében várja az új `Product` értéket, és a válasz vagy 201 legyen, vagy 409, ha már van ilyen nevű elem.
    - **Tipp:** Használd a Spring Boot válasz osztályát (`ResponseEntity< *a válasz body-nak típusa* >`, és `return ResponseEntity.*válasz típusok*.build(*body*)`).

2. Készíts egy olyan API végpontot, ami módosít egy terméket (`Product`) az id-ja alapján; a kérés `PUT` típusú legyen a `/api/product/{id}` címre, a kérés törzsében várja a változtatott `Product` értéket, és a válasz vagy 204 legyen tartalom nélkül, vagy 404, ha nincs ilyen elem.

3. Készíts egy olyan API végpontot, ami töröl egy terméket (`Product`) az id-ja alapján; a kérés `DELETE` típusú legyen a `/api/product/{id}` címre, és a válasz vagy 204 legyen tartalom nélkül, vagy 404, ha nincs ilyen elem.

4. Készíts egy olyan API végpontot, amivel lekérdezhető, hány féle termék van összesen. (Például a lapozást elősegítendő kiszámolhatja a frontend, hogy hány lap lesz.) Ez is egy `GET` típusú kérés legyen a `/api/product/-/count` címre. A visszaadott adat a `CountResult` osztály példánya legyen kitöltve a darabszámmal (természetesen JSON formában).

    ??? question "Miért van a `/-` rész az URL-ben?"
        Ahhoz, hogy ezt megértsük, gondoljuk át, mi lehetne az URL: termékek darabszámára vagyunk kíváncsiak, tehát `/api/product/`, de utána mi? Lehetne `/api/product/count`. Viszont ez "összekeveredik" a `/api/product/123` jellegű URL-lel, ami egy konkrét termék lekérdezésére szolgál. A gyakorlatban a két URL együtt tudna működni, mert a termék azonosító most szám, így a keretrendszer felismeri, hogy ha `/123` az URL vége, akkor a termék ID-t váró végpontot kell végrehajtani, ha pedig `/count` az URL vége, akkor a számosságot megadót. De ez csak akkor működik, ha az ID int. Ha szöveg lenne a termék azonosítója, probléma lenne. Ilyen esetekben olyan URL-t kell "kitalálni", ami nem ütközik. A `/-` rész azt jelzi, hogy ott _nem_ termék azonosító utazik.

!!! example "BEADANDÓ"
    A módosított forráskódot töltsd fel.

    Emellett készíts egy képernyőképet Postman-ből (vagy más teszteléshez használt eszközből), amely egy sikeres termék lekérés eredményét mutatja. A képen legyen látható a kérés és a válasz minden részlete (kérés típusa, URL, válasz kódja, válasz tartalma). A válaszban a névben szerepelnie kell a **Neptun kódodnak**. A képet `f2.png` néven mentsd el és add be a megoldásod részeként!

!!! important "MÉG NEM VÉGEZTÉL"
    Ha push-oltad a kódodat, készíts egy PR-t, amihez rendeld hozzá a gyakorlatvezetődet! (részletek: [a házi feladat leadása](../GitHub.md) oldalon)


## Feladat 3: Termék részleges frissítése (Plusz pont)

!!! note ""
    A pont megszerzése.

A `Product` és a `CountResult` osztály megírt getter és setter függvényeket tartalmaz alapból. Ezeket lehet egyszerűsíteni a `lombok` annotációkkal, amik a gyakoran megírt kódsorokat válltják ki, jelen esetben a `@Getter` és `@Setter` annotációval, így átláthatóbb lesz a kód.
Ezeket lehet csak field-ekre alkalmazni, és akkor specifikus lesz:

```java
   private @Getter @Setter int ID;
```

De a teljes osztályra is akár, és akkor az osztály összes field-jére érvényesül:

```java
    @Getter
    @Setter
    public class Product {}
```

Ez azért hasznos tudni, mert JPA használatakor gyakran használatos a `lombok` `@Data` annotáció, ami magába foglalja az említett annotációkat, és másokat, amik hasznosak az osztály használatakor (`@RequiredArgsConstructor` `@ToString` `@EqualsAndHashCode`)
A `lombok` package-t is maven-nel lehet hozzáadni a projekthez, de ez már megvan.

1. **Melyik package ez?**
2. **Alakítsd át a Product osztályt, hogy annotációval oldja meg a getter és setter függvényeket az összes field-re!**
    - A felesleges kódrészletet kommentezd ki! 

!!! example "BEADANDÓ"
    A módosított forráskódot töltsd fel.

    Emellett készíts egy képernyőképet a `pom.xml`-ből arról a részről, ami a `lombok` annotációkért felelős, amely egy sikeres részleges módosítás eredményét mutatja. A képet `f3.png` néven mentsd el és add be a megoldásod részeként!
