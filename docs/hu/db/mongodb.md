# MongoDB használata

A MongoDB ingyenes, open-source adatbázis kiszolgáló. Mi az ún. _community_ változatát használjuk, kliens szoftvernek pedig a VSCode MongoDB for VSCode kiegészítőjét.

Letöltési linkek:

- <https://www.mongodb.com/download-center/community>
- <https://marketplace.visualstudio.com/items?itemName=mongodb.mongodb-vscode>

Telepítési útmutató: <https://docs.mongodb.com/manual/administration/install-community/>

## MongoDB szerver elindítása

A telepítési modell függvényében lehet, hogy a MongoDB szerver automatikusan elindul. Ha nem kértük ezt a telepítéskor, akkor a telepítési könyvtárban az alábbi paranccsal tudjuk elindítani a szervert. (Ügyeljünk rá, hogy a szerver a mongo&#8203;**d** exe.)

```bash
mongod.exe --dbpath="<munkakönyvtár>"
```

A _munkakönyvtárban_ fog tárolódni az adatbázis. Ha ilyen módon, konzolból indítottuk a szervert, akkor addig fut, amíg a konzolt be nem zárjuk. Leállítani a ++ctrl+c++ billentyűkombinációval kell.

!!! tip "Mongo Dockerrel"
    Alternatívaként futtathatjuk a mongo szervert docker konténer formájában az alábbi paranccsal:

    ```bash
    docker run --name datadriven-mongo -p 27017:27017 -d mongo
    ```

    Így futtatva a `-p 27017:27017` kapcsoló leképzi a konténer belső 27017-es portját a localhost 27017-es portjára, így ugyanúgy használható mint egy telepített verzió.

## Mongo shell

A [_Mongo shell_](https://docs.mongodb.com/manual/mongo/) egy egyszerű konzolos kliens alkalmazás. A hivatalos dokumentációban szereplő példák általában ezt használják. Mi nem fogjuk ezt a programot használni.



## MongoDB for VSCode

A MongoDB for VSCode egy egyszerű és ingyenes kiegészítő VSCode-ban MongoDB adatbázis használatához.

A kiegészítő megnyitásakor kiválaszthatjuk a már korábban létrehozott kapcsolatunkat, vagy készíthetünk egy újat. Alapértelmezésként a helyben futó szervert a `localhost` címen és a `27017` porton érhetjük el.

![Kapcsolódás](images/vscode-connect.png)

![Kapcsolódás2](images/vscode-connect2.png)

A sikeres kapcsolódás után az kiegészítő bal oldalán a faszerkezetben látjuk a kapcsolódott kiszolgálót, az adatbázisokat és a gyűjteményeket. Kezdetben se adatbázisunk, se gyűjteményeink nem lesznek. (Ezeket létrehozhatjuk kézzel is: jobb egérrel kattintva a szerver nevén találjuk például a _Create Database_ parancsot. Mi azonban ezt nem használjuk.)

![Gyűjtemények](images/vscode-db-collections.png)

A gyűjtemények tartalmát _jobb gomb / View Documents_művelettel tekinthetjük meg, amit egy új tab fület nyit. Ha keresni szeretnénk, akkor a _jobb gomb / Search For Documents..._ művelettel írhatunk JavaScript kódot egy playground ablakban.

A dokumentumot törölni, szerkeszteni a rekordra való jobb egér kattintással tudjuk. A szerkesztés során a JSON dokumentumot szerkesztjük.

![Gyűjtemény tartalma](images/vscode-collection-list.png)

Új dokumentumot beszúrni szintén jobb egérrel kattintva tudunk. Itt egy üres szerkesztőt kapunk. Ha új rekordot akarunk létrehozni, célszerű egy meglevő dokumentum JSON-jét lemásolni és úgy hozni létre az újat, hogy a kulcsok nevei biztosan jók legyenek.
