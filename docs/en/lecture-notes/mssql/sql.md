# SQL language, MSSQL platform-specific SQL

You can run these queries on the [sample database](../../db/index.md).

## Simple queries

Which product costs less than 2000 and have less than 50 in stock?

```sql
SELECT Name, Price, Stock
FROM Product
WHERE Price<2000 AND Stock<50
```

Which product has no description?

```sql
SELECT *
FROM Product
WHERE Description IS NULL
```

## Joining tables

Customers with a main site in Budapest (the two alternatives are equivalent).

```sql
SELECT *
FROM Customer c, CustomerSite s
WHERE c.MainCustomerSiteID=s.ID AND City='Budapest'

SELECT *
FROM Customer c INNER JOIN CustomerSite s ON c.MainCustomerSiteID=s.ID
WHERE City='Budapest'
```

List the products that start with letter M, the ordered amounts and deadlines. Include the products that have not been ordered yet.

```sql
SELECT p.Name, SUM(oi.Amount)
FROM Product p
     LEFT OUTER JOIN OrderItem oi ON p.id=oi.ProductID
WHERE p.Name LIKE 'M%'
GROUP BY p.Name
```

## Sorting

```sql
SELECT *
FROM Product
ORDER BY Name
```

Microsoft SQL Server specific: _collation_ specifies the rules for sorting

```sql
SELECT *
FROM Product
ORDER BY Name collate SQL_Latin1_General_Cp1_CI_AI
```

Sort by multiple fields

```sql
SELECT *
FROM Product
ORDER BY Stock DESC, Price
```

## Subqueries

List the order dates, deadlines and Statuses

```sql
SELECT o.Date, o.Deadline, s.Name
FROM [Order] o INNER JOIN Status s ON o.StatusId=s.ID
```

An alternative, but the two are not equivalent: the subquery is the equivalent of the left outer join and not the inner join!

```sql
SELECT o.Date, o.Deadline,
       (SELECT s.Name
        FROM Status s
        WHERE o.StatusId=s.ID)
FROM [Order] o
```

!!! info "`[Order]`"
    `[Order]` is in brackets, because this signals that this is a table name and not the beginning of the `order by` SQL language element.

## Filter duplicates

Which products have been ordered in batches of more than 3? One product may have been ordered multiple times, but we want the name only once.

```sql
SELECT DISTINCT p.Name
FROM Product p INNER JOIN OrderItem oi ON oi.ProductID=p.ID
WHERE oi.Amount>3
```

## Aggregate functions

How much is the most expensive product?

```sql
SELECT MAX(Price)
FROM Product
```

Which are the most expensive products?

```sql
SELECT *
FROM Product
WHERE Price=(SELECT MAX(Price) FROM Product)
```

What was the min, max and average selling price of each product with name containing _Lego_ having an average selling price more than 10000

```sql
SELECT p.Id, p.Name, MIN(oi.Price), MAX(oi.Price), SUM(oi.Price*oi.Amount)/SUM(oi.Amount)
FROM Product p
     INNER JOIN OrderItem oi ON p.ID=oi.ProductID
WHERE p.Name LIKE '%Lego%'
GROUP BY p.Id, p.Name
HAVING AVG(oi.Price)>10000
ORDER BY 2
```

## Inserting records

Inserting a single record by assigning value to all columns (except _identity_)

```sql
INSERT INTO Product
VALUES ('aa', 100, 0, 3, 2, NULL)
```

Set values of selected columns only

```sql
INSERT INTO Product (Name,Price)
VALUES ('aa', 100)
```

Insert the result of a query

```sql
INSERT INTO Product (Name, Price)
SELECT Name, Price
FROM InvoiceItem
WHERE Amount>2
```

MSSQL specific: identity column

```sql
CREATE TABLE VAT
(
   ID int IDENTITY PRIMARY KEY,
   Percentage int
)

INSERT INTO VAT(Percentage)
VALUES (27)

SELECT @@IDENTITY
```

MSSQL specific: setting the value of _identity_ column

```sql
SET identity_insert VAT ON

INSERT INTO VAT (ID, Percentage)
VALUES (123, 27)

SET identity_insert VAT off
```

## Updating records

Raise the price of LEGOs by 10% and add 5 to stock

