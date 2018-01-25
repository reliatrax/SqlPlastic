Feature: MappingRules
	Mapping rules can be used to configure EntitySets and EntityRefs

@MappingRules
Scenario: Foreign Key Mapping 1
	Given a connection to the "SqlPlasticTestDB" database
	And the following foreign key mapping rules for table "dbo.OrderLineItems"
		| ForeingKeyName  | EntityRefName | EntitySetName | DeleteOnNull |
		| FK_Orders       | MyOrder       | McOrdersSet   | true         |
	When I generate models with the default options
	Then the table "OrderLineItems" should contain the following entity references
		| EntityRefName | KeyColumn     | ReferencedColumn | AssociatedSet  | ForeignKeyName | DeleteRule | DeleteOnNull |
		| MyOrder       | LineOrderID   | OrderID          | McOrdersSet    | FK_Orders      | CASCADE    | true         |
		| Product       | LineProductID | ProductID        | OrderLineItems | FK_Products    | CASCADE    | true         |
	And the table "Orders" should contain the following entity sets
		| EntitySetName | KeyColumn | ReferencedColumn | AssociatedRef | ForeignKeyName | DeleteRule |
		| McOrdersSet   | OrderID   | LineOrderID      | MyOrder       | FK_Orders      | CASCADE    |

Scenario: Foreign Key Mapping 2
	Given a connection to the "SqlPlasticTestDB" database
	And the following foreign key mapping rules for table "dbo.Employees"
		| ForeingKeyName | EntityRefName | EntitySetName | DeleteOnNull |
		| FK_ManagerID   | MyBoss        | MyWorkers     | true         |
	When I generate models with the default options
	Then the table "Employees" should contain the following entity references
		| EntityRefName | KeyColumn         | ReferencedColumn | AssociatedSet | ForeignKeyName | DeleteRule | DeleteOnNull |
		| MyBoss        | ManagerEmployeeID | EmployeeID       | MyWorkers     | FK_ManagerID   | NO_ACTION  | true         |
	And the table "Employees" should contain the following entity sets
		| EntitySetName | KeyColumn  | ReferencedColumn  | AssociatedRef | ForeignKeyName | DeleteRule |
		| MyWorkers     | EmployeeID | ManagerEmployeeID | MyBoss        | FK_ManagerID   | NO_ACTION  |
