using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlPlastic
{

    public class CodeGenerator
    {
        Func<DataClassesModel, string> renderer;

        public void CompileTemplates(string templatePath)
        {
            string tblTemplate = File.ReadAllText(Path.Combine(templatePath,"table.handlebars"));

            string dcTemplate = File.ReadAllText(Path.Combine(templatePath,"dataclasses.handlebars"));

            Handlebars.Configuration.TextEncoder = new NoTxtEncoder();  // Don't HTML encode {{ ... }} values, since we're not generating HTML

            Handlebars.Configuration.ThrowOnUnresolvedBindingExpression = true;

            Handlebars.RegisterTemplate("table", tblTemplate);

            renderer = Handlebars.Compile(dcTemplate);
        }

        public string RenderDataClasses( DataClassesModel model )
        {
            string dc = renderer(model);

            return dc;
        }
    }

    public class NoTxtEncoder : ITextEncoder
    {
        string ITextEncoder.Encode(string value)
        {
            return value;
        }
    }
}
