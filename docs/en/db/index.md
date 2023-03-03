# Sample database scheme

The examples and seminars during the semester will use a sample database. The database is a simplified retail management system with products, customers, and orders. This description details the relational schema of the database; the MongoDB variant is the appropriate mirror of this scheme.

## The context of the database

The system is designed to help the retail process of products. The _products_ are grouped into hierarchical _categories_. _Customers_ can browse products, place _orders_, and track the _status_ of the orders.

Customers can have multiple _sites_ (e.g., retail stores of one company with multiple addresses). The order can be completed to any of these sites. Each customer has exactly one "main site," which is where the invoices are addressed. An order can have multiple _items_; each item has its status.

An _invoice_ is printed if the order is ready. An invoice cannot be changed once it is created. Different products have different _VAT_ (value-added tax) rates. These VAT rates are subject to change over time, but these changes must not affect existing invoices.

## Data scheme

The model of the whole database is depicted below.

![Data scheme](../db/images/db.png)

### Tables and columns

| **Table**     | **Column**         | **Description**                                                                                                                      |
| ------------- | ------------------ | ------------------------------------------------------------------------------------------------------------------------------------ |
| VAT           | ID                 | Auto-generated primary key.                                                                                                          |
|               | Percentage         | The percentage of the value-added tax.                                                                                               |
| PaymentMethod | ID                 | Auto-generated primary key.                                                                                                          |
|               | Method             | Short name of the payment method, e.g., cash or wire transfer.                                                                        |
|               | Deadline           | The deadline of the payment method, that is, the deadline for completing the transaction after the invoice is received.              |
| Status        | ID                 | Auto-generated primary key.                                                                                                          |
|               | Name               | Short name of the status (e.g., new, processed).                                                                                      |
| Category      | ID                 | Auto-generated primary key.                                                                                                          |
|               | Name               | Name of the category, e.g., toys, LEGO, etc.                                                                                          |
|               | ParentCategoryID   | Foreign key indicating the parent category; null if this is a top-level category.                                                    |
| Product       | ID                 | Auto-generated primary key.                                                                                                          |
|               | Name               | Product name.                                                                                                                        |
|               | Price              | Product price without tax.                                                                                                           |
|               | Stock              | Amount of this product in stock.                                                                                                     |
|               | VATID              | Foreign key to the VAT table.                                                                                                        |
|               | CategoryID         | Foreign key to the Category table.                                                                                                   |
|               | Description        | XML description of the product.                                                                                                      |
| Customer      | ID                 | Auto-generated primary key.                                                                                                          |
|               | Name               | Customer name.                                                                                                                       |
|               | BankAccount        | Back account number of the customer.                                                                                                 |
|               | Login              | Login name for the webshop.                                                                                                          |
|               | Password           | Password for the webshop.                                                                                                            |
|               | Email              | Email address of the customer.                                                                                                       |
|               | MainCustomerSiteID | The main site of the customer; a foreign key to the CustomerSite table.                                                                |
| CustomerSite  | ID                 | Auto-generated primary key.                                                                                                          |
|               | ZipCode            | The zip code of the address.                                                                                                         |
|               | City               | The city part of the address.                                                                                                        |
|               | Street             | The street and house number part of the address.                                                                                     |
|               | Tel                | Telephone number.                                                                                                                    |
|               | Fax                | Fax number.                                                                                                                          |
|               | CustomerID         | Foreign key to the Customer table.                                                                                                   |
| Order         | ID                 | Auto-generated primary key.                                                                                                          |
|               | Date               | Date when the order was placed.                                                                                                      |
|               | Deadline           | Deadline until the order must be completed.                                                                                          |
|               | CustomerSiteID     | Foreign key to the CustomerSite table; the order is billed and shipped to this site.                                                 |
|               | StatusID           | Foreign key to the Status table; the actual status of the order.                                                                     |
|               | PaymentMethodID    | Foreign key to the PaymentMethod table; the chosen method of payment.                                                                |
| OrderItem     | ID                 | Auto-generated primary key.                                                                                                          |
|               | Amount             | The amount ordered of the specific product.                                                                                          |
|               | Price              | The unit price of the product; by default this is the price of the product, but can be altered (e.g. for bulk order).                |
|               | OrderID            | Foreign key to the Order table; identifier the order this item belongs to.                                                           |
|               | ProductID          | Foreign key to the Product table; identifies the product that is ordered.                                                            |
|               | StatusID           | Foreign key to the Status table; the actual status of the item.                                                                      |
| InvoiceIssuer | ID                 | Auto-generated primary key.                                                                                                          |
|               | Name               | Name of the company selling the products.                                                                                            |
|               | ZipCode            | The zip code of the address.                                                                                                         |
|               | City               | The city part of the address.                                                                                                        |
|               | Street             | The street and house number part of the address.                                                                                     |
|               | TaxIdentifier      | Tax identifier of the company.                                                                                                       |
|               | BankAccount        | Bank account number of the company.                                                                                                  |
| Invoice       | ID                 | Auto-generated primary key.                                                                                                          |
|               | CustomerName        | The name of the customer; printed on the invoice.                                                                                   |
|               | CustomerZipCode    | The zip code of the address.                                                                                                         |
|               | CustomerCity       | The city part of the address.                                                                                                        |
|               | CustomerStreet     | The street and house number part of the address.                                                                                     |
|               | PrintedCopies      | Number of copies printed of this invoice.                                                                                            |
|               | Cancelled          | Has the invoice been cancelled?                                                                                                      |
|               | PaymentMethod      | Payment method of the invoice (text name).                                                                                           |
|               | CreationDate       | Date when the invoice was created.                                                                                                   |
|               | DeliveryDate       | Delivery date of the invoice.                                                                                                        |
|               | PaymentDeadline    | Deadline of the payment.                                                                                                             |
|               | InvoiceIssuerID    | Foreign key to the InvoiceIssuer table; the issuer of the invoice.                                                                   |
|               | OrderID            | Foreign key to the Order table; the order out of which this invoice was created.                                                     |
| InvoiceItem   | ID                 | Auto-generated primary key.                                                                                                          |
|               | Name               | Name of the product; printed on the invoice.                                                                                         |
|               | Amount             | The amount ordered of the specific product.                                                                                          |
|               | Price              | The unit price of the product; by default this is the price of the product, but can be altered (e.g., for bulk order).                |
|               | VATPercentage      | The effective percentage of the tax applied.                                                                                         |
|               | InvoiceID          | Foreign key to the Invoice table; the invoice this item is part of.                                                                  |
|               | OrderItemID        | Foreign key to the OrderItem table; the item out of which this invoice item was created.                                             |

