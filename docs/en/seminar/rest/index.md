# REST API & ASP.NET Web API

The seminar's goal is to practice designing REST APIs and to get to know the .NET Web API technology.

## Pre-requisites

Required tools to complete the tasks:

- Microsoft Visual Studio 2026 (_not_ VS Code)
- Microsoft SQL Server (LocalDB or Express edition)
- SQL Server Management Studio
- Database initialization script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/datadriven/master/overrides/db/mssql.sql)
- Starter code: <https://github.com/bmeviauac01/gyakorlat-rest-kiindulo>

Recommended to review:

- C# language
- Entity Framework and Linq
- REST API and Web API lecture

## How to work during the seminar

The exercises are solved together with the instructor. Try to solve each exercise on your own first, then discuss the solution together. The last two exercises are optional, if time permits.

!!! info ""
    As a reminder, the solutions are included in this guide. Before looking at them, try to solve the exercise yourself!

## Exercise 0: Create/check the database

The database resides on each machine; thus, the database you created previously might not be available. First, check if your database exists, and if it does not, create and initialize it. (See the instructions [in the first seminar material](../transactions/index.md).)

## Exercise 1: Open the starter project

1. Download the project skeleton!

    - Open a new _command prompt_
    - Navigate to a directory, e.g. `c:\work\NEPTUN`
    - Execute the following command: `git clone --depth 1 https://github.com/bmeviauac01/gyakorlat-rest-kiindulo.git`

1. Open the _sln_ file in the cloned directory using Visual Studio.

1. Let us examine the project.

    - This is an ASP.NET Core Web API project, created specifically for hosting REST APIs. It contains a web server internally; thus, when started with ++f5++, it can serve incoming requests.
    - Let us examine the contents of `Program.cs`. It essentially consists of two parts:
        - It creates a `WebApplicationBuilder` object, whose `Services` property is used to configure the Dependency Injection container.
        - After `Build`, we can configure the ASP.NET Core middleware pipeline, which currently only contains the support for controllers. Then the application is run using an embedded web server (Kestrel).
    - The Entity Framework mapping of our database (using the _Code First_ model) is in the `Dal` folder. The `DataDrivenDbContext` class is the central class of data access.
    - The _connection string_ is in the application's configuration file, `appsettings.json`.
    - There is a test controller in the `Controllers` folder. Let us open and examine it. Let us note the `[ApiController]` and `[Route]` attributes and the inheritance. These make a class a _Web API controller_. Everything else works automatically: the controller's methods are invoked for the matching requests (depending on the route and the http method), so no additional configuration is needed.

1. In `appsettings.json`, change the database name in the connection string.

1. Start the application. After building, a console application starts (this time no browser is opened automatically), where we can see the logs. Open a browser and navigate to <http://localhost:5000/api/values>. We should receive a JSON response. Stop the application either by pressing ++ctrl+c++ in the console application, or stop it in Visual Studio.

## Exercise 2: First controller and method, testing with an `.http` file

Create a new Web API controller that responds with a greeting. Test the behavior using the built-in HTTP client of Visual Studio, with an `.http` file.

!!! tip "`.http` files in Visual Studio"
    Starting from version 17.8 of Visual Studio 2022 (and so in Visual Studio 2026 as well), Visual Studio natively supports `.http` files: in these files we can write HTTP requests in plain text, run them by clicking the _Send Request_ link displayed above the request, and see the response in a separate pane. This is a lightweight alternative to standalone applications such as Postman that needs no separate installation - and since the file is part of the project, it can be versioned with git as well.

    Create a `Hello.http` file in the project: in _Solution Explorer_ right-click the project, choose _Add / New Item..._, and look for the _HTTP File_ template.

    A request is as simple as this:

    ```http
    GET http://localhost:5000/api/hello
    ```

    If a file contains multiple requests, they are separated by `###`.

