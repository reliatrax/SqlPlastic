Feature: SpecFlowFeature1
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: All tables, columns, and keys should be present
	Given I generate models from the test database
	Then the resulting model should contain the following tables
	| TableName      | Columns | EntityRefs | EntitySets |
	| Customers      | 3       | 0          | 4          |
	| Products       | 3       | 0          | 1          |
	| Orders         | 3       | 1          | 1          |
	| OrderLineItems | 4       | 2          | 0          |
	| Employees      | 3       | 1          | 1          |
	| Preferences    | 4       | 3          | 0          |
	| MyDataTypes    | 29      | 0          | 0          |
