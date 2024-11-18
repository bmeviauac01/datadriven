# 5. GraphQL

You may earn **4 points** by completing this exercise.

Use GitHub Classroom to get your git repository. You can find the **invitation link in Moodle**. Clone the repository created via the link. It contains a skeleton and the expected structure of your submission. After completing the exercises and verifying them, commit and push your submission.

Check the required software and tools [here](../index.md#required-tools).

As preparation, create a new database as described in the [practical material](../../seminar/mssql/index.md).

## Exercise 0: Neptun code

Your very first task is to type your Neptun code into `neptun.txt` in the root of the repository.

## Exercise 1: Completing the Project and Queries (2 points)

The goal of this homework is to gain practical experience with GraphQL by creating your own API.  
GraphQL is a powerful tool that enables querying and modifying data in a single, well-structured request, making API usage more flexible and efficient.  
The operation of a GraphQL API differs somewhat from traditional REST APIs: at the endpoint, clients can specify exactly what data they want to receive using flexible, controlled queries.  
The purpose of this exercise is to demonstrate how complex data can be accessed in a simple manner.

In this homework, the familiar data model will be used, with queries involving orders, products, categories, and related information.  
The data model, DBContext, and entities are already available in the starter project.  

In the first task, we will explore how to add GraphQL endpoints to an existing project using the Hot Chocolate server-side library.  
To do this, we will first add the necessary packages, expose endpoints, and create the class that will return the required data.

1. Create a `Query` class and add a `GetProducts` function with an `AdatvezDbContext` parameter. This function will serve as the `products` query's endpoint. The DBContext parameter will be injected by the DI container when the method is called. Implement the method to return the products with their categories.

1. Add the following NuGet packages with the versions specified in parentheses:

- HotChocolate.AspNetCore (14.1.0)
- HotChocolate.Data (14.1.0)
- HotChocolate.Data.EntityFramework (14.1.0)

1. In the entry point of the application (`Program.cs`), register the GraphQLServer service using the following code:  
(here, three things happen: the GraphQLServer service is registered, the `AdatvezDbContext` is registered for injection, and the `Query` class is registered to serve as the query endpoints.)

```csharp
builder.Services
    .AddGraphQLServer()
    .RegisterDbContextFactory<AdatvezDbContext>()
    .AddQueryType<Query>();
```

1. Add routing (the ability to handle endpoints) and create a default endpoint using the following lines of code:

```csharp
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL();
});
```

1. Start the application and navigate to `http://localhost:5000/graphql/`.
Here, you will see an interactive tool called Banana Cake Pop, which allows you to run queries and mutations, view the schema, and browse the documentation for the API. 
In the queries tab, you can test the execution of the above function with the following code:

```json
query {
    products {
        name
        category {
            name
        }
    }
}
```

!!! note ""
    Let's examine what happens:

    - When Hot Chocolate or another GraphQL server receives a request, each field is handled by a resolver, which is responsible for fetching or generating the corresponding data.

    - The `products` field is implicitly linked to the `GetProducts` method, and Hot Chocolate automatically connects them.

    - The integration of Hot Chocolate with Entity Framework (EF) allows the GraphQL API to directly query data from the database via EF, simplifying the implementation and operation of GraphQL endpoints.

1. Based on the previous task, create a query that can return products after applying a filter. In the `Query` class, add a `ProductsByCategory` function, which has a `categoryName` parameter of type `string` in addition to the `AdatvezDbContext` parameter. In the implementation, filter by the category name: return those products whose category name matches the provided parameter. Use the following query to test it:


```json
query {
    productsByCategory(categoryName: "Months 0-6") {
        id
        name
        price
    }
}
```

1. In the next task, we will see the power of GraphQL. When information needs to be gathered from multiple tables, a lot of unnecessary data could otherwise pass through the network. In contrast, if we know exactly what is needed, we can query and send only the required data. Create a query that returns orders and gathers the following information:

   - The name of the customer  

   - The names of the products in the order  

   - The categories of the products  

   - The quantity ordered for each product  

```json
query {
    orders {
        customerSite {
            customer {
                name
            }
        }
        orderItems {
            amount
            product
            {
                name
                category
                {
                    name
                }
            }
        }
    }
}
```
1. Query Optimization: In the console that opens when the application starts, you can also view the query generated by Hot Chocolate using Entity Framework. You will see that the `SELECT` statement retrieves many fields that are not necessary, even though we explicitly specified the required fields in the GraphQL query.  
   For functions annotated with `[UseProjection]`, Hot Chocolate transforms the incoming queries directly for the database. To use this feature, two changes are required in addition to adding the annotation. First, the return type of the function should be `IQueryable<Order>`. The second change is to register the availability of projection with the following call:


```csharp
builder.Services
    .AddGraphQLServer()
    .RegisterDbContextFactory<AdatvezDbContext>()
    .AddQueryType<Query>()
    //új sor:
    .AddProjections(); 
```

Look at the change in the SQL query in the console. The implementation can also be reduced, as explicit `Include` calls will no longer be necessary from this point on.

!!! example "SUBMISSION"
    Upload the modified C# source code.

    Additionally, take a screenshot of the query and response created in the Banana Cake Pop interface, showing the required data. Save the screenshot as `f1.png` and include it as part of your solution!

## Exercise 2: Data manipulation using GrapQL (2 points)

GraphQL is not only used for querying data but also for modifications. In the following tasks, the goal is to practice data manipulation.  
Data manipulation is performed by a _Mutation_, which is an operation that modifies data, such as creating, updating, or deleting records in the database.

### Modifying a Product

In the first data manipulation task, you will change the prices of existing products. Increase the price for products that belong to the category provided in the parameters.

1. Create a `Mutation` class named `ProductMutation`.

1. Register the service using the `.AddMutationType<ProductMutation>()` method, similar to how the `Query` service was registered in the first exercise.

1. The `ProductMutation` class should have a function `IncreaseProductPricesByCategory`, which will make the necessary modifications and return an `IQueryable<Product>`. This will show the new prices. The function should have three parameters: 1) the `AdatvezDBContext`, 2) a `categoryName` string, and 3) a `priceIncrease` double value, which indicates how much the price of the product should increase. The return value should be the collection of modified products. Ensure that the changes are also applied to the database.

