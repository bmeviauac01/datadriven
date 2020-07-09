BEGIN TRY

	BEGIN TRANSACTION


	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].InvoiceItem') AND type in (N'U'))
		drop table InvoiceItem
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].Invoice') AND type in (N'U'))
		drop table Invoice
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].InvoiceIssuer') AND type in (N'U'))
		drop table InvoiceIssuer
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].OrderItem') AND type in (N'U'))
		drop table OrderItem
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Order]') AND type in (N'U'))
		drop table [Order]
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].Product') AND type in (N'U'))
		drop table Product
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].Category') AND type in (N'U'))
		drop table Category
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].VAT') AND type in (N'U'))
		drop table VAT
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].PaymentMethod') AND type in (N'U'))
		drop table PaymentMethod
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].Status') AND type in (N'U'))
		drop table Status
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].Customer') AND type in (N'U'))
		alter table Customer drop constraint Customer_MainCustomerSite
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].CustomerSite') AND type in (N'U'))
		drop table CustomerSite
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].Customer') AND type in (N'U'))
		drop table Customer

	--*****************************************************************
	-- Create tables
	--*****************************************************************

	create table [VAT]
	( 
		ID int identity primary key,
		Percentage int
	)

	create table [PaymentMethod]
	(
		ID int identity primary key,
		Method nvarchar(20),
		Deadline int
	)

	create table [Status]
	(
		ID int identity primary key,
		Name nvarchar(20)

	)

	create table [Category]
	(
		ID int identity primary key,
		Name nvarchar(50),
		ParentCategoryID int references Category(ID)
	)

	create table Product
	(
		ID int identity primary key,
		[Name] nvarchar(50),
		Price float,
		Stock int,
		VATID int references VAT(ID),
		CategoryID int references Category(ID),
		[Description] XML
	)


	create table [Customer]
	(
		ID int identity primary key,
		[Name] nvarchar(50),
		BankAccount varchar(50),
		[Login] nvarchar(50),
		[Password] nvarchar(50),
		Email varchar(50),
		MainCustomerSiteID int
	)

	create table [CustomerSite]
	(
		ID int identity primary key,
		ZipCode char(4),
		City nvarchar(50),
		Street nvarchar(50),
		Tel varchar(15),
		Fax varchar(15),
		CustomerID int references Customer(ID)
	)

	alter table [Customer] add constraint Customer_MainCustomerSite foreign key (MainCustomerSiteID) references CustomerSite(ID)

	create table [Order]
	(
		ID int identity primary key,
		[Date] datetime,
		Deadline datetime,
		CustomerSiteID int references CustomerSite(ID),
		StatusID int references Status(ID),
		PaymentMethodID int references PaymentMethod(ID)
	)

	create table [OrderItem]
	(
		ID int identity primary key,
		Amount int,
		Price float,
		OrderID int references [Order](ID),
		ProductID int references Product(ID),
		StatusID int references Status(ID)
	)

	create table [InvoiceIssuer]
	(
		ID int identity primary key,
		[Name] nvarchar(50),
		ZipCode char(4),
		City nvarchar(50),
		Street nvarchar(50),
		TaxIdentifier varchar(20),
		BankAccount varchar(50)
	)

	create table [Invoice]
	(
		ID int primary key,
		CustomerName nvarchar(50),
		CustomerZipCode char(4),
		CustomerCity nvarchar(50),
		CustomerStreet nvarchar(50),
		PrintedCopies int,
		Cancelled bit,
		PaymentMethod nvarchar(20),
		CreationDate datetime,
		DeliveryDate datetime,
		PaymentDeadline datetime,
		InvoiceIssuerID int references InvoiceIssuer(ID),
		OrderID int references [Order](ID)
	)

	create table [InvoiceItem]
	(
		ID int identity primary key,
		[Name] nvarchar(50),
		Amount int,
		Price float,
		VATPercentage int,
		InvoiceID int references Invoice(ID),
		OrderItemID int references OrderItem(ID)
	)

	--*****************************************************************
	-- Insert sample data
	--*****************************************************************


	SET IDENTITY_INSERT VAT ON
	insert into VAT(id, Percentage) values (1,0);
	insert into VAT(id, Percentage) values (2,15);
	insert into VAT(id, Percentage) values (3,27);
	SET IDENTITY_INSERT VAT OFF

	SET IDENTITY_INSERT Category ON
	insert into Category (id, Name, ParentCategoryID) values (1,'Toy',NULL);
	insert into Category (id, Name, ParentCategoryID) values (2,'Play house',NULL);
	insert into Category (id, Name, ParentCategoryID) values (3,'Baby toy',1);
	insert into Category (id, Name, ParentCategoryID) values (4,'Construction toy',1);
	insert into Category (id, Name, ParentCategoryID) values (5,'Wooden toy',1);
	insert into Category (id, Name, ParentCategoryID) values (6,'Plush figure',1);
	insert into Category (id, Name, ParentCategoryID) values (7,'Bicycles',1);
	insert into Category (id, Name, ParentCategoryID) values (8,'Months 0-6',3);
	insert into Category (id, Name, ParentCategoryID) values (9,'Months 6-18',3);
	insert into Category (id, Name, ParentCategoryID) values (10,'Months 18-24',3);
	insert into Category (id, Name, ParentCategoryID) values (11,'DUPLO',4);
	insert into Category (id, Name, ParentCategoryID) values (13,'LEGO',4);
	insert into Category (id, Name, ParentCategoryID) values (14,'Building items',4);
	insert into Category (id, Name, ParentCategoryID) values (15,'Building blocks',5);
	insert into Category (id, Name, ParentCategoryID) values (16,'Toys for skill development',5);
	insert into Category (id, Name, ParentCategoryID) values (17,'Logic toys',5);
	insert into Category (id, Name, ParentCategoryID) values (18,'Craftwork toys',5);
	insert into Category (id, Name, ParentCategoryID) values (19,'Baby taxis',7);
	insert into Category (id, Name, ParentCategoryID) values (20,'Motors',7);
	insert into Category (id, Name, ParentCategoryID) values (21,'Tricycle',7);
	SET IDENTITY_INSERT Category OFF

	SET IDENTITY_INSERT Product ON
	insert into Product (id, Name, Price,Stock,VATid,Categoryid) values (1,'Activity playgim',7488, 21, 3, 8);
	insert into Product (id, Name, Price,Stock,VATid,Categoryid) values (2,'Colorful baby book',1738, 58, 3, 8);
	insert into Product (id, Name, Price,Stock,VATid,Categoryid) values (3,'Baby telephone',3725, 18, 3, 9);
	insert into Product (id, Name, Price,Stock,VATid,Categoryid) values (4,'Fisher Price hammer toy',8356, 58, 3, 10);
	insert into Product (id, Name, Price,Stock,VATid,Categoryid) values (5,'Mega Bloks 24 pcs',4325, 47, 3, 14);
	insert into Product (id, Name, Price,Stock,VATid,Categoryid) values (6,'Maxi Blocks 56 pcs',1854, 36, 3, 14);
	insert into Product (id, Name, Price,Stock,VATid,Categoryid) values (7,'Building Blocks 80 pcs',4362, 25, 3, 14);
	insert into Product (id, Name, Price,Stock,VATid,Categoryid) values (8,'Lego City harbour',27563, 12, 3, 13);
	insert into Product (id, Name, Price,Stock,VATid,Categoryid) values (9,'Lego Duplo Excavator',6399, 26, 3, 11);
	insert into Product (id, Name, Price,Stock,VATid,Categoryid) values (10,'Child supervision for 1 hour',800, 0, 2, 2);
	SET IDENTITY_INSERT Product OFF

	SET IDENTITY_INSERT CustomerSite ON
	insert into CustomerSite (id, ZipCode, City, Street, tel, fax, Customerid)
		values (1,'1114','Budapest','Bud Spencer street 16.','061-569-23-99',null,null);
	insert into CustomerSite (id, ZipCode, City, Street, tel, fax, Customerid)
		values (2,'1051','Budapest','Andrássy út 22.','061-457-11-03','061-457-11-04',null);
	insert into CustomerSite (id, ZipCode, City, Street, tel, fax, Customerid)
		values (3,'3000','Hatvan','Vörösmarty tér. 5.','0646-319-169','0646-319-168',null);
	insert into CustomerSite (id, ZipCode, City, Street, tel, fax, Customerid)
		values (4,'2045','Törökbálint','Main street 17.','0623-200-156','0623-200-155',null);
	SET IDENTITY_INSERT CustomerSite OFF

	SET IDENTITY_INSERT Customer ON
	insert into Customer (id, Name, BankAccount,login, Password,email,MainCustomerSiteId)
		values (1,'Cody Shelton','16489665-05899845-10000038','cshelton','huti9haj1s','cshelton@freemail.hu',2);
	update CustomerSite set Customerid =1 where id = 2;

	insert into Customer (id, Name, BankAccount,login, Password,email,MainCustomerSiteId)
		values (2,'Erika Mckenzie','54255831-15615432-25015126','erikkka','gandalf67j','erikkka@hotmail.com',1);
	update CustomerSite set Customerid=2 where id = 1;
	update CustomerSite set Customerid=2 where id = 3;

	insert into Customer (id, Name, BankAccount,login, Password,email,MainCustomerSiteId)
		values (3,'Krista Hansen','25894467-12005362-59815126','kris','jag7guFs','kris.hansen@gmail.com',4);
	update CustomerSite set Customerid =3 where id = 4;

	SET IDENTITY_INSERT Customer OFF

	SET IDENTITY_INSERT InvoiceIssuer ON
	insert into InvoiceIssuer (id, Name, ZipCode, City, Street, TaxIdentifier, BankAccount)
		values (1,'ToysRus','1119','Budapest','Main street 23','15684995-2-32','259476332-15689799-10020065');
	insert into InvoiceIssuer (id, Name, ZipCode, City, Street, TaxIdentifier, BankAccount)
		values (2,'BabiesRus','1119','Budapest','Main street 23','68797867-1-32','259476332-15689799-10020065');
	SET IDENTITY_INSERT InvoiceIssuer OFF

	SET IDENTITY_INSERT Status ON
	insert into Status (id, Name) values (1,'New');
	insert into Status (id, Name) values (2,'Processing');
	insert into Status (id, Name) values (3,'Packaged');
	insert into Status (id, Name) values (4,'In transit');
	insert into Status (id, Name) values (5,'Delivered');
	SET IDENTITY_INSERT Status OFF

	SET IDENTITY_INSERT PaymentMethod ON
	insert into PaymentMethod (id, Method, Deadline) values (1,'Cash',0);
	insert into PaymentMethod (id, Method, Deadline) values (2,'Wire transfer 8',8);
	insert into PaymentMethod (id, Method, Deadline) values (3,'Wire transfer 15',15);
	insert into PaymentMethod (id, Method, Deadline) values (4,'Wire transfer 30',30);
	insert into PaymentMethod (id, Method, Deadline) values (5,'Credit card',0);
	insert into PaymentMethod (id, Method, Deadline) values (6,'Collect package',0);
	SET IDENTITY_INSERT PaymentMethod OFF

	SET IDENTITY_INSERT [Order] ON
	insert into [Order] (id, Date, Deadline, CustomerSiteid, Statusid, PaymentMethodid)
		values (1,'2020-01-18','2020-01-30',3,5,1);
	insert into [Order] (id, Date, Deadline, CustomerSiteid, Statusid, PaymentMethodid)
		values (2,'2020-02-13','2020-02-15',2,5,2);
	insert into [Order] (id, Date, Deadline, CustomerSiteid, Statusid, PaymentMethodid)
		values (3,'2020-02-15','2020-02-20',1,2,1);
	insert into [Order] (id, Date, Deadline, CustomerSiteid, Statusid, PaymentMethodid)
		values (4,'2020-02-15','2020-02-20',2,3,5);
	SET IDENTITY_INSERT [Order] OFF

	SET IDENTITY_INSERT OrderItem ON
	-- first [Order]
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (1,2,8356,1,4,5);
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (2,1,1854,1,6,5);
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (3,5,1738,1,2,5);
	-- second [Order]
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (4,2,7488,2,1,5);
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (5,3,3725,2,3,5);
	-- third [Order]
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (6,1,4362,3,7,3);
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (7,6,1854,3,6,2);
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (8,2,6399,3,9,3);
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (9,5,1738,3,2,1);
	-- forth [Order]
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (10,23,3725,4,3,3);
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (11,12,1738,4,2,3);
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (12,10,27563,4,8,3);
	insert into OrderItem (id, Amount, Price,OrderID,Productid,Statusid)
		values (13,25,7488,4,1,3);
	SET IDENTITY_INSERT OrderItem OFF


	insert into Invoice (id, CustomerName, CustomerZipCode,CustomerCity,CustomerStreet,PrintedCopies,
						Cancelled,PaymentMethod,CreationDate,DeliveryDate,PaymentDeadline,InvoiceIssuerID,OrderID)
		values (1,'Erika Mckenzie','3000','Hatvan','Second street 5.',2,0,'Cash','2008-01-30','2008-01-30','2008-01-30',1,1);

	insert into Invoice (id, CustomerName, CustomerZipCode,CustomerCity,CustomerStreet,PrintedCopies,
						Cancelled,PaymentMethod,CreationDate,DeliveryDate,PaymentDeadline,InvoiceIssuerID,OrderID)
		values (2,'Cody Shelton','1051','Budapest','First street 22.',2,0,'Wire transfer 8','2008-02-14','2008-02-15','2008-02-23',1,2);


	SET IDENTITY_INSERT InvoiceItem ON
	insert into InvoiceItem (id, Name, Amount, Price, VATPercentage,Invoiceid, OrderItemid)
			values (1,'Fisher Price hammer',2,8356,27,1,1);
	insert into InvoiceItem (id, Name, Amount, Price, VATPercentage,Invoiceid, OrderItemid)
			values (2,'Maxi Blocks 56 pcs',1,1854,27,1,2);
	insert into InvoiceItem (id, Name, Amount, Price, VATPercentage,Invoiceid, OrderItemid)
			values (3,'Colorful baby book',5,1738,27,1,3);
	insert into InvoiceItem (id, Name, Amount, Price, VATPercentage,Invoiceid, OrderItemid)
			values (4,'Activity playgim',2,7488,27,2,4);
	insert into InvoiceItem (id, Name, Amount, Price, VATPercentage,Invoiceid, OrderItemid)
			values (5,'Baby telephone',3,3725,27,2,5);
	SET IDENTITY_INSERT InvoiceItem OFF

	

	--*****************************************************************
	-- XML descriptions
	--*****************************************************************
	
	update Product set Description = 
	'<?xml version="1.0" encoding="ISO-8859-1"?>
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
	</product>'
	where id = 1

	update Product set Description = 
	'<?xml version="1.0" encoding="ISO-8859-1"?>
	<product>
		<product_size>
			<unit>cm</unit>
			<width>15</width>
			<height>2</height>
			<depth>15</depth>
		</product_size>
		<package_parameters>
			<number_of_packages>1</number_of_packages>
			<package_size>
				<unit>cm</unit>
				<width>15</width>
				<height>2</height>
				<depth>15</depth>
			</package_size>
		</package_parameters>
		<description>
			Round ball with nice colors.
		</description>
		<recommended_age>0-18 m</recommended_age>
	</product>'
	where id = 2
	
	update Product set Description = 
	'<?xml version="1.0" encoding="ISO-8859-1"?>
	<product>
		<product_size>
			<unit>cm</unit>
			<width>20</width>
			<height>12</height>
			<depth>35</depth>
		</product_size>
		<package_parameters>
			<number_of_packages>1</number_of_packages>
			<package_size>
				<unit>cm</unit>
				<width>40</width>
				<height>25</height>
				<depth>50</depth>
			</package_size>
		</package_parameters>
		<description>
			Music is good for the ears. Enjoy.
		</description>
		<recommended_age>9-36 m</recommended_age>
	</product>'
	where id = 3
	
	update Product set Description = 
	'<?xml version="1.0" encoding="ISO-8859-1"?>
	<product>
		<package_parameters>
			<number_of_packages>1</number_of_packages>
			<package_size>
				<unit>cm</unit>
				<width>80</width>
				<height>20</height>
				<depth>40</depth>
			</package_size>
		</package_parameters>
		<description>
			Number of elements: 695.
		</description>
		<recommended_age>5-12 y</recommended_age>
	</product>'
	where id = 8
	
	IF @@Trancount >0
		commit
	
END TRY
BEGIN CATCH
	IF @@Trancount >0
		rollback
	IF  CURSOR_STATUS('global','cur') >= -1
		deallocate cur
	
	SELECT 
        ERROR_NUMBER() AS ErrorNumber,
        ERROR_SEVERITY() AS ErrorSeverity,
        ERROR_STATE() as ErrorState,
        ERROR_PROCEDURE() as ErrorProcedure,
        ERROR_LINE() as ErrorLine,
        ERROR_MESSAGE() as ErrorMessage
END CATCH

