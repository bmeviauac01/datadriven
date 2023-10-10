# Using MongoDB

MongoDB is a free, open-source database server. We are using the _community_ edition and _MongoDB for VSCode_ as the client.

Download links:

- <https://www.mongodb.com/download-center/community>
- <https://marketplace.visualstudio.com/items?itemName=mongodb.mongodb-vscode>

Installation instructions: <https://docs.mongodb.com/manual/administration/install-community/>

## Starting MongoDB server

Depending on the installation model, the server might automatically start. If we opted out of this option, we could start the server with the following command within the installation directory. (Note, that the server application is the mongo&#8203;**d** executable.)

```bash
mongod.exe --dbpath="<workdir>"
```

The database will be stored in directory _workdir_. When started with the command above, the server is alive until the console is closed. We can shut down the server using ++ctrl+c++.

!!! tip "Mongo with Docker" 
    Alternatively, you can run the mongo server in a docker container using the following command:

    ```bash
    docker run --name datadriven-mongo -p 27017:27017 -d mongo
    ```

    When running this way, the `-p 27017:27017` switch maps the container's internal port 27017 to localhost's port 27017, so it can be used just like an installed version.

## Mongo shell

The [_Mongo shell_](https://docs.mongodb.com/manual/mongo/) is a simple console client. The official documentation uses this shell in the examples; however, we will not use this app.

## MongoDB for VSCode

MongoDB for VSCode is a simple free extension for VSCode for accessing MongoDB databases.

The extension displays out previous connections, or we can create a new one. By default, the address is `localhost`, and the port is `27017`.

![Connection](images/vscode-connect.png)

![Connection2](images/vscode-connect2.png)

After the connection is established, the databases and collections are displayed in a tree-view on the left. To begin with, we will not have any database or collections. (We can create them manually: right-click on the server and _Create Database_ and run a custom script.)

![Collections](images/vscode-db-collections.png)

A collection can be opened by _right-click / View Documents_. This will open a new tab with results. If we want to search, then with the _right click / Search For Documents..._ operation we can write JavaScript code in a new playground window.

A document can be edited and deleted by right clicking a document. Edit is performed by editing the raw JSON document.

![Collection content](images/vscode-collection-list.png)

A new document can be inserted by right-clicking and writing the JSON content. It is advised to copy an existing document and change it to ensure that key names are correct.

## Studio 3T

[Studio 3T](https://studio3t.com/) is a commercial MongoDB client with richer GUI features. It has a free version but requires registration. It's not available in lab computers, but you can install it on your own computer.
