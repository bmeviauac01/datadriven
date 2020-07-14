# Using MongoDB

MongoDB is a free, open-source database server. We are using the _community_ edition and _Robo 3T_ for client.

Download links:

- <https://www.mongodb.com/download-center/community>
- <https://robomongo.org/download>

Installation instructions: <https://docs.mongodb.com/manual/administration/install-community/>

!!! example "Video guide of the tools"
    How to use these tools: <https://youtu.be/5XsmeKmTGII>

## Starting MongoDB server

Depending on the installation model, the server might automatically start. If we opted out of this option, we can start the server with the following command within the installation directory. (Note, that the server application is the mongo&#8203;**d** executable.)

```bash
mongod.exe --dbpath="<workdir>"
```

The database will be stored in directory _workdir_. If started with the command above, the server is alive until the console is closed. We can shut down the server using _Ctrl + C_.

## Mongo shell

The [_Mongo shell_](https://docs.mongodb.com/manual/mongo/) is a simple console client. The official documentation uses this shell in the examples; however, we will not use this app.

## Robo 3T

Robo 3T is simple free client application for accessing MongoDB databases. There are other client applications (e.g. Studio 3T), but the free version is fine for our purpose.

When the app starts, it displays out previous connections, or we can create a new one. By default, the address is `localhost` and the port is `27017`.

![Connection](./images/robo3t-connection.png)

After connection is established, the connection, databases and collections are displayed in a tree-view on the left. To begin with, we will not have any database or collections. (We can create them manually: right lick on the server and _Create Database_. We will not use this though.)

![Collections](./images/robo3t-db-collections.png)

A collection can be opened by double clicking. This will open a new tab, where a search command is executed. This command can be edited and we can issue custom queries too.

The content of the collection (the docments) are listed below. Each document is a separate row. Document can be viewed, edited and deleted by right clicking a document. Edit is performed by editing the raw JSON document.

![Collection content](./images/robo3t-collection-list.png)

A new document can be inserted by right clicking and writing the JSON content. It is advised to copy an existing document and changing it to make sure that key names are correct.