### Peculiarities

#### Invoicing

Invoices cannot be altered once issued; they can only be canceled. Therefore all data that appears on the order are copied into the `Invoice` and `InvoiceItem` tables once the invoice is created. There is only a single original copy of the invoice; therefore, the number of printed copies is recorded.

#### Invoice issuer

The issuer of the invoices changes very infrequently. However, in case it changes, existing invoices must remain unaltered. The issuer of the invoice is recorded in a separate table, and only one is in effect at all times. Each invoice references the right `InvoiceIssuerId` at the time.

#### VAT

The `VAT` percentage of products can change at any time. However, existing invoices must not be altered. Therefore the actual VAT percentage is stored with the invoice when created and not referenced from the VAT table.

#### Product description

Some products contain an additional XML description, such as the following example.

```xml
<product>
  <product_size>
    <unit>cm</unit>
    <width>150</width>
    <height>50</height>
    <depth>150</depth>
  </product_size>
  <package_parameters>
    <number_of_packages>1</number_of_packages>
    <package_size>
      <unit>cm</unit>
      <width>150</width>
      <height>20</height>
      <depth>20</depth>
    </package_size>
  </package_parameters>
  <description>
    Requires battery (not part of the package).
  </description>
  <recommended_age>0-18 m</recommended_age>
</product>
```