1. Delete the `ValuesController` class. Instead, add a new empty _Api Controller_ named `HelloController`: in _Solution Explorer_ right-click the _Controllers_ folder and choose _Add / Controller... / API Controller - Empty_. The `HelloController` should be available at URL `/api/hello`.
1. Create a method responding to a `GET` request that returns a text. Test it in the `.http` file: add the line `GET http://localhost:5000/api/hello`, then run it by clicking the _Send Request_ link above the line.
1. Change the method serving the REST request so that it optionally accepts a name as a _query parameter_, that is, in the url, and if one is provided, the response should be "Hello" + the received name. Test this as well: add another request, separated from the previous one by `###`, with the content `GET http://localhost:5000/api/hello?name=apple`, and run it too.
1. Finally, create a _new_ REST API endpoint (a new method) that responds at URL <http://localhost:5000/api/hello/apple> just like the previous one did (only now the name is part of the _path_). Add this as another request to the `.http` file as well, and check it.

??? example "Solution"
    ```csharp
    [Route("api/[controller]")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        // subtask 2
        //[HttpGet]
        //public string Hello()
        //{
        //    return "Hello!";
        //}

        // subtask 3
        [HttpGet]
        public string Hello([FromQuery] string name)
        {
            return string.IsNullOrEmpty(name)
                ? "Hello noname!"
                : $"Hello {name}";
        }

        // subtask 4
        [HttpGet("{personName}")] // the name between {} in the route must match the parameter name
        public string HelloRoute(string personName)
        {
            return "Hello route " + personName;
        }
    }
    ```

    Let us summarize what we need to create a WebAPI endpoint:

    - Inherit from `ControllerBase` and add the `[ApiController]` attribute to the class.
        - In this example, inheriting from `ControllerBase` would not be necessary, because the framework does not require it, and we do not use any methods of the base class here.
    - Specify which http request the endpoint responds to with the appropriate `[Http*]` attribute.
    - Specify the route, either on the class or on the method (or on both), with the `[Route]` or the `[HttpXXX]` attribute.
    - Create a method with the right signature (e.g. return value, parameters).

## Exercise 3: Product search API

A real API does not return constants, of course. Create an API for searching among the products sold in the webshop.

- Create a new controller for this.
- Enable listing the products, but only with paging (max. 5 items on each page).
- Enable searching by product name.
- The returned product entity should _not_ be the entity from the database mapping; instead, create a new, so-called _DTO_ (data transfer object) `record` class in a new folder called `Dtos`.

### Using DTOs

The returned product entity should _not_ be the entity from the database mapping; instead, create a new, so-called _DTO_ (data transfer object) class in a new folder called `Dtos`. Create a record class named `Product` for the DTO.

!!! tip "Records in C#"
    The `record` keyword represents a type (a class by default) that has a constructor defined in its header and properties with [`init` only setters](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/init). Thus a record behaves immutably, which fits the behavior of a DTO better. Records also provide other convenience features ([see more](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)), but we will not use them here.

??? example "Solution"

    ```csharp title="Dtos/Product.cs"
    namespace Bme.DataDriven.Rest.Dtos;

    public record Product(int Id, string Name, double? Price, int? Stock);
    ```

### Creating the listing endpoint

Create the endpoint according to the requirements in a new `ProductController` class, then test the application.

??? example "Solution"

    ```csharp
    using Microsoft.AspNetCore.Mvc;

    namespace Bme.DataDriven.Rest.Controllers;

    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly Dal.DataDrivenDbContext _dbContext;

        // This is how we get the database. The framework provides the Dependency Injection service.
        // The DbContext is automatically disposed at the end of the request (DI configuration).
        public ProductController(Dal.DataDrivenDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public List<Dtos.Product> List([FromQuery] string search = null, [FromQuery] int from = 0)
        {
            var filteredList = string.IsNullOrEmpty(search)
                ? _dbContext.Product // if there is no search by name, all products
                : _dbContext.Product.Where(p => p.Name.Contains(search)); // search by name

            return filteredList
                .Skip(from) // paging: starting from which product
                .Take(5) // max 5 products on a page
                .Select(p => new Dtos.Product(p.Id, p.Name, p.Price, p.Stock)) // database entity -> DTO
                .ToList(); // enforce evaluating the IQueryable above, otherwise we would get an error
        }
    }
    ```

The database context can simply be requested via DI as a constructor parameter.

Let us note that we did not need to deal with JSON serialization. The API only returns DTOs; the framework automatically takes care of the serialization.

