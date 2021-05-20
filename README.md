# BMEVIAUAC01 Data-driven systems

![Build docs](https://github.com/bmeviauac01/datadriven-en/workflows/Build%20docs/badge.svg?branch=master)

[BMEVIAUAC01 Data-driven systems](https://www.aut.bme.hu/Course/ENVIAUAC01/) course lecture notes, seminar materials and homework exercises.

The content in built using MkDocs and is published to GitHub Pages at: <https://bmeviauac01.github.io/datadriven-en/>

#### How to build it locally (using Docker)

1. Open a Powershell console to the root of the directory

1. `docker run -it --rm -p 8000:8000 -v ${PWD}:/docs squidfunk/mkdocs-material:7.1.5`

1. Open <http://localhost:8000> in a browser

1. Edit the Markdown source and save; build and webpage refresh will be automatically triggered
