# 4. Java Persistence API, Spring

A házi feladat teljesítésével **4 pont és 3 iMsc pont** szerezhető.

A tanszék AHK rendszerének segítségével hozz létre magadnak egy repository-t. A **meghívó URL-t Moodle-ben találod**. Klónozd le az így elkészült repository-t. Ez tartalmazni fogja a megoldás elvárt szerkezetét. Hozz létre egy `megoldas` nevű branchet, és **arra dolgozz**. A feladatok elkészítése után kommitold és pushold a megoldásod.

## A kiinduló kód áttekintése

Importáld be tetszőleges Java-s IDE-be a repositoryban található Maven alapú projektet, amelyhez 25-ös JDK szükséges. A feladat egy elképzelt logisztikai alkalmazás adatelérési és üzleti logikai rétegének kibővítése. A logisztikai cég szállítás terveket (TransportPlan) készít, amelyek azt foglalják össze, hogy egy adott szállítmányt milyen szakaszokon (Section) keresztül, milyen mérföldköveket (Milestone) érintve terveznek kiszállítani. Az adatmodell entitásainak rövid leírása: 

- **TransportPlan**: egy szállítási tervet reprezentál, szakaszokat (Section) tartalmaz. Egyedi azonosítóval rendelkezik, és tárolja a kapcsolódó megbízások azonosítóit. 

- **Section**: egy szállítmány egy szakaszát reprezentálja, start- és végmérföldkő tartozik hozzá (fromMilestone, toMilestone). A number mező mutatja meg, hogy ő hányadik szakasz a szállítási tervben. (0-tól számozódva.) 

- **Milestone**: egy mérföldkő a szállítás során. Hivatkozik egy címre (Address) és tartalmazza, hogy a terv szerint mikor kell elérni az adott mérföldkövet (plannedTime). Az időpont mindig helyi idő, az időzónát nem kell eltárolni. 

- **Address**: egy cím adatait (ország 2 betűs ISO kódja, város, utca, irányítószám, házszám, szélesség, hosszúság fokokban) tárolja.

A kiinduló projekt teszteket is tartalmaz. A tesztek jellegzetessége, hogy egyáltalán nem szükséges hozzá adatbázist beállítani, mert egy beágyazott in-memory H2 adatbázist használnak. A feladatok megoldása és tesztelése így perzisztens adatbázis nélkül is megoldható. Ha mégis szeretnéd az alkalmazást (LogisticsApplication) önmagában, tesztesetektől függetlenül futtatni, és mögötte egy perzisztens adatbázis tartalmát megtekinteni, akkor az application.properties-ben kell beállítanod a DB elérését, és a pom.xml-ben felvenni a JDBC driver függőségét. A kiinduló projektben az MSSQL driver függősége már benne van, és az application.properties-ben is MSSQL-es példa JDBC URL található. De ez nem lesz hatással a tesztekre, azok mindenképpen az in-memory H2 adatbázissal fognak dolgozni.



## Feladat 0: Neptun kód

Első lépésként a gyökérben található `neptun.txt` fájlba írd bele a Neptun kódodat!

## Feladat 1: Új lekérdezések (2 pont)

Bővítsd az AddressRepository interfészt az alábbi metódusokkal: 

a. A metódussal adott városban lévő címek kérdezhetők le. A városban kis/nagybetű ne számítson, de ezt leszámítva pontos egyezés szükséges

b. A metódus egy adott "téglalapon" belül eső címeket adja vissza, ha megadjuk a téglalap bal felső és jobb alsó sarkainak szélesség/hosszúság koordinátáit.

c. metódus utca átnevezéseket képes kezelni: paraméterként kapott országkód/irányítószám/utcanév hármasra pontosan illeszkedő címekben nevezi át az utcát a szintén paraméterként kapott új névre

A lekérdezések tesztelésére az F1_AddressRepositoryIT (IT = Integration Test) osztály teszt metódusait kell futtatnod. A teszt osztály használatához az elején ki kell töltened a TODO-kat tartalmazó metódusokat oly módon, hogy az általad írt repository metódusokat meghívod megfelelő paraméterezéssel. Mást azonban ne módosíts a teszt osztályban.

!!! example "BEADANDÓ"
    A módosított forráskódot töltsd fel.



## Feladat 2: Üzleti logikai réteg bővítése (2 pont)

Valósítsd meg a **TransportPlanService** osztály alábbi metódusait!

