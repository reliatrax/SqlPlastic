using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlPlastic
{
    public class ModelBuilder
    {
        AssociationBuilder AssocBuilder;
        PlasticConfig Config;

        public ModelBuilder(PlasticConfig config )
        {
            Config = config;

            AssocBuilder = new AssociationBuilder(config);
        }

        public DataClassesModel BuildModel(DbMetaData meta)
        {
            return new DataClassesModel
            {
                ToolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                DatabaseName = meta.DBName,
                NameSpace = Config.Options.NameSpace,
                ContextName = Config.Options.DataContextName,
                Tables = BuildTables(meta.Columns, meta.PrimaryKeys, meta.ForeignKeys)
            };
        }

        public Table[] BuildTables(ColumnDescriptor[] columnDescriptors, PrimaryKeyDescriptor[] pks, ForeignKeyDescriptor[] fks)
        {
            List<Table> tables = new List<Table>();

            // Group the columns by table
            var colsByTable = columnDescriptors.ToLookup(x => x.ObjectID);

            var pksByTableID = pks.ToLookup(x => x.TableObjectID);            // A compound primary key will result in multiple columns marked as "IsPrimary"

            // Group the columns and build the tables
            foreach (var cbt in colsByTable)
            {
                string tableName = cbt.First().TableName;
                string schemaName = cbt.First().SchemaName;
                int tableID = cbt.First().ObjectID;
                string className = GenerateClassName(tableName);
                bool tableHasVersion = cbt.Any(x => x.IsVersion);

                PrimaryKeyDescriptor[] tblPKs = pksByTableID[tableID].ToArray();        // primary keys for this table

                var columns = cbt.OrderBy(x => x.ColumnID).Select(c => BuildColumn(tableID, c, tblPKs, tableHasVersion)).ToArray();

                Table tbl = new Table
                {
                    ClassName = className,
                    SchemaName = schemaName,
                    TableName = tableName,
                    TableObjectID = tableID,
                    Columns = columns,
                    EntityRefs = new List<EntityRefModel>(),
                    EntitySets = new List<EntitySetModel>()
                };

                foreach (Column c in tbl.Columns)
                    c.Table = tbl;

                tables.Add(tbl);
            }

            // Lay in the EntityRef and EntitySet foreign key association properties (the "." properties)
            AssocBuilder.AddAssociationProperties(tables, fks);

            return tables.OrderBy(x => x.SchemaName).ThenBy(x => x.TableName).ToArray();
        }

        //# todo - move this somewhere!!!
        //# todo - allow mapping from JSON config file
        public static string GenerateClassName( string tableName )
        {
            string className = Inflector.Singularize(tableName);
            return className;
        }

        private Column BuildColumn(int tableObjectID, ColumnDescriptor c, PrimaryKeyDescriptor[] pks, bool tableHasVersion)
        {
            TypeDescriptor td = TypeMapper.MapDBType(c);
            string memberName = c.ColumnName;
            string memberType = td.ClrTypeName;

            bool isPrimaryKey = pks.Any(x => x.ColumnID == c.ColumnID);

            // Render MaxLength attributes for Strings and Binary if the config file tells us too, but not for Timestamps!
            bool hasLength = td.IsValueType == false && c.IsVersion == false && c.MaxLength > 0;       // Timestamps must not have a MaxLenth attribute (breaks model binder!)
            string maxLengthAttr = "";
            if (Config.Options.MaxLengthAttributes && hasLength )
                maxLengthAttr = c.MaxLength.ToString();

            return new Column
            {
                MemberName = memberName,
                MemberType = memberType,

                ColumnUID = new ColumnUID( tableObjectID, c.ColumnID ),
                ColumnName = c.ColumnName,
                DataType = c.DataType,
                IsNullable = c.IsNullable,
                IsPrimaryKey = isPrimaryKey,
                MaxLength = c.MaxLength,
                Precision = c.Precision,
                Scale = c.Scale,

                MaxLengthAttr = maxLengthAttr,
                ColumnAttributeArgs = BuildColAttrArgs(c, tableHasVersion, isPrimaryKey)
            };
        }

        private static OrderedDictionary<string,string> BuildColAttrArgs(ColumnDescriptor c, bool tableHasVersion, bool isPrimaryKey)
        {
            // Storage="_AnsweringMachineStatID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true, UpdateCheck=UpdateCheck.Never

            TypeDescriptor td = TypeMapper.LookupDBType(c);

            OrderedDictionary<string,string> args = new OrderedDictionary<string, string>();

            args.Add("Storage", $"\"_{c.ColumnName}\"");

            // Default value is Never
            if (c.IsIdentity)
                args.Add("AutoSync", "AutoSync.OnInsert");
            else if (c.IsVersion)
                args.Add("AutoSync", "AutoSync.Always");

            string dbtype = TypeMapper.BuildDBType(c);
            args.Add("DbType", $"\"{dbtype}\"");

            // I'm copying the behavior of SqlMetal here even though it doesn't really make sense to me
            // SqlMetal only outputs this attribute for Reference types (e.g. string and binary) and outputs both true and false (even though true is the default).
            // In particular, it does not output this attribute for nullable value types like "INT NULL".  Weird.
            if (td.IsValueType == false)
                args.Add("CanBeNull", $"{(c.IsNullable ?"true" :"false")}");

            if (isPrimaryKey)
                args.Add("IsPrimaryKey", "true");

            if (c.IsDbGenerated)
                args.Add("IsDbGenerated", "true");

            if (c.IsVersion)
                args.Add("IsVersion", "true");

            // Default Value of UpdateCheck is "Always" unless a member has IsVersion==true
            // Remarks When this property is used with one of three enums, it determines how LINQ to SQL detects concurrency conflicts.
            // If no member is designed as IsVersion = true, original member values are compared with the current database state.
            if (tableHasVersion)
                args.Add("UpdateCheck", "UpdateCheck.Never");

            return args;
        }
    }
}
