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
            string dbname = null, server = null, contextName = null, outputFileName = null, gennamespace = null;

            // thses are the available options, not that they set the variables
            var options = new OptionSet {
                { "server=", "the number of times to repeat the greeting.", s => server = s },
                { "database=", "the name of someone to greet.", d => dbname = d },
                { "context=", "the name of someone to greet.", c => contextName = c },
                { "code=", "the name of someone to greet.", c => outputFileName = c },
                { "namespace=", "the namespace for the generated context class", n => gennamespace = n },
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

            var colDescriptors = Querries.ListColumns();

            var primaryKeys = Querries.ListPrimaryKeys();
            var foreignKeys = Querries.ListForeignKeys();

            ModelBuilder mb = new ModelBuilder();
            var model = mb.BuildModel(dbname, gennamespace, contextName, colDescriptors, primaryKeys, foreignKeys);

            CodeGenerator cg = new CodeGenerator();
            cg.CompileTemplates();
            string output = cg.RenderDataClasses(model);

            File.WriteAllText(outputFileName, output);
        }
    }
}
