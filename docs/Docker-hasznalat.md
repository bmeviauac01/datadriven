# Szerver platformok futtatása Docker-ben

A Docker egy konténer virtualizációs megoldás. Abban nyújt számunkra segítséget, hogy telepítés nélkül futtathatóak legyenek az adatbázis szerverek. A Docker használata csak lehetőség, nem szükségszerű. Mindegyik adatbázis platform telepíthető és futtatható "rendesen" is.

## Előfeltételek

Telepítsd a Docker CE (Community Edition) változatát Windows, Linux vagy Mac platformok bármelyikére: <https://store.docker.com/search?type=edition&offering=community>.

Windows platform esetén a Docker-t [Linux konténer módban](https://docs.docker.com/docker-for-windows/#switch-between-windows-and-linux-containers) kell elindítani. Ezt a Docker elindulása után a tálca ikonon lehet beállítani. Akkor fut a Docker a jó módban, ha van "Switch to Windows containers..." menüpont. Ha viszont azt látod, hogy "Switch to Linux containers...", akkor kattints rá.

## MongoDB

Parancssorban add ki az alábbi utasítást egy új MongoDB elindításához.

```bash
docker run -p 27017:27017 -it --rm mongo:4.0
```

Első alkalommal ez a parancs letölti az un. Docker image-et, utána elindul. A konzolt ne zárd be, hagyd futni.

A szerverhez csatlakozni az alábbi beállításokkal lehet:

- cím (address): `localhost` : `27017`

A szervert a konzolban CTRL-C billentyűkombinációval tudod leállítani. Ezzel a konténer meg fog szűnni, ami azt jelenti, az **adatok is törlődnek**. Így legközelebbi indításnál tiszta lappal indulsz.

## Microsoft SQL Server

Parancssorban add ki az alábbi utasítást egy új Microsoft SQL Server elindításához.

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=erHla78!ak3Qa" -e "MSSQL_PID=Express" -p 1433:1433 -it --rm mcr.microsoft.com/mssql/server:2017-latest-ubuntu
```

Első alkalommal ez a parancs letölti az un. Docker image-et, utána elindul. A konzolt ne zárd be, hagyd futni.

A szerverhez csatlakozni az alábbi beállításokkal lehet:

- cím (server name): `localhost\sqlexpress,1433`
- authentication: SQL Server Authentication
- user: `sa`
- jelszó: `erHla78!ak3Qa`

Connection string Entity Framework-höz: `Server=localhost\sqlexpress,1433;Database=adatvez;User id=sa;Password=erHla78!ak3Qa`

A szervert a konzolban CTRL-C billentyűkombinációval tudod leállítani. Ezzel a konténer meg fog szűnni, ami azt jelenti, az **adatok is törlődnek**. Így legközelebbi indításnál tiszta lappal indulsz.

## Lehetséges hibák

- `Bind for 0.0.0.0:1433 failed: port is already allocated.`

  Foglalt a port, amin az adatbázis szerver elérhető lesz. Az indító parancsban a -p utáni részben használj más portot, pl. `-p 1499:1433`. Csak a kettőspont előtti számot cseréld.

- `This program requires a machine with at least 2000 megabytes of memory.`

  Docker-nek nincs elég memória allokálva. A tálcán levő Docker ikonon a Settings-et megnyitva az [Advanced panelen adj több memóriát](https://docs.docker.com/docker-for-windows/#advanced).

  Avagy Windows konténer módban próbálod elnidítani az Oracle Server konténerét. Válts át Linux konténer módba.

- `error during connect: Get http://...: The system cannot find the file specified.`

  Nem fut a 'Docker for Windows Service' nevű szolgáltatás. [Indítsd el](https://success.docker.com/article/docker-for-windows-fails-with-a-daemon-not-running-message) a Windows szolgáltatások között.

## További olvasnivaló

Ha érdekel a Docker, további olvasnivalót találsz itt:

- <https://docs.docker.com/get-started/>
- <https://docker-curriculum.com/>
