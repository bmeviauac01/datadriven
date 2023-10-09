# REST API & ASP.NET Web API

The seminar's goal is to practice working with REST APIs and the .NET Web API technology.

## Pre-requisites

Required tools to complete the tasks:

- Microsoft Visual Studio 2022 (_not_ VS Code)
- Microsoft SQL Server (LocalDB or Express edition)
- SQL Server Management Studio
- Postman: <https://www.getpostman.com/downloads/>
- Database initialization script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/datadriven/master/overrides/db/mssql.sql)
- Starter code: <https://github.com/bmeviauac01/gyakorlat-rest-kiindulo>

Recommended to review:

- C# language
- Entity Framework and Linq
- REST API and Web API lecture

## How to work during the seminar

The exercises are solved together with the instructor. A few exercises we can try to solve by ourselves and then discuss the results. The final exercise is individual work if time permits.

!!! info ""
    This guide summarizes and explains the behavior. Before looking at these provided answers, we should think first!

## Exercise 0: Create/check the database

The database resides on each machine; thus, the database you created previously might not be available. First, check if your database exists, and if it does not, create and initialize it. (See the instructions [in the first seminar material](../transactions/index.md).)

## Exercise 1: Open starter project

1. Download the project skeleton!

    - Open a new _command prompt_
    - Navigate to a directory, e.g. `c:\work\NEPTUN`
    - Execute the following command: `git clone --depth 1 https://github.com/bmeviauac01/gyakorlat-rest-kiindulo.git`

1. Open the _sln_ file in the `rest` folder using Visual Studio.

1. Let us examine this project.

    - This is an ASP.NET Core Web API project. This project is created for hosting REST API backends. It contains a web server internally; thus, when running it using F5 we get a fully functional API able to respond to http requests.
    - Let us examine `Program.cs`. We do not need to understand everything here. This is like a console application; the `Main` method here, the entry point that starts a web server.
    - The Entity Framework _Code First_ mapping of our database is in the `Dal` folder. Class `DataDrivenDbContext` is the data access class. We need to fix the _connection string_ in the `OnConfiguring` method in this class.

        !!! note ""
            The connection string usually should not be hard-wired in the source code. This is for the sake of simplicity here.

    - There is a test controller in folder `Controllers`. Let us open and examine the code. Let us note the `[ApiController]` and `[Route]` attributes and the inheritance. These make a class a _Web API controller_. The behavior is automatic: the controller's methods are invoked by the framework when they match the expected signature. This means that no additional configuration is needed here.

1. Start the application. After building the source code, a console application will start where we will see diagnostic messages. Let us open a browser and navigate to <http://localhost:5000/api/values>. We should receive a JSON response. Stop the application by pressing _Ctrl-C_ in the console, or stop with Visual Studio.

## Exercise 2: First controller and testing with Postman

Create a new Web API controller that responds with a greeting. Test the behavior using Postman.

1. Delete the existing class `ValuesController`. Add a new empty _Api Controller_ with the name `HelloController`: in _Solution Explorer_ right-click the _Controllers_ folder and choose _Add / Controller... / API Controller - Empty_. The `HelloController` should respond to URL `/api/hello`.
1. The application shall respond with a text when GET request is received. Test this endpoint using Postman by sending a GET request to <http://localhost:5000/api/hello>.
1. Change the REST endpoint by expecting an optional name as a _query parameter_; if such value is provided, the response greeting should include this name. Test this with Postman: send a name by calling URL <http://localhost:5000/api/hello?name=apple>.
1. Create a _new_ REST API endpoint that responds to URL <http://localhost:5000/api/hello/apple> just like the previous one, but the name is in the _path_ here.

??? example "Solution"
    ```csharp
    [Route("api/hello")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        // 2.
        //[HttpGet]
        //public ActionResult<string> Hello()
        //{
        //    return "Hello!";
        //}

        // 3.
        [HttpGet]
        public ActionResult<string> Hello([FromQuery] string name)
        {
            if(string.IsNullOrEmpty(name))
                return "Hello noname!";
            else
                return "Hello " + name;
        }

        // 4.
        [HttpGet]
        [Route("{personName}")] // the liter inside {} in this route must match the parameter name
        public ActionResult<string> HelloRoute(string personName)
        {
            return "Hello route " + personName;
        }
    }
    ```

    Let us summarize what we need to create a new WebAPI endpoint:

    - Inherit from the `ControllerBase` class and add the `[ApiController]` attribute.
    - Specify the URL route on the class or above the method (or on both) using the `[Route]` attribute.
    - Define a method with the right signature (return value and parameters).
    - Choose what type of http queries to respond to using one of the `[Http*]` attributes.

## Exercise 3: Product search API

A real API does not return constant strings. Create an API for searching among the products of our webshop.

- Create a new controller.
- Enable listing products; 5 per page.
- Enable search based on the name.
- The data returned should _not_ be the database entity; instead create a new _DTO_ (data transfer object) class in a new folder called `Models`.

Test the new endpoints.

