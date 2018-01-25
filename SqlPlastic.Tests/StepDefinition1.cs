using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

using SpecTable = TechTalk.SpecFlow.Table;

namespace SqlPlastic.Tests
{
    [Binding]
    public sealed class StepDefinition1
    {
        string constring;
        PlasticConfig config = new PlasticConfig();     // Default configuration
        DataClassesModel model;

        Table FindTable( string fqTableName )
        {
            string[] parts = fqTableName.Split('.');

            Table tbl = model.Tables.SingleOrDefault(x => x.SchemaName == parts[0] && x.TableName == parts[1]);

            if (tbl == null)
                throw new ArgumentException($"Table '{fqTableName}' not found");
            return tbl;
        }

        Column FindColumn(string fqColumnName)
        {
            string[] parts = fqColumnName.Split('.');
            (string schema, string tableName, string columnName) = (parts[0], parts[1], parts[2]);

            Table tbl = FindTable(schema + "." + tableName);
            Column col = tbl.Columns.SingleOrDefault(x => x.ColumnName == columnName);

            if( col == null)
                throw new ArgumentException($"Column '{fqColumnName}' not found");

            return col;
        }

        [Given(@"a connection to the ""(.*)"" database")]
        public void GivenAConnectionToTheDatabase(string dataBaseName)
        {
            constring = string.Format(@"Data Source =.\sqlexpress; Integrated Security = SSPI; DataBase = {0}", dataBaseName);
        }

        class ForeignKeyMappingDescription
        {
            public string ForeingKeyName { get; set; }
            public string EntityRefName { get; set; }
            public string EntitySetName { get; set; }
            public bool DeleteOnNull { get; set; }
        }

        [Given(@"the following foreign key mapping rules for table ""(.*)""")]
        public void GivenTheFollowingForeignKeyMappingRulesForTable(string tableName, SpecTable tbl)
        {
            var foreignKeyMappings = tbl.CreateSetChecked<ForeignKeyMappingDescription>();

            var fks = foreignKeyMappings.Select(x => new ForeignKeyMappingRule
                        {
                            ForeignKeyName = x.ForeingKeyName,
                            EntityRefName = x.EntityRefName,
                            EntitySetName = x.EntitySetName,
                            DeleteOnNull = x.DeleteOnNull
                        }).ToArray();

            TableMappingRules tmrs = new TableMappingRules
            {
                TableName = tableName,
                ForeignKeys = fks
            };

            config.MappingRules.Add(tableName, tmrs);
        }

        [When(@"I generate models with the default options")]
        public void WhenIGenerateModelsWithTheDefaultOptions()
        {
            // Query the database meta-data
            DbMetaData dbMetaData = QuerryRunner.QueryDbMetaData(constring);


            // Build up a DOM model of the database and its relationships
            ModelBuilder mb = new ModelBuilder(config);
            model = mb.BuildModel(dbMetaData);
        }


        class TableDescription
        {
            public string TableName { get; set; }
            public int Columns { get; set; }
            public int EntityRefs { get; set; }
            public int EntitySets { get; set; }
        }

        [Then(@"the resulting model should contain the following tables")]
        public void ThenTheResultingModelShouldContainTheFollowingTables(SpecTable tbl)
        {
            var tds = tbl.CreateSetChecked<TableDescription>();

            string[] modelTableNames = model.Tables.Select(x => x.TableName).ToArray();
            string[] expectedTableNames = tds.Select(x => x.TableName).ToArray();

            // Compare the table names (this will give nice error messages for missing / additional tables)
            modelTableNames.ShouldBeEquivalentTo(expectedTableNames);

            // Compare the column and key counts
            Dictionary<string, TableDescription> expectedTablesDict = tds.ToDictionary(x => x.TableName);

            foreach (var table in model.Tables)
            {
                var expected = expectedTablesDict[table.TableName];     // will be present due to the assertion above

                table.Columns.Should().HaveCount(expected.Columns, "Table [{0}] Column count", table.TableName);
                table.EntitySets.Should().HaveCount(expected.EntitySets, "Table [{0}] EntitySet count", table.TableName);
                table.EntityRefs.Should().HaveCount(expected.EntityRefs, "Table [{0}] EntityRef count", table.TableName);
            }
        }

        public class ColumnDescription
        {
            public string ColumnName { get; set; }
            public string MemberType { get; set; }
        }

        [Then(@"the resulting table ""(.*)"" should contain exactly the following columns")]
        public void ThenTheResultingTableShouldContainExactlyTheFollowingColumns(string tableName, SpecTable tbl)
        {
            Dictionary<string, ColumnDescription> expectedColsDict = tbl.CreateSetChecked<ColumnDescription>().ToDictionary(x => x.ColumnName);

            Table table = model.Tables.Single(x => x.TableName == tableName);

            // Check that all the column names are present and accounted for
            string[] actualColumnNames = table.Columns.Select(x => x.ColumnName).ToArray();
            actualColumnNames.Should().BeEquivalentTo(expectedColsDict.Keys, "[{0}.{1}]", table.SchemaName, table.TableName);

            foreach (Column c in table.Columns)
            {
                var expectedColumn = expectedColsDict[c.ColumnName];

                c.MemberType.Should().Be(expectedColumn.MemberType, "{0}.{1}", c.Table.TableName, c.ColumnName);
            }
        }

