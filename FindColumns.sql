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
Order by TableName, ColumnID