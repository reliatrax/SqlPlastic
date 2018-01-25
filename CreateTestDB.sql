USE master
GO

/*
drop database SqlPlasticTestDB
GO

CREATE DATABASE SqlPlasticTestDB CONTAINMENT=PARTIAL
ON (NAME='Data',
	FILENAME='C:\CCS\DBBackups\SqlPlasticTestDB.mdf',
	SIZE=4, FILEGROWTH=10%)
LOG ON (NAME='Log',
	FILENAME='C:\CCS\DBBackups\SqlPlasticTestDB.ldf',
	SIZE=4, FILEGROWTH=10%)	
GO
*/

use SqlPlasticTestDB;
GO
drop table dbo.Preferences;
GO
drop table dbo.Employees;
GO
drop table dbo.OrderLineItems;
GO
drop table dbo.Orders;
GO
drop table dbo.Customers;
GO
drop table dbo.Products;
GO
drop table dbo.MyDataTypes;
GO

CREATE TABLE dbo.Customers
(
	CustomerID INT IDENTITY(1,1) NOT NULL,		-- unique id for this record
	
	FirstName VARCHAR(12) NOT NULL,
	LastName VARCHAR(20) NOT NULL,
	
	CONSTRAINT PK_Customers PRIMARY KEY CLUSTERED  (CustomerID ASC),
)
GO

CREATE TABLE dbo.Products
(
	ProductID INT IDENTITY(1,1) NOT NULL,		-- Primary key

	ProductName VARCHAR(100) NOT NULL,	
	ProductPrice MONEY NOT NULL,

	CONSTRAINT PK_ProductID PRIMARY KEY CLUSTERED  (ProductID ASC),
)
GO

CREATE TABLE dbo.Orders
(
	OrderID INT IDENTITY(1,1) NOT NULL,		-- Primary key
	OrderCustomerID INT NOT NULL,			-- Foreign key
	
	OrderDate DATETIME NOT NULL,
		
	CONSTRAINT PK_OrderID PRIMARY KEY CLUSTERED  (OrderID ASC),
	CONSTRAINT FK_Customers FOREIGN KEY (OrderCustomerID) REFERENCES dbo.Customers (CustomerID) ON DELETE CASCADE 
)
GO

-- Many-to-many glue table (glues Products to Orders)
CREATE TABLE dbo.OrderLineItems
(
	OrderLineItemID INT IDENTITY(1,1) NOT NULL,		-- Primary key
	LineOrderID INT NOT NULL,						-- Foreign key
	LineProductID INT NOT NULL,	       			-- Foreign key
	
	LineQuantity INT NOT NULL,
		
	CONSTRAINT PK_OrderLineID PRIMARY KEY CLUSTERED  (OrderLineItemID ASC),
	CONSTRAINT FK_Orders FOREIGN KEY (LineOrderID) REFERENCES dbo.Orders (OrderID) ON DELETE CASCADE,
	CONSTRAINT FK_Products FOREIGN KEY (LineProductID) REFERENCES dbo.Products (ProductID) ON DELETE CASCADE
)
GO

-- Self-Referential nullable foreign key
CREATE TABLE dbo.Employees
(
	EmployeeID INT IDENTITY(100,2) NOT NULL,		-- Primary key
	EmployeeName VARCHAR(100) NOT NULL,	
	EmployeeTitle VARCHAR(200),

	ManagerEmployeeID INT NULL,					-- *nullable* Foreign key to self
		
	CONSTRAINT PK_EmployeeID PRIMARY KEY CLUSTERED  (EmployeeID ASC),
	CONSTRAINT FK_ManagerID FOREIGN KEY (ManagerEmployeeID) REFERENCES dbo.Employees (EmployeeID) ON DELETE NO ACTION
)
GO

-- Multiple foreign keys
CREATE TABLE dbo.Preferences
(
	PreferenceID INT IDENTITY(1,1) NOT NULL,		-- Primary key

	CustomerID_A INT NULL,		-- FK to customers
	CustomerID_B INT NULL,		-- FK to customers
	CustomerID_C INT NULL,		-- FK to customers
		
	CONSTRAINT PK_PreferenceID PRIMARY KEY CLUSTERED  (PreferenceID ASC),
	CONSTRAINT FK_CustomerID_A FOREIGN KEY (CustomerID_A) REFERENCES dbo.Customers (CustomerID) ON DELETE NO ACTION,
	CONSTRAINT FK_CustomerID_B FOREIGN KEY (CustomerID_B) REFERENCES dbo.Customers (CustomerID) ON DELETE NO ACTION,
	CONSTRAINT FK_CustomerID_C FOREIGN KEY (CustomerID_C) REFERENCES dbo.Customers (CustomerID) ON DELETE NO ACTION
)
GO


-- All the data types that we auto-map
CREATE TABLE dbo.MyDataTypes
(
	MyDataTypeID INT NOT NULL,	-- Primary Key

    MyBool BIT NOT NULL,
	MyTinyInt TINYINT NOT NULL,
	MyInt INT NOT NULL,
	MyBigInt BIGINT NOT NULL,
	MySmallMoney SMALLMONEY NOT NULL,
	MyMoney MONEY NOT NULL,
	MyDecimal DECIMAL(19,4) NOT NULL,
	MyNumeric DECIMAL(20,5) NOT NULL,
	MyReal REAL NOT NULL,
	MyFloat FLOAT NOT NULL,

	MyChar1 CHAR(1) NOT NULL,
	MyChar10 CHAR(10) NOT NULL,

	MyVarChar100 VARCHAR(100) NOT NULL,
	MyVarCharMax VARCHAR(MAX) NOT NULL,

	MyNVarChar100 NVARCHAR(100) NOT NULL,
	MyText TEXT NOT NULL,
	MyXML XML NOT NULL,

	MySmallDateTime SMALLDATETIME NOT NULL,
	MyDateTime DATETIME NOT NULL,
	MyDateTime2 DATETIME2 NOT NULL,
    MyDateTimeOffset DATETIMEOFFSET NOT NULL,
	MyDate DATE NOT NULL,
	MyTime TIME NOT NULL,

	MyBinary50 BINARY(50),
	MyVarBinary60 VARBINARY(60),
	MyVarBinaryMax VARBINARY(MAX),
	MyTimeStamp TIMESTAMP,

	MyGUID UNIQUEIDENTIFIER NOT NULL,

	CONSTRAINT PK_MyDataTypeID PRIMARY KEY CLUSTERED  (MyDataTypeID ASC),
)
GO
