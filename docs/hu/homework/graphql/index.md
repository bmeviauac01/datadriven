# 5. GraphQL

GraphQL házi feladat, a teljesítéssel **4 pont és 3 iMsc pont** szerezhető.

GitHub Classroom segítségével hozz létre magadnak egy repository-t. A **meghívó URL-t Moodle-ben találod**. Klónozd le az így elkészült repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. A feladatok elkészítése után kommitold és pushold a megoldásod.

A megoldáshoz szükséges szoftvereket és eszközöket lásd [itt](../index.md#szukseges-eszkozok).

Előkészületként hozz létre egy új adatbázist, a [gyakorlatanyagban](../../seminar/mssql/index.md) leírt módon.

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Projekt kiegészítése és lekérdezések (2 pont)

A házi feladat feladat célja, hogy gyakorlati tapasztalatot szerezzetek a GraphQL használatában egy saját API létrehozásával.
A GraphQL egy erőteljes eszköz, amely lehetővé teszi az adatok lekérdezését és módosítását egyetlen, jól strukturált kérésben, rugalmasabbá és hatékonyabbá téve az API-k használatát.
A GraphQL API működése kissé eltér a hagyományos REST API-tól: a végponton rugalmas, szabályozott lekérdezésekkel pontosan megadhatja a kliens, hogy milyen adatokat szeretne visszakapni.
A gyakorlat célja, hogy megmutassa, hogyan érhetők el komplex adatok egyszerűen.

Ebben a házi feladatban a megszokott adatmodell lesz használva, megrendelések, termékek, kategóriák és hozzájuk kapcsolódó információk lekérdezésével.
Az adatmodell, a DBContext és az entitások már megtalálhatóak a kiinduló projektben.

### GraphQL végpontok létrehozása

Az első feladatban megnézzük hogyan lehet egy már létező projekthez felvenni GraphQL végpontokat a Hot Chocolate szerver oldali könyvtár segítségével.
Ehhez először felvesszük a szükséges csomagokat, kiajánlunk végpontokat, és elkészítjük azt az osztályt, ami visszaadja a szükséges adatokat.

1. Vegyél fel egy `Query` osztályt, amibe tegyél egy `GetProducts` függvényt egy `AdatvezDbContext` paraméterrel. Ez a függvény fog a `products` query végpontjaként szolgálni. A paraméter DBContext típust a DI container fogja majd injektálni a metódus hívásakor. Implementáld is a metódust, hogy visszaadja a termékeket és a hozzájuk tartozó kategóriákat.

1. Vedd fel a következő NuGet csomagokat a zárójelben látható verziókkal:

- HotChocolate.AspNetCore (14.1.0)
- HotChocolate.Data (14.1.0)
- HotChocolate.Data.EntityFramework (14.1.0)

1. A belépési pontba (`Program.cs`) regisztráld be a GraphQLServer szolgáltatást az alábbi kód segítségével:
(itt három dolog történik: beregisztráljuk a GraphQLServer szolgáltatást, az `AdatvezDBContext`-t, hogy injektálható legyen, valamint a `Query` osztályt, mint lekérdezések végpontjaiként funkcionáló osztályt)

    ```csharp
    builder.Services
        .AddGraphQLServer()
        .RegisterDbContextFactory<AdatvezDbContext>()
        .AddQueryType<Query>();
    ```

1. Add hozzá az útválasztást (végpontok kezelésének képességét) és hozz létre egy alapértelmezett végpontot a következő kódsorok segítségével:

    ```csharp
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGraphQL();
    });
    ```

1. Indítsd el az alkalmazást és navigálj a `http://localhost:5000/graphql/` oldalra. Itt láthatod a Banana Cake Pop nevű interaktív eszközt, amivel lehetséges lekérdezések és mutációk futtatás, a séma megtekintése, valamint az API-hoz tartozó dokumentációt is lehet böngészni. A lekérdezések tabon a következő kóddal ki tudod próbálni a fenti függvény futását:

    ```json
    query {
        products {
            name
            category {
                name
            }
        }
    }
    ```

!!! note ""
    Vizsgáljuk meg, hogy mi történik.

    - Amikor a Hot Chocolate vagy más GraphQL szerver egy kérést kap, minden mezőt egy resolver kezel, amely felelős az adott adat lekéréséért, előállításáért.
    
    - A products mezőhöz implicit módon kapcsolódik a GetProducts metódus, a Hot Chocolate automatikusan összeköti őket.
    
    - A Hot Chocolate és az Entity Framework (EF) integrációja lehetővé teszi, hogy a GraphQL API közvetlenül adatokat kérjen le az adatbázisból az EF-en keresztül, ami leegyszerűsíti a GraphQL végpontok implementálását és működését.

!!! note  "Connection string"
    Ha nem LocalDB-t használsz, az `appsettings.json`-ban a connection stringet lehet, hogy módosítanod kell.

### Lekérdezések

1. Készíts az előző feladat alapján egy olyan lekérdezést, amely képes a termékeket visszaadni valamilyen szűrés elvégzése után. A `Query` osztályba vedd fel a `ProductsByCategory` függvényt ami az `AdatvezDBContext`-en kívül egy `categoryName` nevű `string` paraméterrel rendelkezik. Az implementációban szűrj a kategória nevére: azokat a terméket add vissza, amelyek kategóriájának neve megegyezik a kapott paraméterrel. A teszteléshez használd az alábbi lekérdezést:

    ```json
    query {
        productsByCategory(categoryName: "Months 0-6") {
            id
            name
            price
        }
    }
    ```


1. A következő feladatban láthatjuk a GraphQL erejét. Amennyiben sok táblából kell összeválogatni a szükséges információt, sok felesleges adat is keresztül mehetne a hálózaton. Ehhez képest, ha tudjuk mik fognak kelleni, elég csupán a szükséges adatokat lekérdezni és elküldeni. Készíts egy lekérdezést, ami megrendeléseket ad vissza és a következő adatokat szedi össze:

    - A megrendelő nevét

    - A megrendelésben szereplő termékek neveit

    - A termékek kategóriáit

    - Hány darabot rendeltek az adott termékből.

    ```json
    query {
        orders {
            customerSite {
                customer {
                    name
                }
            }
            orderItems {
                amount
                product
                {
                    name
                    category
                    {
                        name
                    }
                }
            }
        }
    }
    ```

1. Lekérdezés optimalizálása: az alkalmazás indításakor megnyíló konzolon megtekintheted a lekérdezést is, amit a Hot Chocolate generál Entity Framework segítségével. Láthatod, hogy a select utasításban elég sok olyan mező is lekérdezésre kerül, ami egyébként nem kéne, pedig a lekérésben egyértelműen megfogalmaztuk, hogy mik kellenek. A Hot Chocolate a `[UseProjection]` annotációval ellátott függvények esetén a beérkező kéréseket az adatbázis számára közvetlen transzformálja. A használatához az annotáció hozzáadásán kívül két módosítás szükséges. Az első, hogy a függvény visszatérési értéke `IQuryable<Order>` legyen. A második pedig, hogy a következő hívással beregisztráld a projection elérhetőségét:

    ```csharp
    builder.Services
        .AddGraphQLServer()
        .RegisterDbContextFactory<AdatvezDbContext>()
        .AddQueryType<Query>()
        //új sor:
        .AddProjections(); 
    ```

Nézd meg konzolon az SQL lekérdezés megváltozását. Az implementáció is csökkenthet, innentől kezdve az explicit `Include` hívások sem lesznek szükségesek.

!!! example "BEADANDÓ"
    A módosított C# forráskódot töltsd fel.

    Emellett készíts egy képernyőképet a Banana Cake Pop felületén készített lekérdezésről és a válaszról, amiben szerepelnek a szükséges adatok. A képet `f1.png` néven mentsd el és add be a megoldásod részeként!

## Feladat 2: Módosítások GrapQL segítségével  (2 pont)

Természetesen a GraphQL nem csak adatok lekérdezésére, hanem módosításokra is használható. A következő feladatokban a cél, hogy adatmanipulációkat gyakoroljunk.
Az adatmanipulációt úgynevezett _Mutation_ hajtja végre, ami egy olyan művelet, amely adatokat módosít, például rekordokat hoz létre, frissít, vagy töröl az adatbázisban.

### Termék módosítása

Az első módosítás a meglévő termékek árát fogja módosítani. Növeljük meg amennyiben a paraméterben kapott kategóriába tartoznak a termékek.

1. Vegyél fel egy Mutation osztályt `ProductMutation` néven.

1. Regisztráld be a szolgáltatást az első feladatban található `Query` regisztrálásához hasonlóan az `.AddMutationType<ProductMutation>()` függvényhívás segítségével.

1. A `ProductMutation` rendelkezzen egy `IncreaseProductPricesByCategory` függvénnyel, ami elvégzi a szükséges módosításokat és visszatér egy `IQueryable<Product>` típussal, amiben láthatjuk az új árakat. A függvény három paraméterrel rendelkezzen: 1) az `AdatvezDBContext`, 2) egy `categoryName` stringgel, és 3) egy `priceIncrease` double értékkel, ami megmondja, hogy hányszorosára dráguljon a termék. A visszatérési értéke a módosított termékek kollekciója legyen. Figyelj rá, hogy a módosításokat az adatbázisba is átvezesd.

