# SQL language, MSSQL platform-specific SQL

You can run these queries on the [sample database](../../db/index.md).

## Simple queries

Which product costs less than 2000 and have less than 50 in stock?

```sql
select Name, Price, Stock
from Product
where Price<2000 and Stock<50
```

Which product has no description?

```sql
select *
from Product
where Description is null
```

## Joining tables

Customers with a main site in Budapest (the two alternatives are equivalent).

```sql
select *
from Customer c, CustomerSite s
where c.MainCustomerSiteID=s.ID and City='Budapest'

select *
from Customer c inner join CustomerSite s on c.MainCustomerSiteID=s.ID
where City='Budapest'
```

List the products that start with letter M, the ordered amounts and deadlines. Include the products that have not been ordered yet.

```sql
select p.Name, sum(oi.Amount)
from Product p
     left outer join OrderItem oi on p.id=oi.ProductID
where p.Name like 'M%'
group by p.Name
```

## Sorting

```sql
select *
from Product
order by Name
```

Microsoft SQL Server specific: _collation_ specifies the rules for sorting

```sql
select *
from Product
order by Name collate SQL_Latin1_General_Cp1_CI_AI
```

Sort by multiple fields

```sql
select *
from Product
order by Stock desc, Price
```

## Subqueries

List the order dates, deadlines and Statuses

```sql
select o.Date, o.Deadline, s.Name
from [Order] o inner join Status s on o.StatusId=s.ID
```

An alternative, but the two are not equivalent: the subquery is the equivalent of the left outer join and not the inner join!

```sql
select o.Date, o.Deadline,
       (select s.Name
        from Status s
        where o.StatusId=s.ID)
from [Order] o
```

!!! info "`[Order]`"
    `[Order]` is in brackets, because this signals that this is a table name and not the beginning of the `order by` SQL language element.

## Filter duplicates

Which products have been ordered in batches of more than 3? One product may have been ordered multiple times, but we want the name only once.

```sql
select distinct p.Name
from Product p inner join OrderItem oi on oi.ProductID=p.ID
where oi.Amount>3
```

## Aggregate functions

How much is the most expensive product?

```sql
select max(Price)
from Product
```

Which are the most expensive products?

```sql
select *
from Product
where Price=(select max(Price) from Product)
```

What was the min, max and average selling price of each product with name containing _Lego_ having an average selling price more than 10000

```sql
select p.Id, p.Name, min(oi.Price), max(oi.Price), sum(oi.Price*oi.Amount)/sum(oi.Amount)
from Product p
     inner join OrderItem oi on p.ID=oi.ProductID
Where p.Name like '%Lego%'
group by p.Id, p.Name
having avg(oi.Price)>10000
order by 2
```

## Inserting records

Inserting a single record by assigning value to all columns (except _identity_)

```sql
insert into Product
values ('aa', 100, 0, 3, 2, null)
```

Set values of selected columns only

```sql
insert into Product (Name,Price)
values ('aa', 100)
```

Insert the result of a query

```sql
insert into Product (Name, Price)
select Name, Price
from InvoiceItem
where Amount>2
```

MSSQL specific: identity column

```sql
create table VAT
(
   ID int identity primary key,
   Percentage int
)

insert into VAT(Percentage)
values (27)

select @@identity
```

MSSQL specific: setting the value of _identity_ column

```sql
set identity_insert VAT on

insert into VAT (ID, Percentage)
values (123, 27)

set identity_insert VAT off
```

## Updating records

Raise the price of LEGOs by 10% and add 5 to stock

```sql
update Product
set Price=1.1*Price,
    Stock=Stock+5
where Name like '%Lego%'
```

Update based on filtering by referenced table content: raise the price by 10% for those products that are subject to 20% VAT, and have more then 10 pcs in stock

```sql
update Product
set Price=1.1*Price
where Stock>10
and VATID in
(
    select ID
    from VAT
    where Percentage=20
)
```

MSSQL Server specific solution to the same task

```sql
update Product
set Price=1.1*Price
from Product p
     inner join VAT v on p.VATID=v.ID
where Stock>10
      and Percentage=20
```

## Deleting records

```sql
delete
from Product
where ID>10
```

## Assigning ranks

Assigning ranks by ordering

```sql
select p.*,
       rank() over (order by Name) as r,
       dense_rank() over (order by Name) as dr
from Product p
```

Ranking within groups

```sql
select p.*
       ,rank() over (partition by CategoryID order by Name) as r
       ,dense_rank() over (partition by CategoryID order by Name) as dr
from Product p
```

