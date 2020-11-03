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
select p.Name, oi.Amount
from Product p
     left outer join OrderItem oi on p.id=oi.ProductID
     left outer join [Order] o on o.ID=oi.OrderID
where p.Name like 'M%'
```

!!! info "`[Order]`"
    `[Order]` is in brackets, because this signals that this is a table name and not the beginning of the `order by` SQL language element.

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

List the order statuses, deadlines and dates

```sql
select o.Date, o.Deadline, s.Name
from [Order] o inner join Status s on o.StatusId=s.ID
```

An alternative, but the two are not equivalent: the subquery is the equivalent of the left outer join and not the innter join!

```sql
select o.Date, o.Deadline,
       (select s.Name
        from Status s
        where o.StatusId=s.ID)
from [Order] o
```

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
select p.Id, p.Name, min(oi.Price), max(oi.Price), avg(oi.Price)
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
where q1.dr<3
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
