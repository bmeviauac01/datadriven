# BMEVIAUAC01 Adatvezérelt rendszerek

![Build docs](https://github.com/bmeviauac01/adatvezerelt/workflows/Build%20docs/badge.svg?branch=master)

[BMEVIAUAC01 Adatvezérelt rendszerek](https://www.aut.bme.hu/Course/VIAUAC01/) tárgy jegyzetei, gyakorlati anyagai, házi feladatai.

A jegyzetek MkDocs segítségével készülnek és GitHub Pages-en kerülnek publikálásra: <https://bmeviauac01.github.io/adatvezerelt/>

#### Helyi gépen történő renderelés (Docker-rel)

1. Powershell konzol nyitása a repository gyökerébe

1. `docker run -it --rm -p 8000:8000 -v ${PWD}:/docs squidfunk/mkdocs-material:7.0.3`

1. <http://localhost:8000> megnyitása böngészőből.

1. Markdown szerkesztése és mentése után automatikusan frissül a weboldal