Paging is worth adding to limit the size of the response (just as paging is customary on user interfaces). A typical solution for this is the "from" style approach used here.

!!! note "Paging differently"
    Paging can be designed in many ways for REST APIs. The solution above is the simplest one, but another approach could be to let the client specify the page size and to send the index of the requested page instead of the absolute `from` offset.

The result of the method before `ToList` is an `IQueryable<T>`. Let us remember that an `IQueryable<T>` does not contain the result; it is merely a descriptor.

!!! warning "`IQueryable<T>` return value and `DbContext` lifetime"
    Without the `ToList` at the end, the application would fail, because when the JSON serialization started iterating the collection, it would try to work on an already closed database connection. Because of this, **never** return `IQueryable` from WebAPI endpoints!

    The reason is that by default `DbContext` types are registered in the DI container with a `Scoped` lifetime, and in ASP.NET Core, by default, a scope is created for each HTTP request. The serialization, however, would fall outside this scope.

## Exercise 4: Product editing API

Extend the product search API with the following functionality:

- Fetch the data of a particular product by its id at url `/api/product/{id}`.
- Modify an existing product (name, price, stock).
- Add a new product (create a new DTO class for this that contains only the name, stock, and price).
- Delete a product by its id.

Test each endpoint!

??? tip "REST API design conventions cheatsheet"

    In REST APIs, every URL (its path part) represents a resource, on which we can perform operations with HTTP verbs, and the server responds with HTTP status codes and DTOs.

    **Typical CRUD resources and their operations**

    | Verb       | URL                    | Success status code | Description                                |
    |------------|------------------------|---------------------|--------------------------------------------|
    | GET        | /api/product           | 200 OK              | list of resources                          |
    | GET        | /api/product?name=Test | 200 OK              | list of resources (filtered)               |
    | POST       | /api/product           | 201 Created         | insert into the list                       |
    | GET        | /api/product/1         | 200 OK              | fetch a resource with a given identifier   |
    | PUT, PATCH | /api/product/1         | 200 OK              | modify a resource with a given identifier  |
    | DELETE     | /api/product/1         | 204 NoContent       | delete a resource with a given identifier  |

    **Typical error status codes in REST APIs**

    | Who made the mistake | Status code               | Description                                        |
    |----------------------|---------------------------|----------------------------------------------------|
    | Client               | 400 Bad Request           | The client sent semantically invalid data          |
    | Client               | 401 Unauthorized          | Login required                                     |
    | Client               | 403 Forbidden             | There is a logged-in user, but without permission  |
    | Client               | 404 Not Found             | Resource not found                                 |
    | Server               | 500 Internal Server Error | An unexpected error occurred                       |

### Fetch by ID

When fetching, also consider the case when the request contains an ID that does not exist in the database. In this case, respond with HTTP status code `404 Not Found`. For this, use the `ActionResult<T>` return value and the helper methods in `ControllerBase`.

??? example "Solution"

    ```csharp
    [HttpGet("{id}")]
    public ActionResult<Dtos.Product> Get(int id)
    {
        var dbProduct = _dbContext.Product.SingleOrDefault(p => p.Id == id);
        return dbProduct != null
            ? Ok(new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)) // on success, return the data itself
            : NotFound(); // 404 http response if the item is not found
    }
    ```

!!! tip "ActionResult<T> by default"
    Customizing the status code is straightforward with the `ActionResult<T>` class and its helper methods. However, let us consider that in the previous exercises we only returned a DTO, where the framework used the 200 OK default, so it was not necessary to explicitly return an `ActionResult<T>`.

    The framework provides one more simplification: we can return the plain DTO even if the return type of the controller action is `ActionResult<T>`, e.g.:

    ```csharp hl_lines="2"
    return dbProduct != null
        ? new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)
        : NotFound();
    ```

### Inserting a new product

- Create the DTO class for the data arriving to the server as a record, and the inserting endpoint.
- Insertion is typically a POST request sent to the URL of the list resource.
- In the response, return the inserted data, and the URL of the inserted resource in the `Location` header. The `CreatedAtAction` method will help us with this.