```sql
UPDATE Product
SET Price=1.1*Price,
    Stock=Stock+5
WHERE Name LIKE '%Lego%'
```

Update based on filtering by referenced table content: raise the price by 10% for those products that are subject to 20% VAT, and have more then 10 pcs in stock

```sql
UPDATE Product
SET Price=1.1*Price
WHERE Stock>10
AND VATID IN
(
    SELECT ID
    FROM VAT
    WHERE Percentage=20
)
```

MSSQL Server specific solution to the same task

```sql
UPDATE Product
SET Price=1.1*Price
FROM Product p
     INNER JOIN VAT v ON p.VATID=v.ID
WHERE Stock>10
      AND Percentage=20
```

## Deleting records

```sql
DELETE
FROM Product
WHERE ID>10
```

## Assigning ranks

Assigning ranks by ordering

```sql
SELECT p.*,
       rank() over (ORDER BY Name) AS r,
       dense_rank() over (ORDER BY Name) AS dr
FROM Product p
```

Ranking within groups

```sql
SELECT p.*
       ,rank() over (partition BY CategoryID ORDER BY Name) AS r
       ,dense_rank() over (partition BY CategoryID ORDER BY Name) AS dr
FROM Product p
```

!!! example "Rank and dense_rank"
     Unlike dense_rank , Rank skips positions after equal rankings. The number of positions skipped depends on how many rows had an identical ranking. For example, Mary and Lisa sold the same number of products and are both ranked as 1. With Rank,  the next position is 3; with dense_rank, the next position is 2.

## CTE (Common Table Expression)

Motivation: subqueries often make queries complex

First three products sorted by name alphabetically

```sql
SELECT *
FROM
(
    SELECT p.*
            ,rank() over (ORDER BY Name) AS r
            ,dense_rank() over (ORDER BY Name) AS dr
    FROM Product p
) a
WHERE a.dr<=3
```

Same solution using CTE

```sql
WITH q1
AS
(
    SELECT *
           ,rank() over (ORDER BY Name) AS r
          ,dense_rank() over (ORDER BY Name) AS dr
    FROM Product
)
SELECT *
FROM q1
WHERE q1.dr<=3
```

How many pieces have been sold from the second most expensive product?

```sql
WITH q
AS
(
    SELECT *
            , dense_rank() over (ORDER BY Price DESC) dr
    FROM Product
)
SELECT q.ID, q.Name, SUM(Amount)
FROM q
     INNER JOIN OrderItem oi ON oi.ProductID=q.ID
WHERE q.dr = 2
GROUP BY q.ID, q.Name
```

Paging: list products alphabetically from 3. to 8. record

```sql
WITH q
AS
(
    SELECT *
            , rank() over (ORDER BY Name) r
    FROM Product
)
SELECT *
FROM q
WHERE q.r BETWEEN 3 AND 8
```

Paging using MSSQL Server (2012+) specific syntax

```sql
SELECT *
FROM Product
ORDER BY Name
offset 2 rows
fetch next 6 rows only

SELECT TOP 3 *
FROM Product
ORDER BY Name
```

## Querying XML documents

In a relational database, in addition to relational data, semi-structured data (e.g., XML) can also be stored - but relational is the main content. For example, in the [sample database](../../db/index.md), the `Description` field of the `Product` table is XML.

### XPath

