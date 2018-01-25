using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlPlastic
{
    public class ConfigRoot
    {
        public OutputOptions Options {get; set;} 

        public TableMappingRules[] MappingRules { get; set; }
    }

    public class OutputOptions
    {
        public string NameSpace { get; set; }
        public string DataContextName { get; set; }
        public bool MaxLengthAttributes { get; set; }
    }

    public class TableMappingRules
    {
        /// <summary>
        /// Complete table name including the schema (e.g. "dbo.Products")
        /// </summary>
        public string TableName { get; set; }
        public ForeignKeyMappingRule[] ForeignKeys { get; set; }

        public TableMappingRules()
        {
            TableName = "";
            ForeignKeys = new ForeignKeyMappingRule[0];
        }
    }

    public class ForeignKeyMappingRule
    {
        public string ForeignKeyName { get; set; }
        public string EntityRefName { get; set; }
        public string EntitySetName { get; set; }
        public bool? DeleteOnNull { get; set; }
    }

    public class PlasticConfig
    {
        public OutputOptions Options;

        public Dictionary<string, TableMappingRules> MappingRules;

        public PlasticConfig()
        {
            MappingRules = new Dictionary<string, TableMappingRules>();

            SetDefaultOptions();
        }

        private void SetDefaultOptions()
        {
            // Provide some default options
            Options = Options ?? new OutputOptions();

            if (string.IsNullOrEmpty(Options.DataContextName))
                Options.DataContextName = "MyDataContext";

            if (string.IsNullOrEmpty(Options.NameSpace))
                Options.NameSpace = "MyDataModels";
        }

        public void ReadJsonConfig( string fileName )
        {
            string json = File.ReadAllText(fileName);

            ConfigRoot config = JsonConvert.DeserializeObject<ConfigRoot>(json);

            Options = config.Options;
            MappingRules = config.MappingRules.ToDictionary(x => x.TableName, StringComparer.InvariantCultureIgnoreCase);

            // Set default values after reading
            SetDefaultOptions();
        }
    }
}
