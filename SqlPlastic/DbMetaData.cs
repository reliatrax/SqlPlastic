using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlPlastic
{
    public class DbMetaData
    {
        public string DBName { get; set; }

        public ColumnDescriptor[] Columns { get; set; }
        public PrimaryKeyDescriptor[] PrimaryKeys { get; set; }
        public ForeignKeyDescriptor[] ForeignKeys { get; set; }
    }

    public class ColumnDescriptor
    {
        public string TableName { get; set; }
        public string SchemaName { get; set; }
        public int ObjectID { get; set; }
        public int ColumnID { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public int MaxLength { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsComputed { get; set; }

        public bool IsVersion => DataType.ToUpper() == "TIMESTAMP";

        public bool IsDbGenerated => IsComputed || IsIdentity || IsVersion;
    }

    public class PrimaryKeyDescriptor
    {
        public string PrimaryKeyName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int TableObjectID { get; set; }
        public int ColumnID { get; set; }
    }

    public class ForeignKeyDescriptor
    {
        public string ForeignKeyName { get; set; }
        public string OnDelete { get; set; }
        public string OnUpdate { get; set; }

        public string ParentTableName { get; set; }
        public string ParentColumnName { get; set; }
        public int ParentObjectID { get; set; }
        public int ParentColumnID { get; set; }

        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }
        public int ReferencedObjectID { get; set; }
        public int ReferencedColumnID { get; set; }
    }
}
