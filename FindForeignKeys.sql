SELECT f.name AS ForeignKey, 
    fc.parent_object_id as ParentObjectID,
	fc.parent_column_id as ParentColumnID,
    OBJECT_NAME(f.parent_object_id) AS ParentTableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ParentColumnName,
	fc.referenced_object_id as ReferencedObjectID,
	fc.referenced_column_id as ReferencedColumnID,
    OBJECT_NAME (f.referenced_object_id) AS ReferenceTableName,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferenceColumnName,
	f.delete_referential_action_desc as OnDelete,
	f.update_referential_action_desc as OnUpdate
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc
ON f.OBJECT_ID = fc.constraint_object_id