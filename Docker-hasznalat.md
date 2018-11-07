# Szerver platformok futtatása Docker-ben

A Docker egy konténer virtualizációs megoldás. Abban nyújt számunkra segítséget, hogy telepítés nélkül futtathatóak legyenek a szerver platformok.

## Előfeltételek

Telepítsd a Docker CE (Community Edition) változatát Windows, Linux vagy Mac platformok bármelyikére: <https://store.docker.com/search?type=edition&offering=community>.

Windows platform esetén a Docker-t [Linux konténer módban](https://docs.docker.com/docker-for-windows/#switch-between-windows-and-linux-containers) kell elindítani. Ezt a Docker elindulása után a tálca ikonon lehet beállítani. Akkor fut a Docker a jó módban, ha van "Switch to Windows containers..." menüpont. Ha viszont azt látod, hogy "Switch to Linux containers...", akkor kattints rá.

## Oracle Server

Parancssorban add ki az alábbi utasítást egy új Oracle Server elindításához.

```
docker run -it --rm --shm-size=1g -p 1521:1521 bmeviauac01/adatvez-oracle
```

Első alkalommal ez a parancs letölti az un. Docker image-et. Utána elindul, ez pár másodpercet fog igénybe venni. Konzolon meg fog jelenni a "DATABASE IS READY TO USE" szöveg. A konzolt ne zárd be, hagyd futni.

A szerverhez csatlakozni az alábbi beállításokkal lehet:

* cím (host): localhost
* port: 1521
* SID: xe
* user: adatvez
* jelszó: erHla78ak3Qa

A szervert a konzolban CTRL-C billentyűkombinációval tudod leállítani. Ezzel a konténer meg fog szűnni, ami azt jelenti, az **adatok is törlődnek**. Így legközelebbi indításnál tiszta lappal indulsz.

## Microsoft SQL Server

Parancssorban add ki az alábbi utasítást egy új Microsoft SQL Server elindításához.

```
docker run -it --rm -p 1433:1433 bmeviauac01/adatvez-mssql
```

Első alkalommal ez a parancs letölti az un. Docker image-et. Utána elindul, ez pár másodpercet fog igénybe venni. A konzolt ne zárd be, hagyd futni.

A szerverhez csatlakozni az alábbi beállításokkal lehet:

* cím (server name): localhost\sqlexpress,1433
* authentication: SQL Server Authentication
* user: sa
* jelszó: erHla78!ak3Qa

Connection string Entity Framework-höz: `Server=localhost\sqlexpress,1433;Database=adatvez;User id=sa;Password=erHla78!ak3Qa`

A szervert a konzolban CTRL-C billentyűkombinációval tudod leállítani. Ezzel a konténer meg fog szűnni, ami azt jelenti, az **adatok is törlődnek**. Így legközelebbi indításnál tiszta lappal indulsz.

## Lehetséges hibák

* `Bind for 0.0.0.0:1521 failed: port is already allocated.`

    Foglalt a port, amin az adatbázis szerver elérhető lesz. Az indító parancsban a -p utáni részben használj más portot, pl. `-p 1599:1521`. Csak a kettőspont előtti számot cseréld.

* `This program requires a machine with at least 2000 megabytes of memory.`

    Docker-nek nincs elég memória allokálva. A tálcán levő Docker ikonon a Settings-et megnyitva az [Advanced panelen adj több memóriát](https://docs.docker.com/docker-for-windows/#advanced).

    Avagy Windows konténer módban próbálod elnidítani az Oracle Server konténerét. Válts át Linux konténer módba.

* `WARNING: You are trying to use the MEMORY_TARGET feature.`

    Windows konténer módban próbálod elnidítani az Oracle Server konténerét. Válts át Linux konténer módba.

* `error during connect: Get http://...: The system cannot find the file specified.`

    Nem fut a 'Docker for Windows Service' nevű szolgáltatás. [Indítsd el](https://success.docker.com/article/docker-for-windows-fails-with-a-daemon-not-running-message) a Windows szolgáltatások között.

## További olvasnivaló

Ha érdekel a Docker, további olvasnivalót találsz itt:

* <https://docs.docker.com/get-started/>
* <https://docker-curriculum.com/>