using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaPoco;

namespace SqlPlastic
{
    public static class QuerryRunner
    {
        const string columnQuery = @"
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

        const string primaryKeyQuery = @"
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

        const string foreignKeyQuery = @"
                        SELECT 
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


        public static DbMetaData QueryDbMetaData(string dbName, string constring)
        {
            ColumnDescriptor[] cols;
            PrimaryKeyDescriptor[] pks;
            ForeignKeyDescriptor[] fks;

            using (var dc = new PetaPoco.Database(constring, providerName: "SqlServer"))
            {
                cols = dc.Query<ColumnDescriptor>(columnQuery).ToArray();

                pks = dc.Query<PrimaryKeyDescriptor>(primaryKeyQuery).ToArray();

                fks = dc.Query<ForeignKeyDescriptor>(foreignKeyQuery).ToArray();
            }

            return new DbMetaData
            {
                DBName = dbName,
                Columns = cols,
                PrimaryKeys = pks,
                ForeignKeys = fks
            };
        }
    }
}
