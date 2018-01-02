using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaPoco;

namespace SqlPlastic
{
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

    static class QuerryRunner
    {
        static string dbname = "NEWVERSIONDB";

        public static ColumnDescriptor[] ListColumns()
        {
            string query = @"
                            SELECT
                                tbl.name as TableName,
                                SCHEMA_NAME(tbl.schema_id) as SchemaName,
                                c.object_id as ObjectID,
                                c.column_id as ColumnID,
                                c.name as ColumnName,
                                t.Name as DataType,
                                c.max_length as MaxLength,
                                c.precision as Precision,
                                c.scale as Scale,
                                c.is_nullable as IsNullable,
                                c.is_identity as IsIdentity,
                                c.is_computed as IsComputed
                            FROM
                                sys.tables as tbl
                            INNER JOIN
                                sys.columns as c ON tbl.object_id = c.object_id
                            INNER JOIN 
                                sys.types as t ON c.user_type_id = t.user_type_id
                            ";

            using (var dc = new PetaPoco.Database(String.Format(@"Data Source =.\sqlexpress; Integrated Security = SSPI; DataBase = {0}", dbname), providerName: "SqlServer"))
            {
                ColumnDescriptor[] cols = dc.Query<ColumnDescriptor>(query).ToArray();
                return cols;
            }
        }


        public static PrimaryKeyDescriptor[] ListPrimaryKeys()
        {
            string query = @"
                            SELECT  i.name AS IndexName,
                                    ic.object_id As TableObjectID,
                                    ic.column_id as ColumnID,
                                    OBJECT_NAME(ic.OBJECT_ID) AS TableName,
                                    COL_NAME(ic.OBJECT_ID, ic.column_id) AS ColumnName
                            FROM    sys.indexes AS i
                                    INNER JOIN sys.index_columns AS ic ON i.OBJECT_ID = ic.OBJECT_ID AND i.index_id = ic.index_id
                            WHERE   i.is_primary_key = 1
                            ORDER BY OBJECT_NAME(ic.OBJECT_ID)
                            ";

            using (var dc = new PetaPoco.Database(String.Format(@"Data Source =.\sqlexpress; Integrated Security = SSPI; DataBase = {0}", dbname), providerName: "SqlServer"))
            {
                PrimaryKeyDescriptor[] pks = dc.Query<PrimaryKeyDescriptor>(query).ToArray();
                return pks;
            }
        }

        public static ForeignKeyDescriptor[] ListForeignKeys()
        {
            // note select *distinct* because the join result in duplicate entries
            string query = @"
                        SELECT distinct
                            f.name AS ForeignKeyName, 
                            fc.parent_object_id as ParentObjectID,
                            fc.parent_column_id as ParentColumnID,
                            OBJECT_NAME(f.parent_object_id) AS ParentTableName,
                            COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ParentColumnName,

                            fc.referenced_object_id as ReferencedObjectID,
                            fc.referenced_column_id as ReferencedColumnID,
                            OBJECT_NAME (f.referenced_object_id) AS ReferencedTableName,
                            COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumnName,
                            f.delete_referential_action_desc as OnDelete,
                            f.update_referential_action_desc as OnUpdate
                        FROM sys.foreign_keys AS f
                        INNER JOIN sys.foreign_key_columns AS fc
                        ON f.OBJECT_ID = fc.constraint_object_id
                            ";

            using (var dc = new PetaPoco.Database(String.Format(@"Data Source =.\sqlexpress; Integrated Security = SSPI; DataBase = {0}", dbname), providerName: "SqlServer"))
            {
                ForeignKeyDescriptor[] fks = dc.Query<ForeignKeyDescriptor>(query).ToArray();
                return fks;
            }
        }
    }
}
