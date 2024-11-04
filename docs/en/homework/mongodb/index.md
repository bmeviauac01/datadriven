# 3. MongoDB

You may earn **4 points** by completing this exercise.

Use GitHub Classroom to get your git repository. You can find the **invitation link in Moodle**. Clone the repository created via the link. It contains a skeleton and the expected structure of your submission. After completing the exercises and verifying them, commit and push your submission.

Check the required software and tools [here](../index.md#required-tools).

Before you begin, create and initialize the database; use the steps [the seminar exercises](../../seminar/mongodb/index.md) describe.

## Exercise 0: Neptun code

Your very first task is to type your Neptun code into `neptun.txt` in the root of the repository.

## Exercise 1: Product with the largest total value (2 points)

The task is to find the product that has the largest total value within a product category. The total value is the **price of the product multiplied by the amount of the product in stock**. You need to implement the following method in class `ProductRepository`.

```csharp
(string, double?) ProductWithLargestTotalValue(ObjectId categoryId)
```

1. Let us check the test related to this exercise in file `TestExercise1.cs` to understand what is expected here.

    - The method accepts a category filter as an argument; products have to be filtered for this category.
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

1. Implement the repository method. The repository class receives the database as a parameter and saves the collection as a local variable in the class; use this field to manipulate the collection.

There are unit tests available in the solution. You can [run the unit tests in Visual Studio](https://docs.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2022), or if you are using another IDE (e.g., VS Code or `dotnet cli`), then [run the tests using the cli](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test). You may update the database connection string in class `TestDbFactory` if needed.

!!! important "Tests"
    The tests presume that the database is in its initial state. Re-run the database initialization script to restore this state.

    Do **NOT** change the unit tests. You may temporarily alter the unit tests if you need to, but make sure to reset your changes before committing.

!!! example "SUBMISSION"
    Upload the changed C# source code.

    You can run the tests in Visual Studio or using `dotnet cli`. Make sure that the screenshot includes the **source code of the repository** and the **test execution outcome**! Save the screenshot as `f1.png` and upload as part of your submission!

    If you are using `dotnet cli` to run the tests, make sure to display the test names too. Use the `-v n` command line switch to set detailed logging.

    The image does not need to show the exact same source code that you submit; there can be some minor changes. If the tests run successfully and you create the screenshot, then later you make some **minor** change to the source, there is no need for you to update the screenshot.

## Exercise 2: Insert a New Product (2 points)

The task is to create a function for inserting a new product. Several conditions must be met during the insertion, which need to be checked before the operation. Implement the `InsertProduct(string name, string category, int vat)` function.

The following conditions should be considered during insertion:

- If a product with the given name already exists, throw an exception (`ArgumentException`).
- If the specified category does not yet exist, throw an exception (`ArgumentException`).
- If the given `vat` value is already associated with the name of another `Product` instance, use that name; otherwise, assign the name `"VAT"` to the product being inserted.

1. Extend the repository class constructor to include categories. To do this, add the corresponding class for the category!

1. Implement the function, ensuring that the parameter constraints are checked before performing the insertion! The tests in the `TestExercise2.cs` file will assist you.

!!! example "SUBMISSION"
    Upload the modified C# source code!
    
    Additionally, take a screenshot similar to the one in the first task, where you have run the relevant tests! Save the image as `f2.png` and upload it as part of your solution!

## Exercise 3 optional: Estimating storage space (0 points)

The company is moving to a new location. We need to know whether the current stock can be moved and will fit into the new storage facility. Implement the method that calculates the **total volume of all products in stock**!

The products have the necessary information in `description.product.package_parameters`:

![Product size](product-size.png)

Use this to calculate the total volume of all items:

- Use the information from `package_parameters` (and **not** from `product_size`).
- A product might have multiple packages; this information is available in `package_parameters.number_of_packages`. This number shall be used as a multiplicator. Each product has a single size, and if it has multiple packages, then all packages are of the same size.
- The final total: for all products Σ (product stock * number of packages * width * height * depth).
- If a product does not have these information, it's volume should be calculated as 0.
- Mind, that the size also has a unit: either _cm_ or _m_, but the final value is expected in cubic meter.

Implement method `double GetAllProductsCumulativeVolume()` that returns a single scalar total of the volume in **cubic meters**. The calculation should be performed by the database (not in C#); use the aggregation pipeline.

!!! into "Sum aggregation"
    You will need the `$group` aggregation stage. Although, we do not need to group the products, still, this will allow us to aggregate all of them. Map each product into the same group (that is, in the `$group` stage the `id` should be a constant for all items), then use the projection part to perform a [`$sum`](https://docs.mongodb.com/manual/reference/operator/aggregation/sum/#use-in-group-stage) aggregation according to the formula above.

    Handling of the _cm_ and _m_ units can be solved with a conditional multiplication in the `sum`. If this does not work, you can use two aggregations: one for _cm_ and one for _m_ units, each with a filter in the pipeline then the aggregation afterwards.

The required parts of the products are not mapped to C# classes yet. You need to do this. Note, that the field names in the BSON do not conform to the usual syntax, thus, when mapping to C# properties, you have to take care of name the properties identically, or use the `[BsonElement(elementName: "...")]` attribute. Make sure you do not break your solution in excercise 2. You should modify product insert method with the following: product dimensions should be 1x1x1 cm, and for other unspecified variables, you may choose any valid value.

!!! warning "Use Fluent Api"
    You must use the C# Fluent Api! Do not write the query using `BsonDocument`!

You may test your implementation with the tests provided in class `TestExercise3`. The tests presume that the database is in its initial state.

!!! example "SUBMISSION"
    Upload the changed C# source code.