??? example "Solution"

    ```csharp
    namespace Bme.DataDriven.Rest.Dtos;

    public record NewProduct(string Name, double? Price, int? Stock);
    ```

    ```csharp
    [HttpPost]
    public ActionResult<Dtos.Product> Add([FromBody] Dtos.NewProduct newProduct)
    {
        var dbProduct = new Dal.Product()
        {
            Name = newProduct.Name,
            Price = newProduct.Price,
            Stock = newProduct.Stock,
            CategoryId = 1, // not nice, temporary solution
            VatId = 1 // not nice, temporary solution
        };

        // save to the database
        _dbContext.Product.Add(dbProduct);
        _dbContext.SaveChanges();

        // this is how we tell where the inserted item can be fetched
        return CreatedAtAction(
            nameof(Get),
            new { id = dbProduct.Id },
            new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)); 
    }
    ```

To test the product endpoints, create a new `Product.http` file in the project (the same way as `Hello.http`). To insert a new product, add a `POST` request to it. Separate the JSON body from the headers with an empty line, and do not forget the `Content-Type` header either:

```http
POST http://localhost:5000/api/product
Content-Type: application/json

{
    "name": "BME cardigan",
    "price": 8900,
    "stock": 100
}
```

During testing, also check the headers of the response, available on the _Headers_ tab at the top of the response pane! In case of an insert, look for the `Location` key among them. This is where the system tells where the result can be fetched. In addition, a POST request usually also returns the inserted data in the response.

### Modifying a product

- Modification is typically represented by the PUT verb.
- Modifying a non-existent resource should result in a 404 error code.
- When implementing the modification, use the existing `Product` DTO, and validate that the ID received in the path and in the body are the same. For this, we can use the `ModelState` property and the `BadRequest` methods.
- The modification is performed via EF as usual.
- The modifying method also typically returns the modified data.

??? example "Solution"

    ```csharp
    [HttpPut("{id}")]
    public ActionResult<Dtos.Product> Modify([FromRoute]int id, [FromBody]Dtos.Product updated)
    {
        if (id != updated.Id)
        {
            ModelState.AddModelError(nameof(id), "Invalid ID received");
            return BadRequest(ModelState);
        }

        var dbProduct = _dbContext.Product.SingleOrDefault(p => p.Id == id);
        if (dbProduct == null)
            return NotFound();

        // perform the modifications
        dbProduct.Name = updated.Name;
        dbProduct.Price = updated.Price;
        dbProduct.Stock = updated.Stock;

        // save to the database
        _dbContext.SaveChanges();

        return new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock);
    }
    ```

To test the modification, add a `PUT` request to the `Product.http` file (separated from the previous request by `###`):

```http
PUT http://localhost:5000/api/product/10
Content-Type: application/json

{
    "id": 10,
    "name": "Silence for one hour",
    "price": 440,
    "stock": 10
}
```

Also try the request with the two IDs in the path and in the body not matching (e.g. change the URL of the request above to `.../api/product/11`, leaving the body unchanged). In this case, we should get a 400 Bad Request with the error details.

