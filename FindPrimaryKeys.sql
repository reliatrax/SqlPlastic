SELECT  i.name AS IndexName,
		ic.object_id As TableObjectID,
		ic.column_id as ColumnID,
        OBJECT_NAME(ic.OBJECT_ID) AS TableName,
        COL_NAME(ic.OBJECT_ID, ic.column_id) AS ColumnName
FROM    sys.indexes AS i
        INNER JOIN sys.index_columns AS ic ON i.OBJECT_ID = ic.OBJECT_ID AND i.index_id = ic.index_id
WHERE   i.is_primary_key = 1
ORDER BY OBJECT_NAME(ic.OBJECT_ID)