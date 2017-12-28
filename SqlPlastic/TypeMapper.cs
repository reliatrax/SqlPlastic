using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlPlastic
{
    class TypeDescriptor
    {
        public string ClrTypeName { get; set; }
        public bool IsValueType { get; set; }

        public TypeDescriptor( string name, bool isValueType )
        {
            ClrTypeName = name;
            IsValueType = isValueType;
        }
    }

    static class TypeMapper
    {
        static readonly TypeDescriptor _bool = new TypeDescriptor("bool", true);
        static readonly TypeDescriptor _char = new TypeDescriptor("char", true);
        static readonly TypeDescriptor _byte = new TypeDescriptor("byte", true);
        static readonly TypeDescriptor _int16 = new TypeDescriptor("Int16", true);
        static readonly TypeDescriptor _int = new TypeDescriptor("int", true);
        static readonly TypeDescriptor _long = new TypeDescriptor("long", true);
        static readonly TypeDescriptor _decimal = new TypeDescriptor("decimal", true);
        static readonly TypeDescriptor _float = new TypeDescriptor("float", true);
        static readonly TypeDescriptor _double = new TypeDescriptor("double", true);
        static readonly TypeDescriptor _dateTime = new TypeDescriptor("System.DateTime", true);
        static readonly TypeDescriptor _dateTimeOffset = new TypeDescriptor("System.DateTimeOffset", true);
        static readonly TypeDescriptor _timeSpan = new TypeDescriptor("System.Timespan", true);
        static readonly TypeDescriptor _guid = new TypeDescriptor("System.GUID", true);

        static readonly TypeDescriptor _string = new TypeDescriptor("string", false);
        static readonly TypeDescriptor _xml = new TypeDescriptor("System.Xml.Linq.XElement", false);
        static readonly TypeDescriptor _binary = new TypeDescriptor("System.Data.Linq.Binary", false);

        static Dictionary<string, TypeDescriptor> typeMapping = new Dictionary<string, TypeDescriptor>( StringComparer.InvariantCultureIgnoreCase )
        {
            {"BIT", _bool },
            {"TINYINT", _byte },
            {"INT", _int },
            {"BIGINT", _long },
            {"SMALLMONEY", _decimal },
            {"MONEY", _decimal },
            {"DECIMAL", _decimal },
            {"NUMERIC", _decimal },
            {"REAL", _float },
            {"FLOAT", _double },

            {"SMALLDATETIME", _dateTime },
            {"DATETIME", _dateTime },
            {"DATETIME2", _dateTime },
            {"DATETIMEOFFSET", _dateTimeOffset },
            {"DATE", _dateTime },
            {"TIME", _timeSpan },

            {"CHAR",   _string },
            {"NCHAR", _string },
            {"VARCHAR", _string },
            {"NVARCHAR", _string },
            {"TEXT", _string},
            {"NTEXT", _string},
            {"XML", _xml},

            {"BINARY", _binary },
            {"VARBINARY", _binary },
            {"IMAGE", _binary },
            {"TIMESTAMP", _binary },

            {"UNIQUEIDENTIFIER", _guid }
        };

        public static TypeDescriptor LookupDBType(ColumnDescriptor cd)
        {
            string dbtype = cd.DataType.ToUpper();

            if ((dbtype == "CHAR" || dbtype == "NCHAR") && cd.MaxLength == 1)
                return _char;
            else
                return typeMapping[dbtype];
        }

        public static string MapDBType(ColumnDescriptor cd)
        {
            TypeDescriptor td;

            string dbtype = cd.DataType.ToUpper();

            if ((dbtype == "CHAR" || dbtype == "NCHAR") && cd.MaxLength == 1)
                td = _char;
            else
                td = typeMapping[dbtype];

            if (cd.IsNullable && td.IsValueType)
                return $"System.Nullable<{td.ClrTypeName}>";

            return td.ClrTypeName;
        }

        // --- REVERSE MAPPING (CLR TO SQL) ---

        public static string BuildDBType(ColumnDescriptor c)
        {
            string dbtype = RegularizeDBType(c.DataType);

            dbtype += FormatLengthPrecision(c);     // add (100), (MAX) to strings, binaries, DECIMALS...

            if (c.IsNullable == false)
                dbtype += " NOT NULL";
            if (c.IsIdentity)
                dbtype += " IDENTITY";

            return dbtype;
        }

        private static string RegularizeDBType(string dataType)
        {
            // Look up exceptions in the dictionary
            if (dbTypeRegularizerDict.TryGetValue(dataType.ToUpper(), out string s))
                return s;

            // By default, SqlMetal seems to just capitalize the first character
            if (dataType.Length >= 1)
                return dataType.Substring(0, 1).ToUpper() + dataType.Substring(1).ToLower();

            return dataType;
        }

        static string FormatLengthPrecision(ColumnDescriptor c)
        {
            string fmtstr(int bytesPerChar)
            {
                return (c.MaxLength < 0) ? "(MAX)" : $"({c.MaxLength / bytesPerChar})";
            }

            switch (c.DataType.ToUpper())
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
                    return $"({c.Precision},{c.Scale})";

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
            {"DATETIME2", "DateTime2(7)" },
            {"DATETIMEOFFSET", "DateTimeOffset(7)" },
            {"TIME", "Time(7)" },

            {"VARCHAR", "VarChar"},
            {"NVARCHAR", "NVarChar"},
            {"NTEXT", "NText"},

            {"VARBINARY", "VarBinary" },
            {"TIMESTAMP", "rowversion" },

            {"UNIQUEIDENTIFIER", "UniqueIdentifier" },
        };
    }
}
