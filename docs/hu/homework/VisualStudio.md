# Visual Studio & .NET SDK telepítése

Egyes házikhoz a Microsoft Visual Studio **2022** verziója szükséges. Az ingyenes, [Community változata](https://visualstudio.microsoft.com/vs/community/) is elegendő a feladatok megoldásához.

!!! info "VS Code"
    A feladatok Visual Studio nélkül, **Visual Studio Code**-dal is megoldhatóak. A kiadott kód váz Visual Studio-hoz készült, annak konfigurációit tartalmazza. Ha VS Code-dal dolgozol, magadnak kell konfigurálni a környezetet.

## Visual Studio Workload-ok telepítése

A Visual Studio telepítésekor ki kell pipálni a _ASP.NET and web development_ [workloadot](https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio?view=vs-2022#step-4---choose-workloads).

![Visual Studio 2022 workload](vs-workload.png)

Meglevő telepítés a _Visual Studio Installer_-ben a [_Modify_](https://docs.microsoft.com/en-us/visualstudio/install/modify-visual-studio?view=vs-2022) gombbal módosítható, ill. ellenőrizhető.

## .NET SDK ellenőrzése és telepítése

Visual Studio mellett bizonyos .NET SDK-k telepítésre kerülnek. A megfelelő verzió ellenőrzéséhez legegyszerűbb a `dotnet` CLI-t használni: konzolban add ki a `dotnet --list-sdks` parancsot. Ez a parancs Linux és Mac esetén is működik. A kimenete hasonló lesz:

```hl_lines="2"
C:\>dotnet --list-sdks
8.0.400 [C:\Program Files\dotnet\sdk]
```

Ha ebben a listában látsz **8.0**-ás verziót, akkor jó. Ha nem, akkor telepíteni kell az SDK-t [innen](https://dotnet.microsoft.com/download/dotnet/8.0).
