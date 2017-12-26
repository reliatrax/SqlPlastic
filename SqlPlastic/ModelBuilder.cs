using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlPlastic
{
    public class ModelBuilder
    {
        public DataClassesModel BuildModel(string dbName, string contextName, ColumnDescriptor[] columnDescriptors, PrimaryKeyDescriptor[] pks, ForeignKeyDescriptor[] fks)
        {
            return new DataClassesModel
            {
                DatabaseName = dbName,
                ContextName = contextName,
                Tables = BuildTables(columnDescriptors, pks, fks)
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
                string className = Inflector.Singularize(tableName);
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
            AssociationBuilder.AddAssociationProperties(tables, fks);

            return tables.OrderBy(x => x.SchemaName).ThenBy(x => x.TableName).ToArray();
        }


        private static Column BuildColumn(int tableObjectID, ColumnDescriptor c, PrimaryKeyDescriptor[] pks, bool tableHasVersion)
        {
            string memberName = c.ColumnName;
            string memberType = MapDBType(c);

            if (c.IsNullable && IsNullableType(memberType))
                memberType = $"System.Nullable<{memberType}>";

            bool isPrimaryKey = pks.Any(x => x.ColumnID == c.ColumnID);

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

                ColumnAttributeArgs = BuildColAttrArgs(c, tableHasVersion, isPrimaryKey)
            };
        }

        // #TODO - refactor so that these are all properties and the arg string is set by the properties
        private static string BuildColAttrArgs(ColumnDescriptor c, bool tableHasVersion, bool isPrimaryKey)
        {
            // Storage="_AnsweringMachineStatID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true, UpdateCheck=UpdateCheck.Never

            List<string> args = new List<string>();

            args.Add($"Storage=\"_{c.ColumnName}\"");       // #TODO - function to get member name from column descriptor

            // Default value is Never
            if (c.IsIdentity)
                args.Add("AutoSync=AutoSync.OnInsert");
            else if (c.IsVersion)
                args.Add("AutoSync=AutoSync.Always");

            string dbtype = BuildDBType(c);
            args.Add($"DbType=\"{dbtype}\"");

            if (c.IsNullable == false)
                args.Add("CanBeNull=false");

            if (isPrimaryKey)
                args.Add("IsPrimaryKey=true");

            if (c.IsDbGenerated)
                args.Add("IsDbGenerated=true");

            if (c.IsVersion)
                args.Add("IsVersion=true");

            // Default Value of UpdateCheck is "Always" unless a member has IsVersion==true
            // Remarks When this property is used with one of three enums, it determines how LINQ to SQL detects concurrency conflicts.
            // If no member is designed as IsVersion = true, original member values are compared with the current database state.
            if (tableHasVersion)
                args.Add("UpdateCheck=UpdateCheck.Never");

            return string.Join(", ", args);
        }

        static string BuildDBType(ColumnDescriptor c)
        {
            string dbtype = RegularizeDBType(c.DataType);

            dbtype += FormatLengthPrecision(c);     // add (100), (MAX) to strings, binaries, DECIMALS...

            if (c.IsNullable == false)
                dbtype += " NOT NULL";
            if (c.IsIdentity)
                dbtype += " IDENTITY";

            return dbtype;
        }

        static HashSet<string> nullableTypes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) {
            "bool",
            "Int16",
            "int",
            "Int64",
            "decimal",
            "float",
            "double",
            "System.DateTime",
            "System.DateTimeOffset",
            "System.TimeSpan",
            "Guid"
        };

        private static bool IsNullableType(string dbType)
        {
            return nullableTypes.Contains(dbType);
        }

        private static string RegularizeDBType(string dataType)
        {
            // Look up exceptions in the dictionary
            if (dbTypeRegularizerDict.TryGetValue(dataType.ToUpper(), out string s))
                return s;

            // By default, SqlMetal seems to just capitalize the first character
            if (dataType.Length >= 1)
                return dataType.Substring(0,1).ToUpper() + dataType.Substring(1).ToLower();

            return dataType;
        }

        static string FormatLengthPrecision(ColumnDescriptor c )
        {
            string fmtstr(int bytesPerChar)
            {
                return (c.MaxLength < 0) ?"(MAX)" :$"({c.MaxLength/bytesPerChar})";
            }

            switch( c.DataType.ToUpper() )
            {
                case "CHAR":
                case "VARCHAR":
                case "TEXT":
                    return fmtstr(1);

                case "NCHAR":
                case "NVARCHAR":
                case "NTEXT":
                    return fmtstr(2);

                case "BINARY":
                case "VARBINARY":
                case "IMAGE":
                    return fmtstr(1);

                case "DECIMAL":
                    return $"({c.Scale},{c.Precision})";

                default:
                    return "";
            }

        }

        // Match the casing of SqlMetal
        static Dictionary<string, string> dbTypeRegularizerDict = new Dictionary<string, string>()
        {
            {"TINYINT", "TinyInt" },
            {"BIGINT", "BigInt" },
            {"SMALLMONEY", "SmallMoney" },

            {"SMALLDATETIME", "SmallDateTime" },
            {"DATETIME", "DateTime" },
            {"DATETIME2", "DateTime2" },
            {"DATETIMEOFFSET", "DateTimeOffset" },

            {"VARCHAR", "VarChar"},
            {"NVARCHAR", "NVarChar"},
            {"NTEXT", "NText"},

            {"VARBINARY", "VarBinary" },
            {"TIMESTAMP", "rowversion" },

            {"UNIQUEIDENTIFIER", "Guid" },
        };


        static Dictionary<string, string> typeMapping = new Dictionary<string, string>()
        {
            {"BIT", "bool" },
            {"TINYINT", "Int16" },
            {"INT", "int" },
            {"BIGINT", "Int64" },
            {"SMALLMONEY", "decimal" },
            {"MONEY", "decimal" },
            {"DECIMAL", "decimal" },
            {"NUMERIC", "decimal" },
            {"REAL", "float" },
            {"FLOAT", "double" },

            {"SMALLDATETIME", "System.DateTime" },
            {"DATETIME", "System.DateTime" },
            {"DATETIME2", "System.DateTime" },
            {"DATETIMEOFFSET", "System.DateTimeOffset" },
            {"DATE", "System.DateTime" },
            {"TIME", "System.TimeSpan" },

            {"CHAR",   "string" },
            {"NCHAR", "string"},
            {"VARCHAR", "string"},
            {"NVARCHAR", "string"},
            {"TEXT", "string"},
            {"NTEXT", "string"},
            {"XML", "Xml.Linq.XElement"},

            {"BINARY", "System.Data.Linq.Binary" },
            {"VARBINARY", "System.Data.Linq.Binary" },
            {"IMAGE", "System.Data.Linq.Binary" },
            {"TIMESTAMP", "System.Data.Linq.Binary" },

            {"UNIQUEIDENTIFIER", "Guid" },
            {"SQL_VARIANT", "Object" },
        }; 

        public static string MapDBType(ColumnDescriptor cd)
        {
            string dbtype = cd.DataType.ToUpper();

            if ((dbtype == "CHAR" || dbtype == "NCHAR") && cd.MaxLength == 1)
                return "char";

            return typeMapping[dbtype];
        }
    }
}
