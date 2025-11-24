# Kis házi feladatok

A házi feladatok **kötelezőek (lásd követelméynek), vizsgapont és iMsc pont** szerezhető velük. A feladatok leírása itt található; a megoldások beadása GitHub Classroom segítségével történik.

!!! important "Működő kód"
    A feladatok során működő kódot, kódrészleteket kell készíteni. A feladat lényege a valóságban működő és a kívánt funkciót ellátó kód készítése.

## A feladatok

1. [MSSQL szerveroldali programozás](mssql/index.md)
1. [Entity Framework](ef/index.md)
1. [MongoDB](mongodb/index.md)
1. [REST API Web API technológiával](rest/index.md)
1. [GraphQL](graphql/index.md)

## A feladatok beadása

Minden házi feladat megoldását egy személyre szóló git repository-ban kell beadni. Ennek pontos [folyamatát lásd itt](GitHub.md). Kérünk, hogy alaposan olvasd végig a leírást!

!!! danger "FONTOS"
    A házi feladatok elkészítése és beadása során az itt leírtak szerint **kell** eljárnod. A nem ilyen formában beadott házi feladatokat nem értékeljük.

    A beadás során a munkafolyamati hibákért (pl. a pull request nem megfelelő emberhez hozzárendelése, a hozzárendelés elfelejtése) pontot vonunk le.

## Képernyőképek

A feladatok gyakran kérik, hogy készíts képernyőképet a megoldás egy-egy részéről, mert ezzel bizonyítod, hogy a megoldásod saját magad készítetted. **A képernyőképek elvárt tartalmát a feladat minden esetben pontosan megnevezi**. A képernyőkép készülhet a teljes desktopról is, de lehet csak a kért alkalmazásról készíteni.

!!! info ""
    A képernyőképeket a megoldás részeként kell beadni, így felkerülnek a git repository tartalmával együtt. Mivel a repository privát, azt az oktatókon kívül más nem látja. Amennyiben olyan tartalom szerepel a képernyőképen, amit nem szeretnél feltölteni, kitakarhatod a képről.

## Szükséges eszközök

- Windows, Linux vagy MacOS: Minden szükséges program platformfüggetlen, vagy van platformfüggetlen alternatívája.
- [GitHub](https://github.com/)-fiók és egy [git](https://git-scm.com/) kliens.

### MSSQL adatbázist használó feladatokhoz
  - Microsoft SQL Server. Az _Express_ változat ingyenesen használható, illetve a Visual Studio mellett feltelepülő _localdb_ változat is megfelelő.
    - [Ubuntu 22.04](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup) alatt is elérhető, vagy
    - [Docker](https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker) segítségével is futtatható Linux vagy MacOS rendszereken.
  - Adatbázis létrehozó script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/datadriven/master/overrides/db/mssql.sql)
  - Fejlesztői eszközök egyike:
    - [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) - Windows platform függő.
    - [Visual Studio Code](https://code.visualstudio.com/download) az [SQL Server (mssql) bővítménnyel](https://marketplace.visualstudio.com/items?itemName=ms-mssql.mssql)
    - [JetBrains DataGrip](https://www.jetbrains.com/datagrip/download/) - Platformfüggetlen, ingyenes non-profit és oktatási célokra.

### MongoDB adatbázist használó feladathoz
  - [MongoDB Community Server](https://www.mongodb.com/download-center/community)
  - Minta adatbázis kódja: [mongo.js](https://raw.githubusercontent.com/bmeviauac01/datadriven/master/overrides/db/mongo.js)
  - Fejlesztői eszközök egyike:
    - [VSCode](https://code.visualstudio.com/download) a [MongoDB for VSCode](https://marketplace.visualstudio.com/items?itemName=mongodb.mongodb-vscode) bővítménnyel
    - [JetBrains DataGrip](https://www.jetbrains.com/datagrip/download/)

### REST API feladatokhoz
  - Fejlesztői eszközök egyike:
    - [Postman](https://www.getpostman.com/)
    - [Hoppscotch](https://docs.hoppscotch.io/) - Nyílt forráskódú, böngészőben is futtatható alternatíva.

### Az első házi kivételével a C# programozós feladatokhoz
  - Fejlesztői eszközök egyike:
    - Microsoft Visual Studio 2022 [az itt található beállításokkal](VisualStudio.md) (Windows rendszereken)
    - [JetBrains Rider](https://www.jetbrains.com/rider/download/) - Windows, MacOS és Linux rendszereken is használható, ingyenes non-profit és oktatási célokra.
    - Visual Studio Code a [C# bővítménnyel](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) és a .NET SDK-val települő [dotnet CLI](https://docs.microsoft.com/en-us/dotnet/tools/)
  - [.NET **8.0** SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

    !!! warning ".NET 8.0"
        A feladat megoldásához **8.0** .NET SDK telepítése szükséges.

        Windowson a Visual Studio telepített verziójától függően előfordulhat, hogy már telepítve van (lásd [itt](VisualStudio.md#net-sdk-ellenorzese-es-telepitese) az ellenőrzés módját); ha nincs, a fenti linkről kell telepíteni az SDK-t (_nem_ a runtime-ot). Linux és MacOS esetén külön szükséges telepíteni.

## A feladatok kiértékelése

A feladatok kiértékelése részben **automatikusan** történik. A futtatható kódokat valóban le fogjuk futtatni, ezért minden esetben fontos a feladatleírások pontos követése (kiinduló kód váz használata, csak a megengedett fájlok változtatása, stb.)!

A kiértékelés eredményéről a GitHub-on kapsz szöveges visszajelzést (lásd [itt](GitHub.md)). Ha ennél több információra van szükséged, a _GitHub Actions_ webes felülete segítségül szolgálhat. Erről [itt](GitHub-Actions.md) találsz egy rövid ismertetőt.

!!! danger "Ellenőrzés"
    Egyes házikban (ahol a technológia ezt lehetővé teszi) találsz unit teszteket. Ezen tesztek **segítenek** ellenőrizni a munkádat, de **nem helyettesítik saját ellenőrzésed**. Amikor feltöltöd a munkádat, alaposabb tesztelésen fog átesni a kódod.
