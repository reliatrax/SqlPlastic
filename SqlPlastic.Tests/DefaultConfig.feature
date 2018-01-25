Feature: DefaultConfig
	Model building with the default options and mapping rules

	Scenario: Verify all datatypes are mapped correctly by default
	Given a connection to the "SqlPlasticTestDB" database
	When I generate models with the default options
	Then the resulting table "MyDataTypes" should contain exactly the following columns
		| Column Name      | Member Type              |
		| MyDataTypeID     | int                      |
		| MyBool           | bool                     |
		| MyTinyInt        | byte                     |
		| MyInt            | int                      |
		| MyBigInt         | long                     |
		| MySmallMoney     | decimal                  |
		| MyMoney          | decimal                  |
		| MyDecimal        | decimal                  |
		| MyNumeric        | decimal                  |
		| MyReal           | float                    |
		| MyFloat          | double                   |
		| MyChar1          | char                     |
		| MyChar10         | string                   |
		| MyVarChar100     | string                   |
		| MyVarCharMax     | string                   |
		| MyNVarChar100    | string                   |
		| MyText           | string                   |
		| MyXML            | System.Xml.Linq.XElement |
		| MySmallDateTime  | System.DateTime          |
		| MyDateTime       | System.DateTime          |
		| MyDateTime2      | System.DateTime          |
		| MyDateTimeOffset | System.DateTimeOffset    |
		| MyDate           | System.DateTime          |
		| MyTime           | System.Timespan          |
		| MyBinary50       | System.Data.Linq.Binary  |
		| MyVarBinary60    | System.Data.Linq.Binary  |
		| MyVarBinaryMax   | System.Data.Linq.Binary  |
		| MyTimeStamp      | System.Data.Linq.Binary  |
		| MyGUID           | System.GUID              |

Scenario: All tables, columns, and keys should be present
	Given a connection to the "SqlPlasticTestDB" database
	When I generate models with the default options
	Then the resulting model should contain the following tables
	| TableName      | Columns | EntityRefs | EntitySets |
	| Customers      | 3       | 0          | 4          |
	| Products       | 3       | 0          | 1          |
	| Orders         | 3       | 1          | 1          |
	| OrderLineItems | 4       | 2          | 0          |
	| Employees      | 4       | 1          | 1          |
	| Preferences    | 4       | 3          | 0          |
	| MyDataTypes    | 29      | 0          | 0          |


Scenario: Entity Refs 1 - Self reference should end in a 1 by default
	Given a connection to the "SqlPlasticTestDB" database
	When I generate models with the default options
	Then the table "Employees" should contain the following entity references
		| EntityRefName | KeyColumn         | ReferencedColumn | AssociatedSet | ForeignKeyName | DeleteRule | DeleteOnNull |
		| Employee1     | ManagerEmployeeID | EmployeeID       | Employees     | FK_ManagerID   | NO_ACTION  | false        |

Scenario: Entity Refs 2
	Given a connection to the "SqlPlasticTestDB" database
	When I generate models with the default options
	Then the table "OrderLineItems" should contain the following entity references
		| EntityRefName | KeyColumn     | ReferencedColumn | AssociatedSet  | ForeignKeyName | DeleteRule | DeleteOnNull |
		| Order         | LineOrderID   | OrderID          | OrderLineItems | FK_Orders      | CASCADE    | true         |
		| Product       | LineProductID | ProductID        | OrderLineItems | FK_Products    | CASCADE    | true         |

Scenario: Entity Sets 1
	Given a connection to the "SqlPlasticTestDB" database
	When I generate models with the default options
	Then the table "Customers" should contain the following entity sets
		| EntitySetName | KeyColumn  | ReferencedColumn | AssociatedRef | ForeignKeyName  | DeleteRule |
		| Orders        | CustomerID | OrderCustomerID  | Customer      | FK_Customers    | CASCADE    |
		| Preferences   | CustomerID | CustomerID_A     | Customer      | FK_CustomerID_A | NO_ACTION  |
		| Preferences1  | CustomerID | CustomerID_B     | Customer1     | FK_CustomerID_B | NO_ACTION  |
		| Preferences2  | CustomerID | CustomerID_C     | Customer2     | FK_CustomerID_C | NO_ACTION  |

Scenario: Column Attributes 1 - Primary Key with Identity
	Given a connection to the "SqlPlasticTestDB" database
	When I generate models with the default options
	Then the column "dbo.Customers.CustomerID" should have the following Column Attributes
		| AttributeName | AttributeValue          |
		| Storage       | "_CustomerID"           |
		| AutoSync      | AutoSync.OnInsert       |
		| DbType        | "Int NOT NULL IDENTITY" |
		| IsPrimaryKey  | true                    |
		| IsDbGenerated | true                    |

Scenario: Column Attributes 2 - Primary Key no Identity, table has TIMESTAMP
	Given a connection to the "SqlPlasticTestDB" database
	When I generate models with the default options
	Then the column "dbo.MyDataTypes.MyDataTypeID" should have the following Column Attributes
		| AttributeName | AttributeValue    |
		| Storage       | "_MyDataTypeID"   |
		| DbType        | "Int NOT NULL"    |
		| IsPrimaryKey  | true              |
		| UpdateCheck   | UpdateCheck.Never |

Scenario: Column Attributes 3 - TimeStamp
	Given a connection to the "SqlPlasticTestDB" database
	When I generate models with the default options
	Then the column "dbo.MyDataTypes.MyTimeStamp" should have the following Column Attributes
		| AttributeName | AttributeValue        |
		| Storage       | "_MyTimeStamp"        |
		| AutoSync      | AutoSync.Always       |
		| DbType        | "rowversion NOT NULL" |
		| CanBeNull     | false                 |
		| IsDbGenerated | true                  |
		| IsVersion     | true                  |
		| UpdateCheck   | UpdateCheck.Never     |

Scenario: Column Attributes 4 - Nullable int column
	Given a connection to the "SqlPlasticTestDB" database
	When I generate models with the default options
	Then the column "dbo.Employees.ManagerEmployeeID" should have the following Column Attributes
		| AttributeName | AttributeValue       |
		| Storage       | "_ManagerEmployeeID" |
		| DbType        | "Int"                |

Scenario: Column Attributes 5 - Nullable VARCHAR
	Given a connection to the "SqlPlasticTestDB" database
	When I generate models with the default options
	Then the column "dbo.Employees.EmployeeTitle" should have the following Column Attributes
		| AttributeName | AttributeValue   |
		| Storage       | "_EmployeeTitle" |
		| DbType        | "VarChar(200)"   |
		| CanBeNull     | true             |

Scenario: Column Attributes 6 - NonNullable VARCHAR
	Given a connection to the "SqlPlasticTestDB" database
	When I generate models with the default options
	Then the column "dbo.Employees.EmployeeName" should have the following Column Attributes
		| AttributeName | AttributeValue          |
		| Storage       | "_EmployeeName"         |
		| DbType        | "VarChar(100) NOT NULL" |
		| CanBeNull     | false                   |