1. Tesztelni az alábbi példa paranccsal tudod (a paraméter neve meg kell egyezzen a lenti hívásban megadott változónévvel: *categoryName*/*priceIncrease*):

    ```json
    mutation {
        increaseProductPricesByCategory(categoryName: "LEGO", priceIncrease: 1.1) {
            name
            price
            category {
                    name
                }
            }
    }
    ```

    !!! note ""
        A lekérdezés két részből áll. Az első maga a mutation hívása lesz megfelelően paraméterezve:

        ```json
        increaseProductPricesByCategory(categoryName: "LEGO", priceIncrease: 1.1)
        ```

        A második része pedig azt mondja meg, hogy a hívás utáni adatot milyen formában szeretnénk látni. Ez megegyezik a fenti lekérdezésekben látott szintaktikával. Jelen esetben például a módosított termékek nevét árát és kategóriáját szeretnénk kiírni:

        
        ```json
        {
            name
            price
            category {
                name
            }
        }
        ```

### Megrendelés beszúrása

A következő feladatban egy új megrendelés beszúrására szeretnénk lehetőséget biztosítani.
A megrendelés csak a termékek neveit és a kívánt darabszámait fogja majd várni paraméterben és az alapján hozza létre a megrendelést.

1. Vegyél fel egy függvényt a `ProductMutation` osztályba `CreateOrder` néven. A szokásos `AdatvezDbContext`-en kívül legyen két listát váró paramétere: az első egy string listát a terméknevekkel `productNames` névvel, a második pedig egy int listát vár a darabszámokkal `quantities` néven.

1. A függvénybe vegyél fel egy új `Order` példányt, amiben minden egyes paraméterben kapott terméknévhez egy új `OrderItem` kerüljön be. Az `OrderItem`-hez pedig keresd meg a név alapján és állítsd be a megfelelő `Product`-ot és darabszámot (a többi értéket figyelmen kívül hagyhatod). A függvény visszatérési értéke az újonnan elkészített `Order` példány legyen.

1. A függvény dobjon hibát amennyiben a kapott termékek száma nem egyezik meg a mennyiségekkel, valamint ha bármelyik termék neve nem található meg az adatbázisban.

1. A következő lekérdezéssel fogod tudni kipróbálni. A lekérdezés beszúrja a megrendelést két termékre, majd az utána megadott struktúrában kiírja az elkészített megrendelést.

    ```json
    mutation {
        createOrder(
            productNames: ["Lego City harbour", "Activity playgim"],
            quantities: [2, 4]
        ) {
            id
            orderItems {
            product {
                name
            }
            amount
            }
        }
    }
    ```

!!! example "BEADANDÓ"
    A módosított C# forráskódot töltsd fel.

    Emellett készíts egy képernyőképet, amelyben a módosításokat futtattad a Banana Cake Pop felületén. A képet `f2.png` néven mentsd el és add be a megoldásod részeként!

## Feladat 3: Haladó GraphQL funkciók (3 iMSc pont)

!!! note ""
    A pont megszerzésére az első két feladat megoldásával együtt van lehetőség.

A következő feladatokban a Hot Chocolate által biztosított GraphQL szerver haladóbb funkcióit használjuk ki, úgy mint a szűrést, rendezést, lapozhatóságot.
A Hot Chocolate beépített szűrés lehetőségének használatához a `Service` regisztrálásakor az `AddFiltering()` hívást kell elvégeznünk, majd a kívánt végpontot kiszolgáló metódust a `[UseFiltering]` attribútummal kell ellátni.

A következő feladatokban neked kell kitalálnod a lekérdezést is és a funkcióhoz tartozó .NET implementációt is. A leadáskor ezeket is mellékelned kell.

1. Az első feladatban található `productsByCategory` hívás kiváltható, ha a `GetProducts` függvényünkre engedélyezzük a beépített szűrést.
Tedd meg, majd teszteld egy olyan lekérdezéssel, ami kategóriák nevére szűr.
A lekérdezés elkészítésében nagy segítségedre lesz a kódkiegészítés.
A megoldás leadásakor a lekérdezést is le kell adnod `q3_1.txt` fájlban, ahol a `"Building Items"` kategóriára kell szűrnöd. A visszatérési adatok előállításakor az 1. Feladat 8-as pontja szerinti módon kell előállítani a választ.

    !!! note ""
        A szűrésnek egyedi szintaxisa van.
        A lekérdezésben a kívánt elem után egy where kifejezésben adjuk meg, hogy milyen szűrőfeltételeket szeretnénk érvényesíteni.
        Itt a séma szerint kell megadni, hogy melyik értékekre szűrünk, szeretnénk-e kombinált feltételek mentén szűrni, stb.

        ```json
        query {
            products (where .. ) {
                name
                category {
                    name
                }
            }
        }
        ```
        Ennek nagyon nagy előnye, hogy nem kell implementálni az összes fajta szűrést, ami előjöhet az API használói közt.
        Ehelyett rájuk van bízva, hogy mi szerint szeretnének szűrni a GraphQL nyelv ezt támogatja, a Hot Chocolate pedig a kliens által kért formátum és feltételekkel szűrve fogja előállítani a választ.
        
        Lehetőség van továbbá egyedi szűrőfeltételeket is létrehozni, melyekről bővebben itt olvashatsz: https://chillicream.com/docs/hotchocolate/v14/fetching-data/filtering

1. A második feladatod a rendezés és a lapozhatóság hozzáadása a `GetOrders` függvényhez. A `GetOrders` függvényt annotáld a megfelelő attribútumokkal, majd a GraphQL regisztrálásakor állítsd be a sorbarendezés és rendezés lehetőségeit. A kipróbáláshoz a lekérdezést módosítani kell, a lekérdezés összeállítása is a feladat része.
A megoldás leadásakor a lekérdezést is le kell adnod `q3_2.txt` fájlban, ahol a lekérdezés 2 darab `Order`-t tartalmaz (lapozás következtében) és `id` alapján csökkenő sorrendben tenned őket.
Figyelj rá, hogy a válasz a lentebb megadott formátumban érkezzen!

    !!! note ""
        A lapozhatóság rendkívül fontos tulajdonság lesz, amikor nagyméretű adatbázisokból kérünk le adatokat, hiszen teljes táblák elküldése és feldolgozása sem szerencsés. Ehelyett gyakori megoldás, hogy például 10-esével kéri le a kliens az adatokat és a következő oldalra navigálva kéri csak le a következő 10-et.

        A lapozható eredmények viszont már nem az adott entitásból álló listáiként jönnek le, hanem úgynevezett kollekciókban. Ezekről többet itt olvashatsz: https://chillicream.com/docs/hotchocolate/v14/fetching-data/pagination

        A sorbarendezés dokumentációját pedig ott találod: https://chillicream.com/docs/hotchocolate/v14/fetching-data/sorting

        A válasz formátuma az alábbi legyen: 
    
        ```json
        {
            "data": {
                "orders": {
                    "edges": [
                        {
                            "node": {
                                "id": 5,
                                "orderItems": [
                                    {
                                        "amount": 2,
                                        "product": {
                                        "name": "Lego City harbour",
                                        "price": 172268.75,
                                        "stock": 12
                                        }
                                    },
                                    {
                                        "amount": 1,
                                        "product": {
                                        "name": "Activity playgim",
                                        "price": 7488,
                                        "stock": 21
                                        }
                                    }
                                ]
                            }
                        }
                    ]
                }
            }
        }
        ```

!!! example "BEADANDÓ"
    A módosított C# forráskódot töltsd fel.
    Az 3. feladat első és második részében található lekérdezéseket mentsd le a `q3_1.txt` és `q3_2.txt` néven, tedd a képek mellé a gyökér mappába, és add le.

    Emellett készíts egy képernyőképet, amelyben a vonatkozó lekérdezéseket lefuttattad. A képet `f3.png` néven mentsd el és add be a megoldásod részeként!
