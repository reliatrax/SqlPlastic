USE master
GO

--drop database SqlPlasticTestDB

CREATE DATABASE SqlPlasticTestDB CONTAINMENT=PARTIAL
ON (NAME='Data',
	FILENAME='C:\CCS\DBBackups\SqlPlasticTestDB.mdf',
	SIZE=4, FILEGROWTH=10%)
LOG ON (NAME='Log',
	FILENAME='C:\CCS\DBBackups\SqlPlasticTestDB.ldf',
	SIZE=4, FILEGROWTH=10%)	
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
	ProductDescript VARCHAR(MAX) NOT NULL,
		
	CONSTRAINT PK_ProductID PRIMARY KEY CLUSTERED  (ProductID ASC),
)
GO

CREATE TABLE dbo.Orders
(
	OrderID INT IDENTITY(1,1) NOT NULL,		-- Primary key
	OrderProductID INT NOT NULL,	       	-- Foreign key
	OrderCustomerID INT NOT NULL,			-- Foreign key
	
	OrderDate DATETIME NOT NULL,
		
	CONSTRAINT PK_MigrationID PRIMARY KEY CLUSTERED  (OrderID ASC),
	CONSTRAINT FK_Products FOREIGN KEY (OrderProductID) REFERENCES dbo.Products (ProductID) ON DELETE CASCADE,
	CONSTRAINT FK_Customers FOREIGN KEY (OrderCustomerID) REFERENCES dbo.Customers (CustomerID) ON DELETE CASCADE 
)
GO
