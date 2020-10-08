# Exercise: MSSQL server-side programming

This exercise is optional. You may earn **2+2 points** by completing this exercise.

Use GitHub Classroom to get your git repository at <https://classroom.github.com/a/CEXFRiAu>. Clone your repository. It contains a skeleton and the expected structure of your submission. After completing the exercises and verifying them, commit and push your submission.

## Required tools

- Windows, Linux, or macOS: All tools are platform-independent, or a platform-independent alternative is available.
- Microsoft SQL Server
    - The free Express version is sufficient, or you may also use _localdb_ installed with Visual Studio
    - A [Linux version](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup) is also available.
    - On macOS, you can use Docker.
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms), or you may also use the platform-independent [Azure Data Studio](https://docs.microsoft.com/en-us/sql/azure-data-studio/download) is
- Database initialization script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/adatvezerelt/master/docs/db/mssql.sql)
- GitHub account and a git client

## Prepare the database

Create a new database with a name that matches **your Neptun code**. Run the database initialization script to create the tables in this database.

!!! warning "Neptun code is important"
    The exercise will ask you for a screenshot that must contain the database name with your Neptun code!

## Exercise 0: Neptun code

Your very first task is to type your Neptun code into `neptun.txt` in the root of the repository.

## Exercise 1: Password expiry maintenance (2 points)

Due to security reasons, we would like to enforce password expiry. For this, we will record the date when the password was last updated.

1. Add a new column to the `Customer` table with the name `PasswordExpiry` storing a date: `alter table [Customer] add [PasswordExpiry] datetime`.

1. Create a trigger that automatically fills the `PasswordExpiry` date column when the password value is updated. The new value should be the current date plus one year. The trigger shall calculate the value. When a new Customer is registered (inserted into the table), the column should always be populated automatically. However, when data is updated, only update the date if the password is changed. (E.g. if only the address is altered, the date should not be updated.) The trigger should only update the date for the inserted/modified records (it should not set it for all records in the table)!

Make sure to verify the behavior of the trigger under various circumstances.

!!! example "SUBMISSION"
    Submit the code of the trigger in file `f1.sql`. This sql file should contain a single statement (a single `create trigger` command) without any `use` or `go` commands.

    Create a screenshot that displays sample records in the `Customer` table with the automatically populated date values. Make sure that the database name and your Neptun code are visible on the screenshot. Save the screenshot as `f1.png` and upload it as part of your submission!

## Exercise 2: Product recommended age (2 points)

!!! note ""
    In the evaluation, you will see the text “imsc” in the exercise title; this is meant for the Hungarian students. Please ignore that.

The database contains an xml column with the name `Description` in the `Product` table. This column has values for some of the records.

An example for the content is below:

```xml hl_lines="9"
<product>
  <product_size>
    <unit>cm</unit>
    <width>150</width>
    <height>50</height>
    <depth>150</depth>
  </product_size>
  <description>Requires battery (not part of the package).</description>
  <recommended_age>0-18 m</recommended_age>
</product>
```

We want to extract the `recommended_age` and move it to a new column in the table.

1. Add a new column to the `Product` table with name `RecommendedAge` storing a text: `alter table [Product] add [RecommendedAge] nvarchar(200)`.

1. Create a T-SQL script that extracts the content of the `<recommended_age>` tag from the xml and moves the value into the `RecommendedAge` column of the table. If the xml description is empty or there is no `<recommended_age>` tag, the column's value should be `NULL`. Otherwise, take the tag's text content (without the tag name), copy the value into the column, and remove the tag from the xml. You can presume that there is at most one `<recommended_age>` element in the xml.

!!! example "SUBMISSION"
    Submit the T-SQL code in file `f2.sql`. Do not use a stored procedure in this exercise; create a simple T-SQL code block. This sql file should be executable by itself and should not contain any `use` or `go` commands.

    Create a screenshot that displays the content of the `Product` table after running the script. The new column and the populated values should be visible on the screenshot. Make sure that the database name and your Neptun code are visible on the screenshot. Save the screenshot as `f2.png` and upload it as part of your submission!
