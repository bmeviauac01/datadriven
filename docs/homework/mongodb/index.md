# Exercise: MongoDB

This exercise is optional. You may earn **2+2 points** by completing this exercise.

Use GitHub Classroom to get your git repository at <https://classroom.github.com/a/ePdcRbph>. Clone your repository. It contains a skeleton and the expected structure of your submission. After completing the exercises and verifying them, commit and push your submission.

## Required tools

- Windows, Linux, or macOS: All tools are platform-independent, or a platform-independent alternative is available.
- MongoDB Community Server ([download](https://www.mongodb.com/download-center/community))
- Robo 3T ([download](https://robomongo.org/download))
- Microsoft Visual Studio 2019 [with the settings here](../VisualStudio.md)
    - When using Linux, or macOS, you can use Visual Studio Code, the .NET Core SDK, and [dotnet CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/).
- [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)
    - Usually installed with Visual Studio; if not, use the link above to install (the SDK and _not_ the runtime).
    - You need to install it manually when using Linux or macOS.
- Sample database initialization script: [mongo.js](https://raw.githubusercontent.com/bmeviauac01/adatvezerelt/master/docs/db/mongo.js)
    - Create and initialize the database; use the steps [the seminar exercises](../../seminar/mongodb/index.md) describe.
- GitHub account and a git client

## Exercise 0: Neptun code

Your very first task is to type your Neptun code into `neptun.txt` in the root of the repository.

## Exercise 1: Modify the tax percentage (2 points)

This exercise requires you to change the percentage of a value-added tax and update all related products. You need to implement the following method in class `ProductRepository`.

```csharp
public void ChangeVatPercentage(string name, int newPercentage)
```

1. Let us first examine where the tax-related data is in the database. Compared to a relational database, the VAT-related data is denormalized in MongoDB and resides in the `products` collection as an embedded document in each item.

    ![Embedded document](embedded-doc.png)

    This is also mirrored in the `Product` C# class.

    ```csharp
    public class Product
    {
        [BsonId]
        public ObjectId ID { get; set; }
        public ObjectId CategoryID { get; set; }

        public string Name { get; set; }
        public double? Price { get; set; }
        public int? Stock { get; set; }
        public VAT VAT { get; set; }
    }
    ```

    This makes working with products efficient as the product's total price can be calculated by taking the price and the embedded tax percentage (without the need to `JOIN` another data table, as it is required in a relational database).

    The disadvantage is though that an update to the tax has to change **all documents**.

1. From the description above, it follows that we need to change more than one document; hence we shall use an `UpdateMany` command to find and update all product records where the name in the `vat` field matches the parameter. Let us review how this method works.

    - `UpdateMany` has a `filter` parameter to specify which product records to update: where `VAT.Name` equals the value in the `name` parameter of the repository method.
    - The `update` parameter specifies the changes to make: change the `VAT.Percentage` to the value received as `newPercentage` parameter of the repository method. You should use a [$set](https://docs.mongodb.com/manual/reference/operator/update/set/) (`Set`) operator for this modification.

1. Implement the repository method. The repository class receives the database as a parameter and saves the collection as a local variable in the class; use this field to manipulate the collection.

There are unit tests available in the solution. You can [run the unit tests in Visual Studio](https://docs.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2019), or if you are using another IDE (e.g., VS Code or `dotnet cli`), then [run the tests using the cli](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test). You may update the database connection string in class `TestDbFactory` if needed.

!!! important "Tests"
    The tests presume that the database is in its initial state. Re-run the database initialization script to restore this state.

    Do **NOT** change the unit tests. You may temporarily alter the unit tests if you need to, but make sure to reset your changes before committing.

!!! example "SUBMISSION"
    Upload the changed C# source code.

    Create a screenshot that displays the **content of the `products` collection after the successful update**. If you execute the test, the _Standard Rate_ VAT percentage should change. Use Robo3T (or any other similar tool) to verify and show that the values have indeed changed. Make sure to expand a few documents to show the new values (just like the image above).

## Exercise 2: Product with the largest total value (2 points)

The task is to find the product that has the largest total value within a product category. The total value is the **price of the product multiplied by the amount of the product in stock**. You need to implement the following method in class `ProductRepository`.

```csharp
(string, double?) ProductWithLargestTotalValue(ObjectId categoryId)
```

1. Let us check the test related to this exercise in file `TestExercise2.cs` to understand what is expected here.

    - The method accepts a category name as an argument; products have to be filtered for this category.
    - The return value should be the name of the product (with the largest total value) and the total value itself.
    - If there are no products in the specified category, the return value should be `(null, null)`.

1. Use the aggregation pipeline of MongoDB. To see how this aggregation pipeline works, you can refer to the seminar material.

    You should build a pipeline consisting of the following stages:

    - Filter the products for the specified category. Use a [$match](https://docs.mongodb.com/manual/reference/operator/aggregation/match/) (`Match`) stage to specify the filter.

    - Calculate for each product the total value (multiply the price and the stock) using a [$project](https://docs.mongodb.com/manual/reference/operator/aggregation/project/) (`Project`) stage. Make sure to include the name of the product, you will need it for the final result.

    - Order the items based on this calculated total value descending. Use a [$sort](https://docs.mongodb.com/manual/reference/operator/aggregation/sort/) (`SortByDescending`) stage.

    - Since it is the largest value that we need, take the first item after sorting. Do not forget that there might not be any product in the specified category. Therefore you should use `FirstOrDefault` to fetch this item.

    !!! note ""
        If the syntax `(string, double?)` is unfamiliar:

        ```csharp
        return ("test", 0.0);
        ```
        
        The function will return with these two values.

You may test your implementation with the tests provided in class `TestExercise2`.

!!! example "SUBMISSION"
    Upload the changed C# source code.
