using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlPlastic
{
    public class DataClassesModel
    {
        public string DatabaseName { get; set; }
        public string ContextName { get; set; }

        public Table[] Tables { get; set; }
        public string NameSpace { get; internal set; }
    }

    public class Table
    {
        public string ClassName { get; set; }

        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public int TableObjectID { get; set; }       // the Table ID

        public Column[] Columns { get; set; }

        public List<EntityRefModel> EntityRefs { get; set; }

        public List<EntitySetModel> EntitySets { get; set; }
    }

    public class Column
    {
        public string MemberName { get; set; }
        public string MemberType { get; set; }

        public ColumnUID ColumnUID { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public int MaxLength { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey => ForeignKey != null;

        public string ColumnAttributeArgs { get; set; }

        public string FullName { get; set; }
        public Table Table { get; set; }
        public EntityRefModel ForeignKey { get; set; }
    }

    public struct ColumnUID
    {
        public int TableObjectID { get; private set; }
        public int ColumnID { get; private set; }

        public ColumnUID(int tableObjectID, int columnID)
        {
            TableObjectID = tableObjectID;
            ColumnID = columnID;
        }
    }

    public class EntityRefModel
    {
        public string EntityRefName { get; set; }

        public Column KeyColumn { get; set; }
        public Column ReferencedColumn { get; set; }

        public EntitySetModel AssociatedSet { get; set; }
        public string ForeignKeyName { get; set; }
        public string DeleteRule { get; set; }
        public string DeleteOnNull { get; set; }        // "true" if this is a non-nullable foreign key.  See: https://blogs.msdn.microsoft.com/dinesh.kulkarni/2008/05/11/linq-to-sql-tips-4-use-deleteonnull-if-you-want-to-delete-object-with-null-fk/
    }

    public class EntitySetModel
    {
        public string EntitySetName { get; set; }

        public Column KeyColumn { get; set; }
        public Column ReferencedColumn { get; set; }

        public EntityRefModel AssociatedRef { get; set; }
        public string ForeignKeyName { get; set; }
        public string DeleteRule { get; set; }
    }

}
