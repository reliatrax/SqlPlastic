using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlPlastic
{
    public class ConfigRoot
    {
        public TableMappingRules[] mappingRules { get; set; }
    }

    public class TableMappingRules
    {
        /// <summary>
        /// Complete table name including the schema (e.g. "dbo.Products")
        /// </summary>
        public string tableName { get; set; }
        public ForeignKeyRuleMapping[] foreignKeys { get; set; }

        public TableMappingRules()
        {
            tableName = "";
            foreignKeys = new ForeignKeyRuleMapping[0];
        }
    }

    public class ForeignKeyRuleMapping
    {
        public string foreignKeyName { get; set; }
        public string entityRefName { get; set; }
        public string entitySetName { get; set; }
        public bool deleteOnNull { get; set; }
    }

    public class PlasticConfig
    {
        public Dictionary<string, TableMappingRules> MappingRules;

        public PlasticConfig()
        {
            MappingRules = new Dictionary<string, TableMappingRules>();
        }

        public void ReadJsonConfig( string fileName )
        {
            string json = File.ReadAllText(fileName);

            ConfigRoot config = JsonConvert.DeserializeObject<ConfigRoot>(json);

            MappingRules = config.mappingRules.ToDictionary(x => x.tableName, StringComparer.InvariantCultureIgnoreCase);
        }

    }
}
