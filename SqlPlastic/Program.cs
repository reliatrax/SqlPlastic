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
            var colDescriptors = Querries.ListColumns();

            var primaryKeys = Querries.ListPrimaryKeys();
            var foreignKeys = Querries.ListForeignKeys();

            ModelBuilder mb = new ModelBuilder();
            var model = mb.BuildModel("MyDataBase", "MyContext", colDescriptors, primaryKeys, foreignKeys);

            CodeGenerator cg = new CodeGenerator();
            cg.CompileTemplates();
            string output = cg.RenderDataClasses(model);

            File.WriteAllText("dataclasses1.cs", output);
        }
    }
}