        public class AttributeDescription
        {
		    public string AttributeName { get; set; }
            public string AttributeValue { get; set; }
        }

        [Then(@"the column ""(.*)"" should have the following Column Attributes")]
        public void ThenTheColumnShouldHaveTheFollowingColumnAttributes(string fqColumnName, SpecTable tbl)
        {
            AttributeDescription[] expectedAttrs = tbl.CreateSetChecked<AttributeDescription>().ToArray();
            Dictionary<string, string> expectedAttrsDict = expectedAttrs.ToDictionary(x => x.AttributeName, x => x.AttributeValue);   // convert to Dictionary<string,string>

            Column c = FindColumn(fqColumnName);

            // Compare the keys (this gives much better error messages for additional / missing keys)
            c.ColumnAttributeArgs.Keys.Should().BeEquivalentTo(expectedAttrsDict.Keys, fqColumnName);

            // Compare the attribute values
            c.ColumnAttributeArgs.ShouldBeEquivalentTo(expectedAttrsDict, fqColumnName);
        }

        public class EntityRefDescription
        {
            public string EntityRefName { get; set; }

            public string KeyColumn { get; set; }
            public string ReferencedColumn { get; set; }

            public string AssociatedSet { get; set; }
            public string ForeignKeyName { get; set; }
            public string DeleteRule { get; set; }
            public string DeleteOnNull { get; set; }
        }

        [Then(@"the table ""(.*)"" should contain the following entity references")]
        public void ThenTheTableShouldContainTheFollowingEntityReferences(string tableName, SpecTable tbl)
        {
            Dictionary<string, EntityRefDescription> expectedEntityRefs = tbl.CreateSetChecked<EntityRefDescription>().ToDictionary(x => x.EntityRefName);

            Table table = model.Tables.Single(x => x.TableName == tableName);

            // Check that all the Associations names are present and accounted for
            string[] actualEntityRefNames = table.EntityRefs.Select(x => x.EntityRefName).ToArray();

            actualEntityRefNames.Should().BeEquivalentTo(expectedEntityRefs.Keys, "[{0}.{1}] Entity Ref Names", table.SchemaName, table.TableName);

            // Compare all the properties of the association
            foreach (EntityRefModel ef in table.EntityRefs)
            {
                var expected = expectedEntityRefs[ef.EntityRefName];

                string because = $"[{table.SchemaName}.{table.TableName}.{ef.EntityRefName}]";

                ef.KeyColumn.ColumnName.Should().Be(expected.KeyColumn, because + " KeyColumn");
                ef.ReferencedColumn.ColumnName.Should().Be(expected.ReferencedColumn, because + " RefColumn");
                ef.AssociatedSet.EntitySetName.Should().Be(expected.AssociatedSet, because + " AssociatedSet");
                ef.ForeignKeyName.Should().Be(expected.ForeignKeyName, because + " ForeignKeyName");
                ef.DeleteRule.Should().Be(expected.DeleteRule, because + " DeleteRule");
                ef.DeleteOnNull.Should().Be(expected.DeleteOnNull, because + " DeleteOnNull");
            }
        }

        public class EntitySetDescription
        {
            public string EntitySetName { get; set; }

            public string KeyColumn { get; set; }
            public string ReferencedColumn { get; set; }

            public string AssociatedRef { get; set; }
            public string ForeignKeyName { get; set; }
            public string DeleteRule { get; set; }
        }

        [Then(@"the table ""(.*)"" should contain the following entity sets")]
        public void ThenTheTableShouldContainTheFollowingEntitySets(string tableName, SpecTable tbl)
        {
            Dictionary<string, EntitySetDescription> expectedEntitySets = tbl.CreateSetChecked<EntitySetDescription>().ToDictionary(x => x.EntitySetName);

            Table table = model.Tables.Single(x => x.TableName == tableName);

            // Check that all the Associations names are present and accounted for
            string[] actualEntitySetNames = table.EntitySets.Select(x => x.EntitySetName).ToArray();

            actualEntitySetNames.Should().BeEquivalentTo(expectedEntitySets.Keys, "[{0}.{1}] EntitySet Names", table.SchemaName, table.TableName);

            // Compare all the properties of the association
            foreach (EntitySetModel ef in table.EntitySets)
            {
                var expected = expectedEntitySets[ef.EntitySetName];

                string because = $"[{table.SchemaName}.{table.TableName}.{ef.EntitySetName}]";

                ef.KeyColumn.ColumnName.Should().Be(expected.KeyColumn, because + " KeyColumn");
                ef.ReferencedColumn.ColumnName.Should().Be(expected.ReferencedColumn, because + " ReferencedColumn");
                ef.AssociatedRef.EntityRefName.Should().Be(expected.AssociatedRef, because + " AssociatedRef");
                ef.ForeignKeyName.Should().Be(expected.ForeignKeyName, because + " ForeignKeyName");
                ef.DeleteRule.Should().Be(expected.DeleteRule, because + " DeleteRule");
            }
        }
    }
}
