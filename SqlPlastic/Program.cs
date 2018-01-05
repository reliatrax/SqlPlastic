using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlPlastic
{
    class Program
    {
        static void Main(string[] args)
        {
            string dbname = null, server = ".\\SQLEXPRESS", outputFileName = null, jsonFileName = null;

            // thses are the available options, not that they set the variables
            var options = new OptionSet {
                { "server=", "The name of the SQL Server (eg. \".\\SQLEXPRESS\")", s => server = s },
                { "database=", "The name of the database to generate Linq-to-SQL classes for", d => dbname = d },
                { "code=", "the output file name", c => outputFileName = c },
                { "config=", "filename for the JSON configuration file (optional)", n => jsonFileName = n },
            };

            List<string> extra;
            try
            {
                // parse the command line
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                // output some error message
                Console.WriteLine("Invalid command line arguments: {0}", e.Message);
                return;
            }

            // Read the JSON configuration file (if there is one)
            PlasticConfig plastic = new PlasticConfig();

            if (jsonFileName != null)
                plastic.ReadJsonConfig(jsonFileName);

            // Query the database meta-data
            string constring = String.Format(@"Data Source =.\sqlexpress; Integrated Security = SSPI; DataBase = {0}", dbname);
            DbMetaData dbMetaData = QuerryRunner.QueryDbMetaData(dbname, constring);

            // Build up a DOM model of the database and its relationships
            ModelBuilder mb = new ModelBuilder(plastic);
            var model = mb.BuildModel(dbMetaData);

            // Generate the output code from the model
            string exePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            string templatePath = Path.Combine(exePath, "Templates");
            CodeGenerator cg = new CodeGenerator();
            cg.CompileTemplates(templatePath);
            string output = cg.RenderDataClasses(model);

            File.WriteAllText(outputFileName, output);
        }
    }
}