1. You can test it with the following example query (the parameter names should match those specified in the example call below: *categoryName*/*priceIncrease*):

```json
mutation {
    increaseProductPricesByCategory(categoryName: "LEGO", priceIncrease: 1.1) {
        name
        price
        category {
                name
            }
        }
}
```

    !!! note ""
        The query consists of two parts. The first is the mutation call with the appropriate parameters:

        ```json
        increaseProductPricesByCategory(categoryName: "LEGO", priceIncrease: 1.1)
        ```

        The second part specifies the format in which we want to see the data after the call. This follows the syntax seen in the previous queries. In this case, for example, we want to display the modified product names, prices, and categories:

        ```json
        {
            name
            price
            category {
                name
            }
        }
        ```

### Creating an Order

In the next task, we will add the ability for the server to insert a new order.
The mutation will only expect the product names and their desired quantities as parameters and will create the order accordingly.

1. Add a function to the `ProductMutation` class named `CreateOrder`. In addition to the usual `AdatvezDbContext`, it should have two parameters: the first one is a string list for product names named `productNames`, and the second is an integer list for quantities named `quantities`.

1. Inside the function, create a new `Order` instance. For each product name in the provided parameters, create a new `OrderItem`. For each `OrderItem`, find the corresponding `Product` by its name and set the appropriate quantity (you can ignore other properties). The function should return the newly created `Order` instance.

1. The function should throw an error if the number of products does not match the number of quantities, or if any of the product names cannot be found in the database.

1. You can test it with the following query. The query will insert an order for two products, and then print the created order in the following structure:

```json
mutation {
    createOrder(
        productNames: ["Lego City harbour", "Activity playgim"],
        quantities: [2, 4]
    ) {
        id
        orderItems {
        product {
            name
        }
        amount
        }
    }
}
```

Example "SUBMISSION"

    Upload the modified C# source code.

    Additionally, take a screenshot where you have executed the modifications using the Banana Cake Pop interface. Save the image as `f2.png` and include it as part of your solution!

## Exercise 3 optional: Advanced GraphQL functions (0 points)

!!! note ""
    In the evaluation, you will see the text “imsc” in the exercise title; this is meant for the Hungarian students. Please ignore that.

In the following tasks, we will utilize more advanced features provided by the Hot Chocolate GraphQL server, such as filtering, sorting, and pagination.

To use Hot Chocolate's built-in filtering capabilities, we need to call `AddFiltering()` when registering the `Service`, and then apply the `[UseFiltering]` attribute to the method that serves the desired endpoint.

In the following tasks, you will need to come up with the query and the corresponding .NET implementation. These should be submitted as part of the solution.

1. In the first task, the `productsByCategory` call can be replaced by enabling the built-in filtering for our `GetProducts` function. 
   Do this, and test it with a query that filters by category names.
   The code completion will be very helpful in creating the query.
   When submitting the solution, the query should be submitted in the `q3_1.txt` file, where you need to filter by the `"Building Items"` category.

    !!! note ""
        Filtering has a unique syntax.
        In the query, after the desired element, we specify the filtering conditions using a `where` expression.
        Here, according to the schema, we need to specify which values we want to filter by, whether we want to filter using combined conditions, etc.

        ```json
        query {
            products (where .. ) {
                name
                category {
                    name
                }
            }
        }
        ```
        The great advantage of this approach is that we do not need to implement all possible filtering scenarios that could arise among API users.
        Instead, it is left to the users to specify how they want to filter, and the GraphQL language supports this. Hot Chocolate will generate the response filtered based on the format and conditions requested by the client.
        
        Furthermore, it is possible to create custom filtering conditions, which you can read more about here: https://chillicream.com/docs/hotchocolate/v14/fetching-data/filtering

1. Your second task is to add sorting and pagination to the `GetOrders` function. Annotate the `GetOrders` function with the appropriate attributes, then set up the sorting and pagination options when registering GraphQL. To test it, you will need to modify the query, and constructing the query is also part of the task.
When submitting the solution, you must also submit the query in the `q3_2.txt` file, where the query contains 2 `Order` items (due to pagination) and sorts them in descending order by `id`.

!!! note ""
    Pagination is a very important feature when querying large databases, as sending and processing entire tables is not ideal. Instead, a common solution is that the client requests data in batches, for example, 10 items at a time, and only requests the next batch when navigating to the next page.

    The paginated results do not come back as lists of the entities, but as so-called collections. You can read more about them here: https://chillicream.com/docs/hotchocolate/v14/fetching-data/pagination

    The documentation for sorting can be found here: https://chillicream.com/docs/hotchocolate/v14/fetching-data/sorting

!!! example "SUBMISSION"
    Upload the modified C# source code.
    Save the queries from parts 1 and 2 of task 3 as `q3_1.txt` and `q3_2.txt`, place them in the root folder along with the images, and submit them.

    Additionally, take a screenshot where the relevant queries are run. Save the screenshot as `f3.png` and submit it as part of your solution!