a. **getFirstAndLastMilestone**: A metódus adjon vissza egy kételemű Milestone listát, amelyben az adott id-jű szállítási terv legelső és legutolsó mérföldköve található. Ha a tervben még nincs szakasz, akkor üres listát adjon vissza! Ha az adott id-hez nem létezik terv, dobjon IllegalArgumentException-t! 

b. **registerDelay**: a metódus egy adott terv adott mérföldkövénél regisztrál egy percekben megadott várható késést. 
   * Nem létező szállítási terv, vagy mérföldkő esetén kivételt kell dobni (IllegalArgumentException-t)
   
   * Növeld meg az adott mérföldkőnél tervezett időpontot a késés hosszával!
   
   * Ha a mérföldkő a kezdő mérföldkő a szakaszon belül, akkor annak a szakasznak a végmérföldkövénél tervezett időt is növeld meg a késés hosszával!
   
   * Ha a mérföldkő egy szakaszon belüli végmérföldkő, akkor a következő szakasz kezdő mérföldkövének tervezett idejét növeld a késés hosszával!
   

Mindkét metódushoz készen állnak tesztesetek az F2a_TransportPlanServiceGetFirstAndLastMilestoneIT, F2b_TransportPlanServiceRegisterDelayIT osztályokban. Ezeket nem szabad módosítanod.

## Feladat 3: Új szakasz hozzáadása (3 iMSc pont)

!!! note ""
    A pont megszerzésére az első két feladat megoldásával együtt van lehetőség.

Valósítsd meg a TransportPlanService **addSection** metódusát. Ezzel egy létező szállítási terv szakaszai közé tudunk egy új szakaszt beszúrni, adott sorszámmal (number), két adott id-jű milestone között. A következő szabályokat kell betartani: 

   * Ha nem létezik a szállítási terv, vagy a mérföldkövek közül bármelyik, kivételt kell dobni. (IllegalArgumentException-t)
   * Az új szakasz sorszáma 0 és MAX között lehet inkluzív, ahol MAX a tervhez tartozó szakaszok beszúrás előtti darabszáma. Ha ez nem teljesül, dobódjon IllegalArgumentException.
   * A szakasz beszúrása történhet a meglévő szakaszok elé, után, és közé is. Minden esetben meg kell tartani a szakaszok folyamatos sorszámozását. Pl. ha eddig 0, 1, 2-es szakaszok voltak, és az 1-es numberrel szúrunk be, akkor a korábbi 1-es, 2-es numberű szakasz új sorszáma 2-es és 3-as legyen. (Shifteljük a későbbi szakaszokat.) 
   * Az új szakaszt közvetlenül megelőző vagy követő szakasz (ha léteznek ilyenek) mérföldköveinél ellenőrizni kell a címeket: A megelőző szakasz végmérföldkövénél lévő cím és az új szakasz kezdőmérföldkövénél lévő cím közötti távolság 500 méteren belül kell legyen. A következő szakasz startmérföldkövéhez és az új szakasz végmérföldkövéhez tartozó címek között szintén legfeljebb 500 méter lehet a távolság.  A távolságot a szélesség/hosszúság koordinátákból a Haversine formulával számítsd ki. 
   * Az új szakasz végmérföldkövéhez tartozó tervezett időpont nem lehet korábbi a startmérföldkőhöz tartozó tervezett időpontnál.
   * Ha az új szakasz előtt van már meglévő szakasz, akkor ezen megelőző szakasz végmérföldkövéhez tartozó tervezett időpont nem lehet az új szakasz startmérföldkövének tervezett időpontja után. Ellenkező esetben dobódjon IllegalArgumentException.
   * Ha az új szakasz után van már meglévő szakasz, akkor ezen következő szakasz startmérföldkövéhez tartozó tervezett időpont nem lehet az új szakasz végmérföldkövének tervezett időpontja előtt. Ellenkező esetben dobódjon IllegalArgumentException.
   * Ha a fenti feltételek teljesülnek, mindenképpen új szakaszt kell létrehozni, akkor is, ha a megadott milestone-ok között esetleg már létezik egy másik szakasz. 

A metódushoz készen állnak tesztesetek az F3_TransportPlanServiceAddSectionIT osztályban. Ezeket nem szabad módosítanod.

!!! example "BEADANDÓ"
    A módosított forráskódot töltsd fel.

