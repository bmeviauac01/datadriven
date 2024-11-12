# 5. GraphQL

GraphQL házi feladat, a teljesítéssel **4 pont és 3 iMsc pont** szerezhető.

GitHub Classroom segítségével hozz létre magadnak egy repository-t. A **meghívó URL-t Moodle-ben találod**. Klónozd le az így elkészült repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. A feladatok elkészítése után kommitold és pushold a megoldásod.

A megoldáshoz szükséges szoftvereket és eszközöket lásd [itt](../index.md#szukseges-eszkozok).

Előkészületként hozz létre egy új adatbázist, a [gyakorlatanyagban](../../seminar/mongodb/index.md) leírt módon.

## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Projekt kiegészítése és lekérdezések (2 pont)

A házi feladat feladat célja, hogy gyakorlati tapasztalatot szerezzetek a GraphQL használatában egy saját API létrehozásával. A GraphQL egy erőteljes eszköz, amely lehetővé teszi az adatok lekérdezését és módosítását egyetlen, jól strukturált kérésben, rugalmasabbá és hatékonyabbá téve az API-k használatát.
A gyakorlat célja, hogy megmutassa, hogyan érhetők el komplex adatok egyszerűen.

Ebben a házi feladatban a megszokott adatmodell lesz használva, termékek, kategóriák és hozzájuk kapcsolódó információk lekérdezésével.
Az adatmodell, a DBContext és az entitások már megtalálhatóak a kiinduló projektben.

1. Vegyél fel egy `Query` osztályt, ami tegyél egy `GetProducts` függvényt az alábbi paraméterezéssel, ami garantálja a beregisztrált DBContext injektálását:

```csharp
public IEnumerable<Product> GetProducts([Service] AdatvezDbContext dbContext)
```

1. Implementáld a fenti metódust, hogy visszaadja a termékeket és a hozzájuk tartozó kategóriákat.

1. Add hozzá a `ConfigureServices` metódusba a GraphQLServer szolgáltatást az alábbi kód segítségével.

```csharp
services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddFiltering();
```

1. Add hozzá a `Configure` metódusba az útválasztást (végpontok kezelésének képességét) és hozz létre egy alapértelmezett végpontot a következő kódsorok segítségével: 

```csharp
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL();
});
```

1. indítsd el az alkalmazást és navigálj a `http://localhost:5000/graphql/` oldalra. Itt láthatod a Banana Cake Pop nevű interaktív eszközt, amivel lehetséges lekérdezések és mutációk futtatás, a séma megtekintése, valamint az API-hoz tartozó dokumentációt is lehet böngészni. A lekérdezések tabon a következő kóddal ki tudod próbálni a fenti függvény futását:

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

1. Készíts az előző feladat alapján egy olyan lekérdezést, amely képes a termékeket visszaadni valamilyen szűrés elvégzése után. Jelen esetben szűrj a kategória nevére: azokat a terméket add vissza, amelyek kategóriájának neve megegyezik a kapott paraméterrel. A teszteléshez használd az alábbi lekérdezést:

    ```json
    query {
        productsByCategory(categoryName: "Months 0-6") {
            id
            name
            price
        }
    }
    ```


1. A következő feladatban láthatjuk a GraphQL erejét. Amennyiben sok táblából kell összeválogatni a szükséges információt, sok felesleges adat is keresztül mehetne a hálózaton. Ehhez képest, ha tudjuk mik fognak kelleni, elég csupán a szükséges adatokat lekérdezni. Készíts egy lekérdezést, ami a következő adatokat szedi össze a megrendelésekből:

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

## Feladat 2: Módosítások GrapQL segítségével  (2 pont)

Természetesen a GraphQL nem csak adatok lekérdezésére, hanem módosításokra is használható. A következő feladatokban a cél, hogy adatmanipulációkat gyakoroljunk.
Az adatmanipulációt úgynevezett _Mutation_ hajtja végre, ami egy olyan művelet, amely adatokat módosít, például rekordokat hoz létre, frissít, vagy töröl az adatbázisban.

### Termék módosítása

Az első módosítás a meglévő termékek árát fogja módosítani. Növeljük meg a Amennyiben a paraméterben kapott kategóriába tartoznak a termékem

1. Vegyél fel egy Mutation osztályt `ProductMutation` néven.

1. Regisztráld be a szolgáltatást a `.AddMutationType<ProductMutation>()` függvényhívás segítségével.

1. A `ProductMutation` rendelkezzen egy `IncreaseProductPricesByCategoryAsync` függvénnyel, ami elvégzi a szükséges módosításokat. A függvény három paraméterrel rendelkezzen: 1) az `AdatvezDBContext` az 1-es feladatban lévő annotációkkal, 2) egy kategória név stringgel, és 3) egy double értékkel, ami megmondja, hogy mennyivel dráguljon a termék.

1. Tesztelni az alábbi példa paranccsal tudod:

```json
mutation {
    increaseProductPricesByCategory(categoryName: "LEGO", priceIncrease: 5.0) {
        id
        name
        price
        category {
                name
            }
        }
}
```

### Megrendelés beszúrása

A következő feladatban egy új megrendelés beszúrására szeretnénk lehetőséget biztosítani.
A megrendelés csak a termékek neveit és a kívánt darabszámait fogja majd várni paraméterben és az alapján hozza létre a megrendelést.

1. Vegyél fel egy függvényt a `ProductMutation` osztályba `ProductMutation` néven. A szokásos `AdatvezDbContext`-en kívül legyen két listát váró paramétere: az első egy string listát a terméknevekkel, a második pedig egy int listát vár a darabszámokkal.

1. A függvénybe vegyél fel egy új `Order` példányt, amiben minden egyes paraméterben kapott terméknévhez egy új `OrderItem` kerüljön be. Az `OrderItem`-hez pedig keresd meg a név alapján és állítsd be a megfelelő `Product`-ot és darabszámot (a többi értéket figyelmen kívül hagyhatod).

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

# Feladat 3: Haladó GraphQL funkciók (3 iMSc pont)

A következő feladatokban a GraphQL haladóbb funkcióit használjuk ki, úgy mint a szűrést, rendezést, lapozhatóságot.

1. A Hot Chocolate rendelkezik beépítve olyan a szűrés, a rendezés, és a lapozhatóság funkcióval. A `GetOrders` függvényt annotáld a megfelelő attribútumokkal, majd a GraphQL regisztrálásakor állítsd be a szűrés és rendezés lehetőségeit. A következő lekérdezéssel kipróbálhatod működés közben őket, anélkül, hogy a `GetOrders` függvény implementációját változtatni kellene:

```json
query {
  orders(
    where: { 
      orderItems: { 
        product: { price: { gte: 100 }, stock: { gte: 10 } }
      }
    }
    first: 5    # Limit the results to the first 5 orders
    orderBy: price_ASC  # Sort by price in ascending order
  ) {
    id
    orderItems {
      quantity
      product {
        name
        price
        stock
      }
    }
  }
}
```

1. Előfordul, hogy szeretnénk komplexebb vagy egyedi logikát készíteni a GraphQL lekérdezéshez. Készíts egyedi szűrésre alkalmas osztályt, ami a `FilterInputType` leszármazottja és arra lesz képes, hogy lehet szűrni vele arra, hogy van-e raktáron (nagyobb-e a raktárkészlet, mint 0).

```json
query {
  orders(filter: { orderItems: { product: { inStock: false } } }) {
    id
    orderItems {
      quantity
      product {
        name
        stock
      }
    }
  }
}
```