An XML document has a tree structure. The [**XPath**](https://www.w3schools.com/xml/xpath_intro.asp) language allows navigating this tree and selecting specific content. The following table illustrates the capabilities of the _XPath_ language.

| **XPath expression**        | **Meaning**                                                      |
| --------------------------- | ---------------------------------------------------------------- |
| tagname                     | Node with specified name                                         |
| /                           | Search starts from the root                                      |
| //                          | In a descendend at any level                                     |
| .                           | Current node                                                     |
| ..                          | Parent node                                                      |
| @name                       | Specific attribute                                               |
| /library/book[k]            | The book at index k within the library node (indexes start at 1) |
| /library/book[last()]       | Last child                                                       |
| /library/book[position()<k] | The first k-1 child nodes                                        |
| //title[@lang="hu"]         | Title elements that have lang attribute with value "hu"          |
| //title[text()]             | The text content of the title nodes                              |
| /library/book[price>5000]   | Books within the library node that have a price more than 5000   |

!!! note "XQuery and XPath"
    _XPath_ has many other capabilities in addition to the ones above, including expressing more complex queries.

    In the following examples, we will specify the data to be queried using [_XQuery_](https://docs.microsoft.com/en-us/sql/t-sql/xml/xml-data-modification-language-xml-dml). _XQuery_ builds on _XPath_ and adds additional functionality. Both _XPath_ and _XQuery_ are platform-independent languages ​​based on W3C standards.

### Queries

Let us have a table with an XML column. In addition to querying the entire XML value, we can query content from within the XML document. In order to do this, we need to use T-SQL functions capable of working on the XML content: [`query(XQuery)`](https://docs.microsoft.com/en-us/sql/t-sql/xml/query-method-xml-data-type) , [`value(XQuery, SQLType)`](https://docs.microsoft.com/en-us/sql/t-sql/xml/value-method-xml-data-type) and [`exist(XQuery)`](https://docs.microsoft.com/en-us/sql/t-sql/xml/exist-method-xml-data-type). Let's look at a few examples of these.

Let us query how many packages the products consist of.

```sql
SELECT Description.query('/product/package_parameters/number_of_packages')
FROM Product
```

For example, this could yield:

```xml
<number_of_packages>1</number_of_packages>
```

The function `query()` returns XML; if it is only the value that is needed, we can use the `value()` function. The `value()` function must also specify the type of data queried as a string literal.

```sql
SELECT Description.value('(/product/package_parameters/number_of_packages)[1]', 'int')
FROM Product
```

The result will be 1.

!!! info "SQLType"
    The type passed as a parameter cannot be xml. Conversion to the specified type is performed with the T-SQL [`CONVERT`](https://docs.microsoft.com/en-us/sql/t-sql/functions/cast-and-convert-transact-sql) function.

Let us query the names of the recommended products for ages 0-18 months.

```sql
SELECT Name
FROM Product
WHERE Description.exist('(/product)[(./recommended_age)[1] eq "0-18 m"]')=1
```

Function `exist()` returns 1 if the _XQuery_ expression evaluation yields a non-empty result; or 0 if the query result is empty.

We can also use the `value()` method instead of `exist()` here.

```sql
SELECT Name
FROM Product
WHERE Description.value('(/product/recommended_age)[1]', 'varchar(MAX)')='0-18 m'
```

### Manipulating queries

We can not only query XML data, but also modify it in place. The modification in the database is performed in an atomic way, i.e., there is no need to fetch the XML into a client application, modify it and then write it back. Instead, following the philosophy of server-side programming, we bring the logic (here: modification) into the database. Data modification queries can be performed with the [`modify(XML_DML)`](https://docs.microsoft.com/en-us/sql/t-sql/xml/modify-method-xml-data-type) function, where we use the so-called [XML DML](https://docs.microsoft.com/en-us/sql/t-sql/xml/xml-data-modification-language-xml-dml) language to describe the desired change. Let's look at a few examples.

In the product called Lego City harbor, let us change the recommended age to 6-99 years.

```sql
UPDATE Product
SET Description.modify(
'replace value of (/product/recommended_age/text())[1]
WITH "6-99 y"')
WHERE Name='Lego City harbour'
```

The XML DML expression consists of two parts: in the first part (`replace value of`) the element to be modified is selected; in the second part (`with`) the new value is specified. Only one element can be modified within an XML, so the path must be specified to match only one element - thus the `[1]` at the end of the example.

Let us insert a `weigth` tag into the XML description of product Lego City harbor after the `package_size` tag.

```sql
UPDATE Product
SET Description.modify(
'INSERT <weight>2.28</weight>
after (/product/package_parameters/package_size)[1]')
WHERE Name='Lego City harbour'
```

The expression has of two parts here too: the first one (`insert`) specifies the new element, and the second one describes where to insert the new element. The new item can be added as a sibling or child of the specified item.

Let us remove the `description` tag(s) from the description of every product.

```sql
UPDATE Product
SET Description.modify('DELETE /product/description')
WHERE Description IS NOT NULL
```

When deleting, we specify the path of the items to be deleted after `delete`.