??? example "Solution"
    ```csharp
    // *********************************
    // Models/Product.cs

    namespace BME.DataDriven.REST.Models
    {
        public class Product
        {
            public Product(int id, string name, double? price, int? stock)
            {
                Id = id;
                Name = name;
                Price = price;
                Stock = stock;
            }

            // Contains only the relevant data; e.g. the database foreign keys are of no use here.
            // Assignment only via the constructor; this makes it unambiguous
            // that this is a snapshot of information that cannot be modified.

            public int Id { get; private set; }
            public string Name { get; private set; }
            public double? Price { get; private set; }
            public int? Stock { get; private set; }
        }
    }



    // *********************************
    // Controllers/ProductsController.cs

    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    namespace BME.DataDriven.REST.Controllers
    {
        [Route("api/products")] // it is better to explicitly specify the url
        [ApiController]
        public class ProductsController : ControllerBase
        {
            private readonly Dal.DataDrivenDbContext dbContext;

            // The database is obtained through the Dependency Injection service of the framework.
            // The DbContext is automatically disposed at the end of the request.
            public ProductsController(Dal.DataDrivenDbContext dbContext)
            {
                this.dbContext = dbContext;
            }

            [HttpGet]
            public ActionResult<Models.Product[]> List([FromQuery] string search = null, [FromQuery] int from = 0)
            {
                IQueryable<Dal.Product> filteredList;

                if (string.IsNullOrEmpty(search)) // no search yields all products
                    filteredList = dbContext.Product;
                else // search by name
                    filteredList = dbContext.Product.Where(p => p.Name.Contains(search));

                return filteredList
                        .Skip(from) // paging: from which product
                        .Take(5) // 5 items on one page
                        .Select(p => new Models.Product(p.Id, p.Name, p.Price, p.Stock)) // db to dto conversion
                        .ToArray(); // enforce evaluating the IQueryable - otherwise would yield an error
            }
        }
    }
    ```

    Let us note that we did not need to concern ourselves with JSON serialization. The API returns objects. The framework automatically handles the serialization.

    Paging is useful to limit the size of the response (and paging is also customary on UIs). Specifying a “from” is a simple and frequently used solution.

    The result of the method before the `ToArray` is an `IQueryable`. We may remember that the `IQueryable` does not contain the result; it is merely a descriptor of the query. If we had no `ToArray`, we would see an error. When the framework would begin the serialization to JSON, it would start iterating the query; but at this point, the database connection has already been closed. Therefore WebAPI endpoints should not return `IEnumerable` or `IQueryable`.

## Exercise 4: Editing products via the API

Add the following functionality to our API:

- Fetch the data of a particular product specified by id at url `/api/products/id`.
- Update the name, price, and stock of a product.
- Add a new product (create a new DTO class for input that contains only the name, price, and stock).
- Delete a product by specifying the id.

Test each endpoint!

**Inserting** a new product you will need the following settings in Postman:

- POST request to the correct URL
- Specify the _Body_: choose `raw` and then `JSON`
- And use the JSON as _body_ below:
  ```json
  {
    "name": "BME pen",
    "price": 8900,
    "stock": 100
  }
  ```
  
Note: In our case the JSON data is deserialized into a newly introduced (see later) `Models.NewProduct` object. As the property setters are private in this class, JSON field names are mapped to the constructor parameter names of this class (in a case insensitive manner): therefore, it’s important how we name the constuctor parameters in this class.

**Updating** a product you will need the following settings:

- PUT request to the correct URL
- Specify the _Body_: choose `raw` and then `JSON`
- And use the JSON as _body_ below:
  ```json
  {
    "ID": 10,
    "name": "Silence for one hour",
    "price": 440,
    "stock": 10
  }
  ```
  
Note: In our case the JSON data is deserialized into a `Models.Product` object. As the property setters are private in this class, JSON field names are mapped to the constructor parameter names of this class (in a case insensitive manner): therefore, it’s important how we name the constuctor parameters in this class.

![Postman PUT query](images/postman-put-query.png)

Make sure to check the headers of the response too! Update and insert should add the _Location_ header. This header should contain the URL to fetch the record.

