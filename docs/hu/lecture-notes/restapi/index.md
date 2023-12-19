# REST API (Gyakorló példák)

!!! info "Érdekesség"
    A példák többsége egy létező, közismert tanulmányi rendszer működésén alapszik.

## Elméleti kérdések

1. Minek a rövidítése a REST?

2. Milyen egységekben, milyen műveletekben kell gondolkoznunk, amikor REST API-t tervezünk?

3. Foglald össze, hogy a REST API szerint az egyes HTTP igéket milyen típusú műveletekhez használjuk!

??? example "Megoldás: 1. feladat"
    Representational State Transfer

??? example "Megoldás: 2. feladat"
    Ebben az architektúra stílusban az alapvető egység az **erőforrás**, ami tipikusan valamilyen adatentitásnak feleltethető meg (pl. Task, Product, Order, stb.).
    Műveleteket tekintve egy-egy kérés-válasz páros valamilyen erőforrásra vonatkozó **állapotátvitel** (State Transfer). Az átvitel irányát a HTTP ige határozza meg.

??? example "Megoldás: 3. feladat"
    ![Részlet az előadásból](images/rest-http-verbs-hu.png)


## Gyakorlati kérdések

1. Egy tanulmányi rendszerből szeretnénk lekérdezni egy adott hétre vonatkozó eseményeket. A böngésző fejlesztői eszközeivel azt tapasztaljuk, hogy ebben az esetben az alábbi kérést küldjük el a szervernek:
```
Request URL:        https://domain.hu/hallgatoi/TimeTableHandler.ashx
Request Method:     POST
Status Code:        200 OK
Remote Address:     152.66.28.54:443
Referrer Policy:    strict-origin-when-cross-origin
```
Az előző feladatot tekintve miben sérti ez a megoldás a REST API iránymutatásait?

2. Tanulmányi rendszerünket frissíteni szeretnénk, hogy valóban REST API-t implementáljon. Hogyan írnád át a következő lekérdezést, ha...  
a) specifikációba van rögzítve, hogy mindig pontosan egy naptári hetet szeretnénk lekérdezni és később sem szeretnénk ezen változtatni?  
b) a lekérdezendő időtartam eleje és vége is tetszőlegesen választható marad?  
```
Request URL:        https://domain.hu/hallgatoi/timetable
Request Method:     POST
```
``` json
Request Body: {
    startDate: 1702281600000,
    endDate: 1702857599000,
    showClasses: true,
    showExams: true,
    showTasks: false
}
```

??? example "Megoldás: 1. feladat"
    A `Request Method` mezőben azt látjuk, hogy `POST` metódust használtunk *lekérdezéshez*. A REST API szerint lekérdezéshez `GET` használandó.

??? example "Megoldás: 2. feladat"
    A megoldás során egy dologban biztosak lehetünk: `Request Method: GET`, hiszen lekérdezni szeretnénk adatot.  

    **a) feladat**  
    Innentől kezdve azonban már több irányban is elindulhatunk. Legegyszerűbb megoldás, ha egyszerűen minden paramétert, ami eddig a `Request Body`-ban utazott, beírjuk az URL-be, mint paraméter. Ekkor a megoldásunk így nézne ki:
    ```
    Request URL:        https://domain.hu/hallgatoi/timetable?startDate=1702281600000&endDate=1702857599000&showClasses=true&showExams=true&showTasks=false
    Request Method:     GET
    ```
    Ez az a pillanat, ahol sokan hátradőlnének, esetleg a vállukat is megveregetnék, hogy megoldották a feladatot. Azonban gondolkodjunk el egy picit... Mi is a REST API alapgondolata? Erőforrásokat kezelünk. Ahhoz, hogy a megoldásunk valóban megfeleljen ennek a szemléletnek, szerver oldalon sajnos nagyobb változtatás szükséges, mint az előző megoldásnál, de mi most API-t tervezünk, emiatt ne fájjon a fejünk.  
    Na de milyen erőforrás szerepel ebben a lekérdezésben? Mit szeretnénk lekérdezni? *Egy hét* eseményeit. Az események a válaszban fognak szerepelni, mint objektumok, ezért azokat nem tudjuk kiemelni, de miért ne emelhetnénk ki a heteket?
    ```
    https://domain.hu/hallgatoi/timetable/week/51?showClasses=true&showExams=true&showTasks=false
    ```
    Így már mindjárt olvashatóbb az URL. Egy részletről azonban megfeledkeztünk: Melyik évre vagyunk kíváncsiak? Gyorsan javítsuk is ki:
    ```
    https://domain.hu/hallgatoi/timetable/year/2023/week/51?showClasses=true&showExams=true&showTasks=false
    ```
    Még egy utolsó gondolat motoszkálhat bennünk: Nem lenne szebb azt, hogy milyen eseményekre vagyunk kíváncsiak egy listában átadni? Próbáljuk ki!
    A végleges lekérdezésünk, tehát a következő:
    ```
    Request URL:     https://domain.hu/hallgatoi/timetable/year/2023/week/51?showEvents=classes,exams
    Request Method:  GET
    ```  
    
    **b) feladat**  
    Ebben az esetben nem tudjuk az előző kérdéshez hasonlóan kezelni a lekérdezendő intervallumot, itt kénytelenek leszünk az intervallum kezdetét és végét is paraméterként átadni. A megjelenítendő események típusát azonban itt is átadhatjuk listaként, így tehát egy lehetséges megoldás a következő:
    ```
    Request URL:     https://domain.hu/hallgatoi/timetable?startDate=1702281600000&endDate=1702857599000&showEvents=classes,exams
    Request Method:  GET
    ```