!!! example "Rank and dense_rank"
     Unlike dense_rank , Rank skips positions after equal rankings. The number of positions skipped depends on how many rows had an identical ranking. For example, Mary and Lisa sold the same number of products and are both ranked as 1. With Rank,  the next position is 3; with dense_rank, the next position is 2.

## CTE (Common Table Expression)

Motivation: subqueries often make queries complex

First three products sorted by name alphabetically

```sql
select *
from
(
    select p.*
            ,rank() over (order by Name) as r
            ,dense_rank() over (order by Name) as dr
    from Product p
) a
where a.dr<=3
```

Same solution using CTE

```sql
with q1
as
(
    select *
           ,rank() over (order by Name) as r
          ,dense_rank() over (order by Name) as dr
    from Product
)
select *
from q1
where q1.dr<=3
```

How many pieces have been sold from the second most expensive product?

```sql
with q
as
(
    select *
            , dense_rank() over (order by Price desc) dr
    from Product
)
select q.ID, q.Name, sum(Amount)
from q
     inner join OrderItem oi on oi.ProductID=q.ID
where q.dr = 2
group by q.ID, q.Name
```

Paging: list products alphabetically from 3. to 8. record

```sql
with q
as
(
    select *
            , rank() over (order by Name) r
    from Product
)
select *
from q
where q.r between 3 and 8
```

Paging using MSSQL Server (2012+) specific syntax

```sql
select *
from Product
order by Name
offset 2 rows
fetch next 6 rows only

select top 3 *
from Product
order by Name
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
select Description.query('/product/package_parameters/number_of_packages')
from Product
```

For example, this could yield:

```xml
<number_of_packages>1</number_of_packages>
```

The function `query()` returns XML; if it is only the value that is needed, we can use the `value()` function. The `value()` function must also specify the type of data queried as a string literal.

```sql
select Description.value('(/product/package_parameters/number_of_packages)[1]', 'int')
from Product
```

The result will be 1.

!!! info "SQLType"
    The type passed as a parameter cannot be xml. Conversion to the specified type is performed with the T-SQL [`CONVERT`](https://docs.microsoft.com/en-us/sql/t-sql/functions/cast-and-convert-transact-sql) function.

Let us query the names of the recommended products for ages 0-18 months.

```sql
select Name
from Product
where Description.exist('(/product)[(./recommended_age)[1] eq "0-18 m"]')=1
```

Function `exist()` returns 1 if the _XQuery_ expression evaluation yields a non-empty result; or 0 if the query result is empty.

We can also use the `value()` method instead of `exist()` here.

```sql
select Name
from Product
where Description.value('(/product/recommended_age)[1]', 'varchar(max)')='0-18 m'
```

### Manipulating queries

We can not only query XML data, but also modify it in place. The modification in the database is performed in an atomic way, i.e., there is no need to fetch the XML into a client application, modify it and then write it back. Instead, following the philosophy of server-side programming, we bring the logic (here: modification) into the database. Data modification queries can be performed with the [`modify(XML_DML)`](https://docs.microsoft.com/en-us/sql/t-sql/xml/modify-method-xml-data-type) function, where we use the so-called [XML DML](https://docs.microsoft.com/en-us/sql/t-sql/xml/xml-data-modification-language-xml-dml) language to describe the desired change. Let's look at a few examples.

In the product called Lego City harbor, let us change the recommended age to 6-99 years.

```sql
update Product
set Description.modify(
'replace value of (/product/recommended_age/text())[1]
with "6-99 y"')
where Name='Lego City harbour'
```

The XML DML expression consists of two parts: in the first part (`replace value of`) the element to be modified is selected; in the second part (`with`) the new value is specified. Only one element can be modified within an XML, so the path must be specified to match only one element - thus the `[1]` at the end of the example.

Let us insert a `weigth` tag into the XML description of product Lego City harbor after the `package_size` tag.

```sql
update Product
set Description.modify(
'insert <weight>2.28</weight>
after (/product/package_parameters/package_size)[1]')
where Name='Lego City harbour'
```

The expression has of two parts here too: the first one (`insert`) specifies the new element, and the second one describes where to insert the new element. The new item can be added as a sibling or child of the specified item.

Let us remove the `description` tag(s) from the description of every product.

```sql
update Product
set Description.modify('delete /product/description')
where Description is not null
```

When deleting, we specify the path of the items to be deleted after `delete`.
