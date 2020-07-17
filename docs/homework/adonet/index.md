# Exercise: ADO.NET data access

This exercise is optional. You may earn **2+2 points** by completing this exercise.

Use GitHub Classroom to get your personal git repository at <TBD>. Clone your repository. It contains a skeleton and the expected structure of your submission. After completing the exercises and verifying them commit and push your submission.

## Required tools

- Windows, Linux or MacOS: All tools are platform-independent, or a platform-independent alternative is available.
- Microsoft SQL Server
    - The free Express version is sufficient, or you may also use _localdb_ installed with Visual Studio
    - A [Linux version](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup) is also available.
    - On MacOS you can use Docker.
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms), or you may also use the platform-independent [Azure Data Studio](https://docs.microsoft.com/en-us/sql/azure-data-studio/download) is
- Database initialization script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/adatvezerelt/master/docs/db/mssql.sql)
- Microsoft Visual Studio 2019 [with the settings here](../VisualStudio.md)
    - When using Linux or MacOS you can use Visual Studio Code, the .NET Core SDK and [dotnet CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/).
- [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)
    - Usually installed with Visual Studio; if not, use the link above to install (the SDK and _not_ the runtime).
    - You need to install it manually when using Linux or MacOS.
- GitHub account and a git client

## Exercise 0: Neptun code

Your very first task is to type your Neptun code into `neptun.txt` in the root of the repository.

## Exercise 1: Product repository (2 points)

Create _repository_ class for managing the `Product` entities; use **ADO.NET Connection** technology. Open the _sln_ file from checked out folder using Visual Studio. Find classes `Repository.ProductRepository` and `Model.Product`. Implement the following methods of class `ProductRepository`:

- `Search(string name)`: find all products in the database matching the provided name, and return them as C# objects. If the name filter argument is `null`, the method should return all products; otherwise it should match names that contain the specified string in a _case-insensitive_ manner!
- `FindById(int id)`: returns a single product matched by the ID, or returns `null` if not found.
- `Update(Product p)` updates the properties of a product in the database based on the values received as parameter. Update the `Name`, `Price` and `Stock` values, and you may ignore the rest.

You should mind the following requirements:

- Only make changes to class `ProductRepository`!
- In the repository code open the ADO.NET connection using the connection string in field `connectionString` (and do **not** use `TestConnectionStringHelper` here).
- You need to find the tax percentage of the product too. In the returned instance of `Model.Product` you must include the percentage of the referenced `VAT` record, and not the ID of this VAT record! The name of the category of the product has to be retrieved similarly.
- You may only use ADO.NET.
- You must prohibit SQL injection.
- Make no changes to `Model.Product` in this exercise!
- Do not change the definition of class `ProductRepository` (do not change the name of the class, nor the constructor or method declarations); only write the method bodies.

There are unit tests available in the solution. You can [run the unit tests in Visual Studio](https://docs.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2019), or if you are using another IDE (e.g. VS Code and/or `dotnet cli`), then [run the tests using the cli](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test). You may update the database connection string in class `TestConnectionStringHelper` if needed.

!!! important "Tests"
    The tests presume that the database is in its initial state. Re-run the database initialization script to restore this state.

    Do **NOT** change the unit tests. You may temporarily alter the unit tests if you need to, but make sure to reset your changes before committing.

!!! example "SUBMISSION"
    Upload the changed C# source code.

    Create a screenshot displaying the successfully executed unit tests. You can run the tests in Visual Studio or using `dotnet cli`. Make sure that the screenshot includes the **source code of the repository** (as much as you can fit on the screenshot), and the **test execution outcome**! Save the screenshot as `f1.png` and upload as part of your submission!

    If you are using `dotnet cli` to run the tests, make sure to display the test names too. Use the `-v n` command line switch to set detailed logging.

    It is not necessary for the image to show the exact same source code that you actually submit, there can be some minor changes here and there. That is, if the tests run successfully and you create the screenshot, then later you make some **minor** change to the source, there is no need for you to update the screenshot.

## Exercise 2: Optimistic concurrency handling (2 points)

!!! note ""
    In the evaluation you will see the text “imsc” in the exercise title; this is meant for the Hungarian students. Please ignore that.

When updating a product in the database, the code shall identify and prohibit overwriting a previously unseen modification. Implement this behavior in `ProductRepository.UpdateWithConcurrencyCheck`. This method shall deny the update if it discovers a _lost update_ concurrency issue.

The specific sequence of events that we want to prohibit:

1. User _A_ queries a product.
1. User _B_ fetches the same product.
1. User _A_ changes a property, such as the price, then updates the database with this change.
1. User _B_ makes a change to the product properties (either the price property, or another one), and overwrites the changes made by user _A_ without noticing it.

!!! tip "Optimistic concurrency handling"
    Use the technique of optimistic concurrency handling to resolve this issue. You must not use transactions here, since the query and the update of the data happens over a longer time period without maintaining a database connection. Implement method `ProductRepository.UpdateWithConcurrencyCheck`, and also update `Model.Product` as needed. You may **not** add any new columns to the database.
You should mind the following requirements:

- Only make changes to method `ProductRepository.UpdateWithConcurrencyCheck` and class `Model.Product`!
- The method shall indicate as return value whether the change was saved (that it, it discovered no concurrency issues).
- You may only use ADO.NET.
- You must prohibit SQL injection.
- Do not change the definition of class `ProductRepository` (do not change the name of the class, nor the constructor or method declarations); only write the single method body.
- Do not change the constructor signature of class `Model.Product` (number, order or names of the parameters), but you may change the body. Do not alter any existing properties of the class, but you can add new ones.

!!! example "SUBMISSION"
    Upload the changed C# source code. Explain the behavior in a C# comment in method `UpdateWithConcurrencyCheck` (in 2-3 sentences).
