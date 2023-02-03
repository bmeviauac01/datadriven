# BMEVIAUAC01 Adatvezérelt rendszerek

![Build docs](https://github.com/bmeviauac01/datadriven/workflows/Build%20docs/badge.svg?branch=master)

[BMEVIAUAC01 Adatvezérelt rendszerek](https://www.aut.bme.hu/Course/VIAUAC01/) tárgy jegyzetei, gyakorlati anyagai, házi feladatai.

A jegyzetek MkDocs segítségével készülnek és GitHub Pages-en kerülnek publikálásra: <https://bmeviauac01.github.io/datadriven/>

## MKDocs tesztelése (Docker-rel)

### Helyi gépen

A futtatáshoz Dockerre van szükség, amihez Windows-on a [Docker Desktop](https://www.docker.com/products/docker-desktop/) egy kényelmes választás.

### GitHub Codespaces fejlesztőkörnyezetben

A GitHub Codespaces funkciója jelentős mennyiségű virtuális gép időt ad a felhasználók számára, ahol GitHub repositoryk tartalmát tudjuk egy virtuális gépben fordítani és futtatni.

Ehhez elegendő a repository (akár a forkon) Code gombját lenyitni majd létrehozni egy új codespace-t. Ez lényegében egy böngészős VSCode, ami egy konténerben fut, és az alkalmazás által nyitott portokat egy port forwardinggal el is érhetjük a böngészőnkből.

### Dockerfile elindítása (Helyi gépen van Codespaces-ben)

A repository tartalmaz egy Dockerfile-t, ami at MKDocs keretrendszer és függőségeinek konfigurációját tartalmazza. Ezt a konténert le kell buildelni, majd futtatni, ami lebuildeli az MKDocs doksinkat, és egyben egy fejlesztési idejű webservert is elindít.

1. Terminál nyitása a repository gyökerébe.
2. Adjuk ki ezt a parancsot Windows (PowerShell), Linux és MacOS esetén:

   ```cmd
   docker build -t mkdocs .
   docker run -it --rm -p 8000:8000 -v ${PWD}:/docs mkdocs
   ```

3. <http://localhost:8000> vagy a codespace átirányított címének megnyitása böngészőből.
4. Markdown szerkesztése és mentése után automatikusan frissül a weboldal.


# BMEVIAUAC01 Data-driven systems

![Build docs](https://github.com/bmeviauac01/datadriven/workflows/Build%20docs/badge.svg?branch=master)

[BMEVIAUAC01 Data-driven systems](https://www.aut.bme.hu/Course/ENVIAUAC01/) course lecture notes, seminar materials and homework exercises.

The content in built using MkDocs and is published to GitHub Pages at: <https://bmeviauac01.github.io/datadriven/en/>

## Render website (with Docker)

You need Docker in order to build and run the documentation. On a local machine with Windows [Docker Desktop](https://www.docker.com/products/docker-desktop/) could be the right tooling or you could use any cloud based development environment like GitHub Codespaces.

This repository contains a Dockerfile which need to be built and run.

1. Open a terminal on the repository's root.
2. Run the following commands on Windows (PowerShell), Linux or MacOS:

   ```cmd
   docker build -t mkdocs .
   docker run -it --rm -p 8000:8000 -v ${PWD}:/docs mkdocs --config-file=mkdocs.en.yml
   ```

3. Open <http://localhost:8000> or codespace's port forwarded address in a browser.
4. Edit Markdown files. After saving any file the webpage should refresh automatically.
