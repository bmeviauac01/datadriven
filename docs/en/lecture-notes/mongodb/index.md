# MongoDB basics, operations, and the MongoDB .NET Driver

## NoSQL databases

NoSQL databases are data management systems that do not work with the relational data model. The NoSQL name can be a little misleading, as the concept has little to do with the SQL language - the main difference is rather the representation of the data. Why do we need such new databases when we already have the relational model and relational databases? A small database with a simple schema can be easily described in the relational model. But our applications evolve: new functionalities are added, making the schema more complex. More and more data is added to the database making the maintenance inefficient.

Relational databases need constant schema changes and updates, which can be cumbersome. Constant data migrations can be a pain. Furthermore, relational databases can be bottlenecks from the scalability perspective - but we will not discuss this aspect in more detail.

NoSQL databases offer a solution to these problems. **Instead of the rigid schema**, NoSQL databases have **a flexible schema**. In other words, we will require less consistency regarding the data.

## Basic concepts of MongoDB

MongoDB is a client-server database system that has a non-relational schema. The _mongod_ (Mongo daemon) process on the right is the database server. The other side is our application, where a client connects to the database using a network connection. This connection uses the so-called _wire protocol_, which is a MongoDB proprietary communication protocol. The protocol transmits data and queries in JSON format represented in a binary fashion as BSON.

![MongoDb architecture](images/mongodb_architecture.png)

### Logical structure

The top layer of a MongoDB database system is the so-called _cluster_. The servers are organized into these clusters. We will not discuss clusters here; these are tools for enabling scalability. The databases are the _mongod_ processes, which host the databases. A MongoDB server/cluster stores multiple databases. And the databases contain _collections_. If we want to map these concepts to a relational model, then the collections correspond to the tables, and the rows/records in a table correspond to the _documents_ of the collection.

Let us investigate these further.

#### Document

The document is the unit of storage in MongoDB. A document is a JSON (-like) file: it contains key-value pairs. MongoDB itself stores it as a BSON in binary format.

```csharp
{
    name: "sue",
    age: 26,
    status: "A",
    groups: [ "news", "sports"]
}
```

The keys can have arbitrary names with a few limitations, such that they have to be unique and cannot begin with the `$` character. The names are case sensitive. Values can be string, number, date, binary, embedded document, `null`, or even as the `groups` in the example shows, an array - a relational database cannot represent an array in such a simple way.

Mapping to the object-oriented world, a document is an object. MongoDB documents have a maximum size of 16MB and this is not a configurable parameter.

#### Collection

Collections are analogous to relational database tables, but without a schema. Collections need no definition; the system creates them upon first use. Collections are a place for "similar" documents. Although there is no schema, _indexes_ can still be defined on the collections to support fast searching. Since there is no schema, there are no domain integrity requirements enforced either.

#### Database

The database has the same purpose as in the relational model. It gathers all data of our application. Access management is also configured on the database level. The name of databases is case sensitive and lowercase by convention.

#### Key

The `_id` field is the unambiguous identifier of each document. Other keys cannot be defined. This field does not need to be specified during insert; the client driver or the server can generate a new value (a 12 byte `ObjectId` by default).

Uniqueness can be guaranteed with the use of indices. We can define an index and mark it as unique to create a key-like field. These unique indices can contain multiple fields too.

There are no references to keys in MongoDB. A document can reference another document by copying it's key, but the system has no consistency guarantees for these (e.g., the referenced document can be deleted).

## MongoDB operations and the MongoDB .NET Driver

