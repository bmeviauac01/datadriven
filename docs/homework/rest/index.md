# Exercise: REST API and Web API

This exercise is optional. You may earn **2+2 points** by completing this exercise.

Use GitHub Classroom to get your personal git repository at <https://classroom.github.com/a/h3EDX6Bu>. Clone your repository. It contains a skeleton and the expected structure of your submission. After completing the exercises and verifying them commit and push your submission.

## Required tools

- Windows, Linux or MacOS: All tools are platform-independent, or a platform-independent alternative is available.
- Microsoft Visual Studio 2019 [with the settings here](../VisualStudio.md)
    - When using Linux or MacOS you can use Visual Studio Code, the .NET Core SDK and [dotnet CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/).
- [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)
    - Usually installed with Visual Studio; if not, use the link above to install (the SDK and _not_ the runtime).
    - You need to install it manually when using Linux or MacOS.
- [Postman](https://www.getpostman.com/)
- GitHub account and a git client

## Exercise 0: Neptun code

Your very first task is to type your Neptun code into `neptun.txt` in the root of the repository.

## Exercise 1: Operations on products (2 points)

The repository you cloned contains a skeleton application. Open the provided Visual Studio solution and start the application. A console window should appear that hosts the web application. While the web app is running test it: open a browser to <http://localhost:5000/api/product>. The page should display a list of products in JSON format.

Check the source code.

- `Startup.cs` initializes your application. This is an ASP.NET Core web application.
- There is no database used in this project to make things simpler. Class `ProductRepository` contains hard-wired data used for testing.
- `ProductsController` uses _dependency injection_ to instantiate  `IProductRepository`.

Exercises:

1. In class `DAL.ProductRepository` edit the field `Neptun` and add your Neptun code here. The string should contain the 6 characters of your Neptun code.

    !!! warning "IMPORTANT"
        The data altered this way will be displayed on a screenshot in a later exercise, hence this step is important.

1. Create a new API endpoint for verifying whether a particular product specified by its ID exists. This new endpoint should respond to a `HEAD` HTTP query on URL `/api/product/{id}`. The HTTP response should be status code 200 or 404 (without any body either case).

1. Create a new API endpoints that returns a single `Product` specified by its ID; the query is a `GET` query on URL `/api/product/{id}` and the response should be either 200 OK with the product as body, or 404 when the product is not found.

1. Create a new API endpoint for querying the total number of products. (Such an endpoint could be used, for example, by the UI for paging the list of products.) This should be a GET HTTP request to URL `/api/product/-/count`. The returned result should be a JSON serialized `CountResult` object with the correct count.

    ??? question "Why is there a `/-` in the URL?"
        In order to understand the need for this, let us consider what the URL should look like: we are querying products, so `/api/product` is the prefix, but what is the end of the URL? It could be `/api/product/count`. However, this clashes with `/api/product/123` where we can get a particular product. In practice, the two URLs could work side-by-side here, since the product ID is an integer and the framework would recognize that an URL ending in `/123` is to get a product and the `/count` is to get the counts. But this works only as long as the ID is an integer. If the product ID would be a string, this would be more problematic. Our solution makes sure that the URLs do not clash. The `/-` is to indicate that there is _no_ product ID.

        Note: the way URLs are matched to controller methods is more complicated actually. ASP.NET Core has a notion of priorities when trying to find a controller method for an URL. This priority can be modified on the [`[Http*]` attributes by setting the `Order` property](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.httpgetattribute?view=aspnetcore-3.1#properties).

!!! example "SUBMISSION"
    Upload the changed source code.

    Create a screenshot from Postman (or any alternative tool you used for testing) that shows a successful query that fetches an existing product. The screenshot should display both the request and response with all information (request type, URL, response code, response body). Save the screenshot as `f1.png` and upload as part of your submission! The response body must contain your **Neptun code**.

## Exercise 2: OpenAPI documentation (2 points)

!!! note ""
    In the evaluation you will see the text “imsc” in the exercise title; this is meant for the Hungarian students. Please ignore that.

OpenAPI (formerly Swagger) is a REST API documentation tool. It is similar to the WSDL for Web Services. Its goal is to describe the API in a standardized format. In this exercise you are required to create a [OpenAPI specification and documentation](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-3.1) for your REST API.

1. Please follow the official Microsoft tutorial at: <https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-3.1&tabs=visual-studio>

    - Make sure to use **Swashbuckle**.
    - The `swagger.json` should be generated by your application (you need not write it), and it should be available at URL `/swagger/v1/swagger.json`.
    - Set up _Swagger UI_, which should be available at URL `/neptun`. To achieve this, when configuring `UseSwaggerUI` set the `RoutePrefix` as your Neptun code **all lower-case**.
    - (You can ignore the "Customize and extend" parts in the tutorial.)

1. When ready, start the web application and check `swagger.json` at URL <http://localhost:5000/swagger/v1/swagger.json>, then open SwaggerUI too at <http://localhost:5000/neptun>.

1. Test the “Try it out” in SwaggerUI: it will send out the query that your backend will serve.

    ![SwaggerUI Try it out](swaggerui-try.png)

!!! example "SUBMISSION"
    Upload the changed source code. Make sure to upload the altered `csproj` file too; it contains a new NuGet package added in this exercise.

    Create a screenshot of SwaggerUI open in the browser. Make sure that the URL is visible and that it contains `/neptun` with your Neptun code. Save the screenshot as `f2.png` and upload as part of your submission!