!!! note "Validating DTOs"
    DTOs can be subjected to other validations as well, for which we can use the built-in [validation attributes](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0#validation-attributes) of ASP.NET Core, or even other external libraries, such as [FluentValidation](https://docs.fluentvalidation.net/en/latest/).

!!! note "PUT vs PATCH"
    For the modification operation, the PUT or PATCH verbs are typically used. The main difference between them is that PUT expects the entire modified resource as input, whereas PATCH expects only a partial data set (typically key-value pairs). In .NET, PUT is simpler to implement, but there is [built-in support](https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-10.0) for PATCH as well.

### Deleting a product

- For deletion, we use the DELETE HTTP verb; on the success path, it should respond with 204 No Content.
- A non-existent resource should result in 404 here as well.

??? example "Solution"

    ```csharp
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var dbProduct = _dbContext.Product.SingleOrDefault(p => p.Id == id);
        if (dbProduct == null)
            return NotFound();

        _dbContext.Product.Remove(dbProduct);
        _dbContext.SaveChanges();

        return NoContent(); // a successful delete is signaled with a 204 NoContent response (could be 200 OK as well if we included the entity)
    }
    ```

!!! note "Idempotent delete operation"
    A typical design decision is to make the delete operation idempotent, that is, to give the same result when run multiple times in a row. This will not be true in our case, because we send 404 for a non-existent resource and 204 for an existing one. This operation could be made idempotent by always returning status code 204, even if we did nothing.

## Exercise 5 (optional): Creating a new product: category and VAT

When creating a new product, we should also specify the category and the VAT rate. Modify the product insertion above so that the name of the category and the numeric value of the VAT rate can be specified as well. Based on the received data, look up the corresponding `VAT` and `Category` records in the database, or create new ones if they do not exist.

??? example "Solution"

    ```csharp title="NewProduct.cs" hl_lines="5-6"
    public record NewProduct(
        string Name,
        double? Price,
        int? Stock,
        int VatPercentage,
        string CategoryName);
    ```

    ```csharp title="ProductController.cs" hl_lines="4-10 17-18"
    [HttpPost]
    public ActionResult<Dtos.Product> Add([FromBody]Dtos.NewProduct newProduct)
    {
        var dbVat = _dbContext.Vat.SingleOrDefault(v => v.Percentage == newProduct.VatPercentage);
        if (dbVat == null)
            dbVat = new Dal.VAT() { Percentage = newProduct.VatPercentage };

        var dbCat = _dbContext.Category.SingleOrDefault(c => c.Name == newProduct.CategoryName);
        if (dbCat == null)
            dbCat = new Dal.Category() { Name = newProduct.CategoryName };

        var dbProduct = new Dal.Product()
        {
            Name = newProduct.Name,
            Price = newProduct.Price,
            Stock = newProduct.Stock,
            Category = dbCat,
            VAT = dbVat,
        };

        // save to the database
        _dbContext.Product.Add(dbProduct);
        _dbContext.SaveChanges();

        // this is how we tell where the inserted item can be fetched
        return CreatedAtAction(
            nameof(Get),
            new { id = dbProduct.Id },
            new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)); 
    }
    ```

## Exercise 6 (optional): Asynchronous controller method

Rewrite the previous exercise to be [**asynchronous**](../../lecture-notes/async/index.md), that is, use `async-await`. With asynchronous execution, the server uses the available threads more efficiently while waiting for database operations. We can do this easily, because Entity Framework provides asynchronous execution out of the box, which we can use in our controller method.

??? example "Solution"

    ```csharp hl_lines="2 4 8 23"
    [HttpPost]
    public async Task<ActionResult<Dtos.Product>> Add([FromBody]Dtos.NewProduct newProduct)
    {
        var dbVat = await _dbContext.Vat.SingleOrDefaultAsync(v => v.Percentage == newProduct.VatPercentage);
        if (dbVat == null)
            dbVat = new Dal.VAT() { Percentage = newProduct.VatPercentage };

        var dbCat = await _dbContext.Category.SingleOrDefaultAsync(c => c.Name == newProduct.CategoryName);
        if (dbCat == null)
            dbCat = new Dal.Category() { Name = newProduct.CategoryName };

        var dbProduct = new Dal.Product()
        {
            Name = newProduct.Name,
            Price = newProduct.Price,
            Stock = newProduct.Stock,
            Category = dbCat,
            VAT = dbVat,
        };

        // save to the database
        _dbContext.Product.Add(dbProduct);
        await _dbContext.SaveChangesAsync();

        // this is how we tell where the inserted item can be fetched
        return CreatedAtAction(
            nameof(Get),
            new { id = dbProduct.Id },
            new Dtos.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)); 
    }
    ```

Let us see how simple this was. We use the `...Async` methods provided by Entity Framework, `await`-ing each of them, and we only had to change the method signature to return `Task<T>` (so that it can be awaited asynchronously from outside) and mark it with the `async` keyword (so that we can use `await` inside). Everything else is still taken care of by the framework.

!!! note "Asynchrony in server applications"
    `async-await` is a .NET framework capability supported by both ASP.NET Core and Entity Framework. However, we can encounter it in many other places as well, for example in client applications.

    In server applications, our primary goal is to increase throughput by letting the serving thread handle another HTTP request while waiting asynchronously (in our case, for a DB operation).