!!! note ""
    The following code snippets use the official Nuget package [MongoDB.Driver](https://www.nuget.org/packages/mongodb.driver).


## Establishing a connection

To access the MongoDB database, you first need a connection. A `MongoClient` class represents the connection. We need the server address to establish a connection (see <https://docs.mongodb.com/manual/reference/connection-string/> for details on the connection string).

```csharp
var client = new MongoClient("mongodb://localhost:27017");
```

The connection should be treated as a singleton and not disposed of.

!!! info "Connection lifetime"
    The connection is typically stored in a global static variable, or an IoC (Inversion of Control) / DI (Dependency Injection) store.

Although the database name may be in the connection string (e.g. `mongodb://localhost:27017/datadriven`), it is used only for authentication. Thus, after establishing the connection, we need to specify what database we will use.

```csharp
var db = client.GetDatabase("datadriven");
```

The database does not need to exist in advance. The above call will automatically create if the database does not already exist.

## Managing collections

Unlike a relational database, **in MongoDB, our operations are always performed on a single collection**, so the selection of a collection is not part of the issued command (as in the `from` in SQL), but a prerequisite for the operation. You can get a specific collection by calling `GetCollection`; its generic parameter is the C# class implementing the document type.

```csharp
var collection = db.GetCollection<BsonDocument>("products");
```

The basic concept of the .NET MongoDB driver is to map every document to a .NET object. This is also called _ODM (Object Document Mapping)_. ODM is the equivalent of ORM in the NoSQL database world.

!!! warning ""Raw" json"
    In other languages and platforms, MongoDB drivers do not always map to objects. Sample codes found on the Internet often show communication via "raw" JSON documents. Let's try to avoid this, as we learned in ORM, that object-oriented mapping is more convenient and secure.

In the previous example, a document of the type "BsonDocument" is used. `BsonDocument` is a generic document representation in which we can store key-value pairs. It is uncomfortable and unsafe to use; thus we usually do try to avoid it. See the suggested solution soon.

You can run queries on the variable representing the collection, such as inserting a document and then listing the contents of the collection. The collection will be created automatically the first time you use it, so you don't have to define it.

```csharp
collection.InsertOne(new BsonDocument()
{
    { "name", "Apple" },
    { "categoryName", "Apple" },
    { "price", 123 }
});

// listing all documents: a search criteria is needed
// this is an empty criteria matching all documents
var list = collection.Find(new BsonDocument()).ToList();
foreach(var l in list)
    Console.WriteLine(l);
```

!!! important "Naming convention"
    Field names in the document start with lowercase letters like `price` or `categoryName` (this is the so-called _camel case_ spelling). This is a **convention** of the MongoDB world for historical reasons. Unless there is a good reason, do not deviate from it.

## Mapping documents to C# objects

As with relational databases, we can work with objects and classes in MongoDB. The .NET driver for MongoDB offers this conveniently.

The first step is to define the C# class(es) to map the contents of the database. Since there is no schema for the database and table, we cannot generate C# code based on the schema (as we did with the Entity Framework). So in this world, we tend to follow the _Code First_ approach, which is to write C# code and have the system translate it to database collections.

Let us define the following classes to represent _Products_.

```csharp
public class Product
{
    public ObjectId Id { get; set; } // this will be the identifier with name _id
    public string Name { get; set; }
    public float Price { get; set; }
    public int Stock { get; set; }
    public string[] Categories { get; set; } // array field
    public VAT VAT { get; set; } // embedded document
}

public class VAT // this class is only ever embedded, hence needs to id
{
    public string VATCategoryName { get; set; }
    public float Percentage { get; set; }
}
```

Note that the name of the field was `price` before, but in C# it starts with a capital letter, according to _Pascal Case_: `Price`. The MongoDB .NET driver integrates with the C# language and the .NET environment and respects its conventions so that the names in the class definition and the field names in the MongoDB documents will be mapped automatically: the `Price` class property will be `price` in the document.

### Customizing the mapping

The C# class - MongoDB document mapping is automatic, but it can also be customized. There are several ways to deviate from the conventions.

The easiest way is to use custom attributes in the class definition:

```csharp
public class Product
{
    // maps to field _id
    [BsonId]
    public ObjectId Identifier { get; set; }

    // can specify the name explicitly
    [BsonElement("price")]
    public string TotalPrice { get; set; }

    // properties can be ignored
    [BsonIgnore]
    public string DoNotSave { get; set; }
}
```

Our other option is to register so-called _convention packs_ at a higher level. The convention pack describes the rules of mapping. (A set of conventions also defines the default behavior.)

For example, you can specify the following to map the field names to camel case and exclude data members with a default value (defined in the C# language) from the document.

```csharp
// define convention pack
var pack = new ConventionPack();
pack.Add(new CamelCaseElementNameConvention());
pack.Add(new IgnoreIfDefaultConvention(true));

// register the convention pack
// the first parameter is a name to reference this pack
// the last argument is a fitlering criteria when to use this convention
ConventionRegistry.Register("datadriven", pack, t => true);
```

We also have more sophisticated customizations, such as defining conversion logic for translation between a C# representation and a MongoDB representation, and specifying how to save inheritance hierarchies. For more details, see the official documentation: <https://mongodb.github.io/mongo-csharp-driver/2.8/reference/bson/serialization/>.

## Queries

We will use the collection from now on by mapping it to the `Product` class. This is the recommended solution; the `BsonDocument` based solution is used only when necessary.

The simplest query we have already seen is to list all the documents:

```csharp
var collection = db.GetCollection<Product>("products");

var list = collection.Find(new BsonDocument()).ToList();
foreach (var p in list)
    Console.WriteLine($"Id: {p.Id}, Name: {p.Name}");
```

Listing is done using the `Find` method. The name illustrates MongoDB's philosophy: listing an entire collection is not practical, so there is no simple syntax for it. `Find` requires a search criteria, which is an empty condition here to matches everything.

There are several ways to describe search criteria.

With **`BsonDocument`** based filtering, the filtering condition must be written according to the MongoDB syntax. We generally will avoid this because the MongoDB .NET driver provides a more convenient solution for us.

In most cases, we can use **Lambda expressions** to describe the filtering.

```csharp
collection.Find(x => x.Price < 123);
```

In this case, the Lambda expression is a delegate of type `Predicate <T>`, that is, expects a `Product` and returns `bool`. Thus in the example above, the `x` variable represents a `Products` instance. Of course, this search also works for more complex cases.

```csharp
collection.Find(x => x.Price < 123 && x.Name.Contains("red"));
```

The filtering described by the Lambda expressions hides what search syntax we actually have in MongoDB. For example, the above `Contains` search condition will actually mean a search with a regular expression.

In MongoDB's own language, the previous filter looks like this:

```json
{
  "price": {
    "$lt": 123.0
  },
  "name": "/red/s"
}
```

Note that this description is itself a document. If we wanted to write the filter condition ourselves, we would have to create this descriptor in a `BsonDocument`. The document keys describe the fields used for filtering, and the values are the filter criteria. In some cases, the condition is a scalar value such as a regular expression (or if we filter for equality); in other cases, the condition is an embedded document, as with the `<` condition. Here, the `$lt` key is a special key that denotes the _less than_ operator and the value to the right of the operator is 123.0. The regular expression should be specified according to [JavaScript RegExp Syntax](https://www.w3schools.com/jsref/jsref_obj_regexp.asp). The conditions listed in this way are automatically evaluated in and `and` fashion.

Instead of the Lambda expression, we can create a similar description without having to compile a filter condition in "text" form. The .NET driver for MongoDB gives us the ability to use a so-called **_builders_**.

```csharp
collection.Find(
    Builders<Product>.Filter.And(
        Builders<Product>.Filter.Lt(x => x.Price, 123),
        Builders<Product>.Filter.Regex(x => x.Name, "/red/s"),
    )
);
```

The above syntax is a bit more eloquent than the Lambda expression, but it is closer to the MongoDB philosophy, and better describes what we want. We can view this syntax as SQL, a declarative, goal-oriented, but platform-specific description. However, it is also type-safe.

The `Builders<T>` generic class is an auxiliary class that we can use to build filtering and other MongoDB specific definitions. `Builders<Product>.Filter` can be used to define filtering conditions that match the _Product_ C# class. First, we create an _and_ connection, within which we have two filtering conditions. The operators are the _less than_ and regular expressions seen before. We pass two parameters to these functions: the field to be filtered and the operand.

Note that no string-based field names were used here or in the Lambda expressions. We can refer to the class fields with the _C# Expression_ syntax. This is practical because we avoid typing field names.

Note that all ways of describing the search criteria are identical. The MongoDB driver maps each syntax to its internal representation. Lambda expression-based requires fewer characters and fits better into C#, while the builder approach is used to express MongoDB features better. You can use either one.

### Using query results

The result of the `collection.Find(...)` function is not yet the result set, but only a descriptor to execute the query. There are generally three ways to retrieve and process the result.

#### Listing

Get the complete result set as a list: `collection.Find(...).ToList()`.

#### Get first/single item

If you only need the first item, or know that there will be only one item, you can use `collection.Find(...).First()`, `.FirstOrDefault()`, or `.Single()`, `.SingleOrDefault()` functions.

#### Cursor

If the result set contains multiple documents, it is advisable to iterate it using a cursor. MongoDB limits the size of the response to a query, so if we query too many records, we may get an error instead of a result. To overcome this, we use the cursors where we always get only a subset of the documents.

```csharp
var cur = collection.Find(...).ToCursor();
while (cur.MoveNext()) // cursor stepping
{
    foreach (var t in cur.Current) // the value of the cursor is not a single document, but a list in itself
    { ... }
}
```

### Operators for filtering

The filter criteria apply to the fields in the document, and the filter criteria are always constant. Thus **it is not possible, for example, to compare two fields**, and we cannot refer to other collections. There is a so-called MongoDB aggregation pipeline, which allows you to formulate more complex queries, but for now, let us focus on simple queries.

The filter condition compares a field in the document to a constant we specify. The following options are most commonly used.

#### Comparison operators

```csharp
collection.Find(x => x.Price == 123);
collection.Find(Builders<Product>.Filter.Eq(x => x.Price, 123)); //Eq, as in equals

collection.Find(x => x.Price != 123);
collection.Find(Builders<Product>.Filter.Ne(x => x.Price, 123)); // Ne, as in not equals

collection.Find(x => x.Price >= 123);
collection.Find(Builders<Product>.Filter.Gte(x => x.Price, 123)); // Gte, as in greater than or equal to

collection.Find(x => x.Price < 123);
collection.Find(Builders<Product>.Filter.Lt(x => x.Price, 123)); // Lt, as in less than
```

#### Boolean operators

```csharp
collection.Find(x => x.Price > 500 && x.Price < 1000);
collection.Find(
    Builders<Product>.Filter.And(
        Builders<Product>.Filter.Gt(x => x.Price, 500),
        Builders<Product>.Filter.Lt(x => x.Price, 1000)
    )
);

collection.Find(x => x.Price < 500 || x.Stock < 10);
collection.Find(
    Builders<Product>.Filter.Or(
        Builders<Product>.Filter.Lt(x => x.Price, 500),
        Builders<Product>.Filter.Lt(x => x.Stock, 10)
    )
);

collection.Find(x => !(x.Price < 500 || x.Stock < 10));
collection.Find(
    Builders<Product>.Filter.Not(
        Builders<Product>.Filter.Or(
            Builders<Product>.Filter.Lt(x => x.Price, 500),
            Builders<Product>.Filter.Lt(x => x.Stock, 10)
        )
    )
);
```

#### Value is one of multiple alternatives

```csharp
collection.Find(x => x.Id == ... || x.Id = ...);
collection.Find(Builders<Product>.Filter.In(x => x.Id, new[] { ... }));
// similarly Nin, as in not in operátor
```

#### Value exists (not null)

```csharp
collection.Find(x => x.VAT != null);
collection.Find(Builders<Product>.Filter.Exists(x => x.VAT));
```

!!! note "Exists filtering"
    Does exist, that is, non-null filtering is special because there are two ways to have a null value in MongoDB: if the key exists in the document and it has a value of null; or if the key does not exist at all.

### Filtering fields of embedded document

Embedded documents can be used for filtering in the same way. The following are all valid, and it does not matter if the embedded document (_VAT_) does not exist:

```csharp
collection.Find(x => x.VAT.Percentage < 27);
collection.Find(Builders<Product>.Filter.Lt(x => x.VAT.Percentage, 27));

collection.Find(Builders<Product>.Filter.Exists(x => x.VAT.Percentage, exists: false));
// does not exists, that is, in C#, equals null
```

### Filtering based on an array field

Any field in the document can be an array value, as in the example `string [] Categories`. In MongoDB, we can define filtering based on an array field using the `Any*` criterion.

```csharp
// products of this category
collection.Find(Builders<Product>.Filter.AnyEq(x => x.Categories, "Balls"));

// products that are assigned to at least one category not listed by name
collection.Find(Builders<Product>.Filter.AnyNin(x => x.Categories, new[] { "Balls", "Rackets" }));
```

!!! note "Any..."
    The `Any*` conditions look at every element of an array but match only once with respect to the document. So, if multiple elements of an array match a condition, we only get the document once in the result set.

## Query execution pipeline

MongoDB queries are executed through a pipeline. We won't go into details about this, but in addition to simple filtering, we'll see a few examples frequently used in queries.

#### Paging, sorting

For paging, we specify the maximum number of matching documents we request:

```csharp
collection.Find(...).Limit(100);
```

And for the items on the following page, we skip the items already seen on the first page:

```csharp
collection.Find(...).Skip(100).Limit(100);
```

`Skip` and `Limit` are meaningless in this form because without sorting, the "first 100 elements" query is not deterministic. So for these types of queries, it is necessary to provide an appropriate sorting requirement. Sorting is defined using `Builders<T>`.

```csharp
collection.Find(...)
    .Sort(Builders<Product>.Sort.Ascending(x => x.Name))
    .Skip(100).Limit(100);
```

!!! question "Paging issue"
    The above paging mechanism is still not entirely correct. For example, if a product is deleted in between the query of the first and second pages, the products will shift by one, and there may be a product that will be skipped. This is, in fact, not a problem just with MongoDB. Consider how you would solve this problem.

#### Number of documents

There are two ways to query the number of documents that match a query:

```csharp
collection.CountDocuments(Builders<Product>.Filter.AnyEq(x => x.Categories, "Balls"));

collection.Find(Builders<Product>.Filter.AnyEq(x => x.Categories, "Balls")).CountDocuments();
```

#### Aggregation pipeline

Aggregation operations process multiple documents and return some calculated results from them. MongoDB provides three ways to perform aggregation operations:

- Aggregation pipelines,
- Single Purpose Aggregation Operations,
- and Map-reduce functions.

Since MongoDB version 5.0, **Map-reduce** is an obsolete method because the aggregation pipeline is better in terms of usability and speed.

For **Single Purpose Aggregation Operations**, MongoDB provides us with `IMongoCollection<TDocument>.EstimatedDocumentCount()`, `IMongoCollection<TDocument>.Count()` and `IMongoCollection<TDocument>.Distinct()` functions, which all perform simple aggregation on a single collection.

![Single Purpose Aggregation Operation](images/mongodb_spao.svg)

!!! citation "Source"
    <https://docs.mongodb.com/manual/images/distinct.bakedsvg.svg>

General aggregations can be performed by defining a pipeline manually. An **aggregation pipeline** is built up from stages, each serving a specific action _(filter, group, count, calculate, etc.)_ on its input documents. A pipeline can also return multiple results from a set of documents _(e.g., total, average, maximum, or minimum values)_.

Let's look at this through an example of grouping.

```csharp
// products in the "Balls" category grouped by VAT percentage
foreach (var g in collection.Aggregate()
                            .Match(Builders<Product>.Filter.AnyEq(x => x.Categories, "Balls")) // filtering
                            .Group(x => x.VAT.Percentage, x => x) // grouping
                            .ToList())
{
    Console.WriteLine($"VAT percentage: {g.Key}");
    foreach(var p in g)
        Console.WriteLine($"\tProduct: {p.Name}");
}
```

## Insert, Modify, Delete

After queries, let's get to know data modification constructs.

### Inserting a new document

To insert a new document, you need the object representing the new document. We can add this to the collection.

```csharp
var newProduct = new Product
{
    Name = "Apple",
    Price = 890,
    Categories = new[] { "Fruits" }
};
collection.InsertOne(newProduct);

Console.WriteLine($"Inserted record id id: {newProduct.Id}"); // after insert the ID of the document will be available in the C# instance
```

Note that the `Id` field is not assigned. This will be set by the client driver. If we want, we can give it a value, but it is not customary.

Remember, there is no schema in MongoDB, so the inserted document may be completely different from the rest of the items in the collection. Note that not all fields are assigned values. Because there are no integrity criteria, any insertion will be successful, but there may be problems with queries (for example, assuming that the `Stock` field is always set).

You can use the `InsertMany` function to insert multiple documents, but remember that there are no transactions, so adding multiple documents is an independent operation. If, for any reason, an error occurs during the insertion, the successfully inserted documents will remain in the database. However, each document is saved atomically, so no "half" document can be added to the database in the event of an error.

### Delete documents

To delete, you need to define a filter condition and execute it with the `DeleteOne` or `DeleteMany` functions. The difference is that `DeleteOne` only deletes the _first_ matching document, while `DeleteMany` deletes all. If you know that only one document can match this condition (for example, deleting it by ID), you should use `DeleteOne` as the database does not have to perform an exhaustive search.

The deletion condition can be described by the syntax familiar to the search.

!!! note ""
    Deletion is different from Entity Framework. Here, the entity does not have to be loaded; instead, we specify a filtering condition.

```csharp
var deleteResult = collection.DeleteOne(x => x.Id == new ObjectId("..."));
Console.WriteLine($"Deleted: {deleteResult.DeletedCount} records");
```

If you want to retrieve the deleted element, you can use `FindOneAndDelete`, which returns the deleted entity.

### Updating documents

Perhaps the most interesting feature of MongoDB is the update of documents. While the functionalities showed before (queries, inserts, deletions) are similar to most databases (either relational or NoSQL), MongoDB supports a much broader range of modification operations.

There are two ways to change a document: replace the entire document with a new one or update its parts.

#### Complete document replacement

To replace a document completely, we need a filtering condition to specify which document we want to replace; and we need a new document.

```csharp
var replacementProduct = new Product
{
    Name = "Apple",
    Price = 890,
    Categories = new[] { "Fruit" }
};
var replaceResult = collection.ReplaceOne(x => x.Id == new ObjectId("..."), replacementProduct);
Console.WriteLine($"Updated: {replaceResult.ModifiedCount}");
```

A single document is matched and replaces it with another document. The operation itself is atomic, that is, if it is interrupted, no half document is saved. You can use the `FindOneAndReplace` method to get the pre-swap document.

!!! note "Interesting"
    It is also possible to change the document ID during update (the replacement document can have a different ID).

#### Document update operators

Document update operators can change the value of a document's fields atomically without replacing the entire document. We use the help of the `Builder<T>` to describe the modifying operations.

Set your stock to a constant value:

```csharp
collection.UpdateOne(
    filter: x => x.Id == new ObjectId("..."),
    update: Builders<Product>.Update.Set(x => x.Stock, 5));
```

The first parameter of the `UpdateOne` function is the filter condition. You can use any of the syntax described before. The second parameter is the descriptor of the update operation, which you can build with `Builders<T>`.

In the example code above, the argument names are specified (`filter:` and `update:`) to make it clear what the parameter represents. This is optional, but it increases readability (at the expense of code length).

The operation can update multiple fields at the same time.

```csharp
collection.UpdateOne(
    filter: x => x.Id == new ObjectId("..."),
    update: Builders<Product>.Update
                .Set(x => x.Stock, 5)
                .CurrentDate(x => x.StockUpdated)
                .Unset(x => x.NeedsUpdate)
);
```

Typical modifier operators are:

- `Set`: Set the value of the field;
- `SetOnInsert`: like `Set` but executed only when a new document is inserted (see _upsert_ below);
- `Unset`: delete field (remove key and value from document);
- `CurrentDate`: set the current date;
- `Inc`: increment value;
- `Min`,`Max`: change the value of a field if the value entered is smaller / larger than the current value of the field;
- `Mul`: value multiplication;
- `PopFirst`,`PopLast`: remove first / last element from an array;
- `Pull`: remove value from an array;
- `Push`: add value to an array at the end (further options in the same operator: array sorting, keeping the first _n_ element of an array);
- `AddToSet`: add a value to an array if it does not already exist.

The above operations are meaningful even if the specified field does not exist. Depending on the type of operator, the database will make changes to a default value. For example, for `Inc` and `Mul`, the field will be set to 0 and then modified. For array operations, an empty array is modified. For other operations, you can look up the behavior in the [documentation](https://docs.mongodb.com/manual/reference/operator/update/).

Multiple documents can be modified at the same time using this method. The requested update operations are performed on all documents that match the filter criteria.

For example: in view of the summer season, put _all_ balls on sale with a 25% discount.

```csharp
collection.UpdateMany(
    filter: Builders<Product>.Filter.AnyEq(x => x.Categories, "Balls"),
    update: Builders<Product>.Update.Mul(x => x.Price, 0.75)
                                   .AddToSet(x => x.Categories, "On sale"));
```

Update operators change the documents atomically. Using them can eliminate some of the problems caused by concurrent data access.

#### _Upsert_: updating or inserting a document

During update operations, we have the option to _upsert (update/insert)_. This means that either an insertion or an update is made, depending on whether the item was in the database. The default behavior is _not_ to upsert, we must request it explicitly.

```csharp
collection.ReplaceOne(
    filter: x => x.Id == new ObjectId("..."),
    replacement: replacementObject,
    options: new UpdateOptions() { IsUpsert = true });
```

We can also do upsert with update operators. As we have seen, modifier operators are not concerned about missing fields. Likewise, it does not matter if the document does not exist; this is equivalent to performing a modifying operation on a completely blank document.

```csharp
collection.UpdateOne(filter: ..., update: ..., options: new UpdateOptions() { IsUpsert = true });
```

The upsert operation can be a workaround for managing concurrency in the absence of a transaction. Because we do not have a transaction, we cannot verify before insertion that a particular record does not yet exist. Instead, we can use the upsert method, which allows atomic querying and insertion/modification.

!!! note "`merge`"
    Note: In SQL, the `merge` command provides a similar solution.
