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
        DataClassesModel model;

        [Given(@"I generate models from the test database")]
        public void GivenIGenerateModelsFromTheTestDatabase()
        {
            // Query the database meta-data
            string dbName = "SqlPlasticTestDB";
            string constring = string.Format( @"Data Source =.\sqlexpress; Integrated Security = SSPI; DataBase = {0}", dbName);
            DbMetaData dbMetaData = QuerryRunner.QueryDbMetaData(dbName, constring);

            PlasticConfig config = new PlasticConfig();

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
            Dictionary<string,TableDescription> expectedTablesDict = tds.ToDictionary(x => x.TableName);

            foreach( var table in model.Tables )
            {
                var expected = expectedTablesDict[table.TableName];     // will be present due to the assertion above

                table.Columns.Should().HaveCount(expected.Columns, "Table [{0}] Column count", table.TableName);
                table.EntitySets.Should().HaveCount(expected.EntitySets, "Table [{0}] EntitySet count", table.TableName);
                table.EntityRefs.Should().HaveCount(expected.EntityRefs, "Table [{0}] EntityRef count", table.TableName);
            }
        }
    }
}