??? example "Solution"
    ```csharp
    // *********************************
    // Models/NewProduct.cs

    namespace BME.DataDriven.REST.Models
    {
        public class NewProduct
        {
            public NewProduct(string name, double? price, int? stock)
            {
                Name = name;
                Price = price;
                Stock = stock;
            }

            public string Name { get; private set; }
            public double? Price { get; private set; }
            public int? Stock { get; private set; }
        }
    }



    // *********************************
    // Controllers/ProductsController.cs
    namespace BME.DataDriven.REST.Controllers
    {
        public class ProductsController : ControllerBase
        {
            // ...

            // GET api/products/id
            [HttpGet]
            [Route("{id}")]
            public ActionResult<Models.Product> Get(int id)
            {
                var dbProduct = dbContext.Product.SingleOrDefault(p => p.Id == id);

                if (dbProduct == null)
                    return NotFound(); // expected response when an item is not found
                else
                    return new Models.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock); // in case of success return the item itself
            }

            // PUT api/products/id
            [HttpPut]
            [Route("{id}")]
            public ActionResult Modify([FromRoute] int id, [FromBody] Models.Product updated)
            {
                if (id != updated.Id)
                    return BadRequest();

                var dbProduct = dbContext.Product.SingleOrDefault(p => p.Id == id);

                if (dbProduct == null)
                    return NotFound();

                // modifications performed here
                dbProduct.Name = updated.Name;
                dbProduct.Price = updated.Price;
                dbProduct.Stock = updated.Stock;

                // save to database
                dbContext.SaveChanges();

                return NoContent(); // response 204 NoContent
            }

            // POST api/products
            [HttpPost]
            public ActionResult Create([FromBody] Models.NewProduct newProduct)
            {
                var dbProduct = new Dal.Product()
                {
                    Name = newProduct.Name,
                    Price = newProduct.Price,
                    Stock = newProduct.Stock,
                    CategoryId = 1, // not nice, temporary solution
                    VatId = 1 // not nice, temporary solution
                };

                // save to database
                dbContext.Product.Add(dbProduct);
                dbContext.SaveChanges();

                return CreatedAtAction(nameof(Get), new { id = dbProduct.Id }, new Models.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)); // this will add the URL where the new item is available into the header
            }

            // DELETE api/products/id
            [HttpDelete]
            [Route("{id}")]
            public ActionResult Delete(int id)
            {
                var dbProduct = dbContext.Product.SingleOrDefault(p => p.Id == id);

                if (dbProduct == null)
                    return NotFound();

                dbContext.Product.Remove(dbProduct);
                dbContext.SaveChanges();

                return NoContent(); // successful delete is signaled with 204 NoContent (could be 200 OK as well if we included the entity)
            }
        }
    }
    ```

## Exercise 5: Add new product with category and VAT

When creating the new product, we have to specify the category, as well as the value-added tax. Change the insert operation from before by allowing the category name and the tax percentage to be specified. Find the `VAT` and `Category` records based on the provided data, or create new records if needed.

??? example "Solution"
    ```csharp
    // *********************************
    // Models/NewProduct.cs
    namespace BME.DataDriven.REST.Models
    {
        public class NewProduct
        {
            // ...
            // Also extend the constructor!
            // Important note: It's important how constructor parameters are named.
            // Our properties have private setters, and thanks to this json deserialization
            // maps JSON object field names to constructor parameter names (in a case
            // insensitive manner).

            public int VATPercentage { get; private set; }
            public string CategoryName { get; private set; }
        }
    }

    // *********************************
    // Controllers/ProductsController.cs
    namespace BME.DataDriven.REST.Controllers
    {
        // ...

        [HttpPost]
        public ActionResult Create([FromBody] Models.NewProduct newProduct)
        {
            var dbVat = dbContext.Vat.FirstOrDefault(v => v.Percentage == newProduct.VATPercentage);
            if (dbVat == null)
                dbVat = new Dal.VAT() { Percentage = newProduct.VATPercentage };

            var dbCat = dbContext.Category.FirstOrDefault(c => c.Name == newProduct.CategoryName);
            if (dbCat == null)
                dbCat = new Dal.Category() { Name = newProduct.CategoryName };

            var dbProduct = new Dal.Product()
            {
                Name = newProduct.Name,
                Price = newProduct.Price,
                Stock = newProduct.Stock,
                Category = dbCat,
                VAT = dbVat
            };

            // save to database
            dbContext.Product.Add(dbProduct);
            dbContext.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = dbProduct.Id }, new Models.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)); // this will add the URL where the new item is available into the header
        }
    }
    ```

## Exercise 6: Asynchronous controller method

Let us refactor the previous exercise code for [**asynchronous**](../../lecture-notes/async/index.md) execution, that is, let us use `async-await`. Asynchronous execution utilizes the execution threads of the server more efficiently while waiting for database operations. We can easily make our code asynchronous by relying on the asynchronous support of Entity Framework.

??? example "Solution"
    ```csharp hl_lines="2 4 8 23"
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Models.NewProduct newProduct)
    {
        var dbVat = await dbContext.Vat.FirstOrDefaultAsync(v => v.Percentage == newProduct.VATPercentage);
        if (dbVat == null)
            dbVat = new Dal.VAT() { Percentage = newProduct.VATPercentage };

        var dbCat = await dbContext.Category.FirstOrDefaultAsync(c => c.Name == newProduct.CategoryName);
        if (dbCat == null)
            dbCat = new Dal.Category() { Name = newProduct.CategoryName };

        var dbProduct = new Dal.Product()
        {
            Name = newProduct.Name,
            Price = newProduct.Price,
            Stock = newProduct.Stock,
            Category = dbCat,
            VAT = dbVat
        };

        // save to database
        dbContext.Product.Add(dbProduct);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = dbProduct.Id }, new Models.Product(dbProduct.Id, dbProduct.Name, dbProduct.Price, dbProduct.Stock)); // this will add the URL where the new item is available into the header
    }
    ```

    Let us see how simple this was. Entity Framework provides us the `...Async` methods, and we only have to `await` them, and update the method signature a litte. Everything else is taken care of by the framework.

    Note. The `async-await` is a .NET frmaework feature supported by both ASP.NET Core and Entity Framework. It is also supported by a lot of other libraries as well.
