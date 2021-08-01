CREATE FUNCTION [changelog].[TableComponents](
	@schema nvarchar(50),
	@name nvarchar(100)
) RETURNS @results TABLE (
	[Type] nvarchar(50) NOT NULL, -- column, index, FK, index or FK col, check
	[Parent] nvarchar(50) NULL, -- container object within the table, or null for the table itself	
	[Name] nvarchar(100) NOT NULL, -- name of column, index, or constraint
	[Position] int NULL, -- ordinal position of element
	[Definition] xml NOT NULL
) AS
BEGIN
	DECLARE @objectId int
	SET @objectId = OBJECT_ID(@schema + '.' + @name)

	-- columns
	INSERT INTO @results (
		[Type], [Name], [Position], [Definition]
	) SELECT
		'Column', [col].[name], [col].[column_id],		
		'<type>' + TYPE_NAME([col].[system_type_id]) + '</type>' +
		'<length>' + CONVERT(varchar, [col].[max_length]) + '</length>' +
		'<precision>' + CONVERT(varchar, [col].[precision]) + '</precision>' +
		'<scale>' + CONVERT(varchar, [col].[scale]) + '</scale>' +
		'<collation>' + ISNULL([col].[collation_name], '') + '</collation>' +
		'<nullable>' + CASE [col].[is_nullable] WHEN 1 THEN 'true' ELSE 'false' END + '</nullable>' +
		'<identity>' + CASE [col].[is_identity] WHEN 1 THEN 'true' ELSE 'false' END + '</identity>' +
		'<computed>' + CASE [col].[is_computed] WHEN 1 THEN 'true' ELSE 'false' END + '</computed>' +
		'<expression>' + ISNULL([ccol].[definition], '') + '</expression>' +
		'<default>' + ISNULL([def].[definition], '') + '</default>'		
	FROM [sys].[columns] [col]
	LEFT JOIN [sys].[computed_columns] [ccol] ON
		[col].[object_id]=[ccol].[object_id] AND
		[col].[name]=[ccol].[name]
	LEFT JOIN [sys].[default_constraints] [def] ON
		[col].[object_id]=[def].[parent_object_id] AND
		[col].[column_id]=[def].[parent_column_id]
	WHERE 
		[col].[object_id]=@objectId

	-- indexes
	INSERT INTO @results (
		[Type], [Name], [Position], [Definition]
	) SELECT
		'Index', [ndx].[name], [ndx].[index_id],		
		'<type>' + CASE [ndx].[type] WHEN 1 THEN 'clustered' ELSE 'non-clustered' END + '</type>' +
		'<unique>' + CASE [ndx].[is_unique] WHEN 1 THEN 'true' ELSE 'false' END + '</unique>' +
		'<ignoreDups>' + CASE [ndx].[ignore_dup_key] WHEN 1 THEN 'true' ELSE 'false' END + '</ignoreDups>' +
		'<primary>' + CASE [ndx].[is_primary_key] WHEN 1 THEN 'true' ELSE 'false' END + '</primary>' +
		'<uniqueConstraint>' + CASE [ndx].[is_unique_constraint] WHEN 1 THEN 'true' ELSE 'false' END + '</uniqueConstraint>' +
		'<disabled>' + CASE [ndx].[is_disabled] WHEN 1 THEN 'true' ELSE 'false' END + '</disabled>' +
		'<padded>' + CASE [ndx].[is_padded] WHEN 1 THEN 'true' ELSE 'false' END + '</padded>' +
		'<fillFactor>' + CONVERT(varchar, [ndx].[fill_factor]) + '</fillFactor>' +
		'<filter>' + ISNULL([ndx].[filter_definition], '') + '</filter>'		
	FROM
		[sys].[indexes] [ndx]
	WHERE
		[ndx].[object_id]=@objectId

	-- index columns
	INSERT INTO @results (
		[Type], [Name], [Parent], [Position], [Definition]
	) SELECT
		'IndexColumn', [col].[name], [x].[name], [xcol].[index_column_id], 		
		'<sort>' + CASE [xcol].[is_descending_key] WHEN 1 THEN 'DESC' ELSE 'ASC' END + '</sort>' +
		'<included>' + CASE [xcol].[is_included_column] WHEN 1 THEN 'true' ELSE 'false' END + '</included>'		
	FROM 
		[sys].[index_columns] [xcol]
		INNER JOIN [sys].[indexes] [x] ON 
			[xcol].[object_id]=[x].[object_id] AND 
			[xcol].[index_id]=[x].[index_id]
		INNER JOIN [sys].[columns] [col] ON 
			[xcol].[object_id]=[col].[object_id] AND 
			[xcol].[column_id]=[col].[column_id]
		INNER JOIN [sys].[tables] [t] ON [x].[object_id]=[t].[object_id]
	WHERE
		[x].[object_id]=@objectId

	-- foreign keys
	INSERT INTO @results (
		[Name], [Type], [Definition]
	) SELECT				
		[fk].[name], 'ForeignKey',		
		'<referencedSchema>' + SCHEMA_NAME([ref_t].[schema_id]) + '</referencedSchema>' +
		'<referencedTable>' + [ref_t].[name] + '</referencedTable>' +
		'<cascadeDelete>' + CASE [fk].[delete_referential_action] WHEN 1 THEN 'true' ELSE 'false' END + '</cascadeDelete>' +
		'<cascadeUpdate>' + CASE [fk].[update_referential_action] WHEN 1 THEN 'true' ELSE 'false' END + '</cascadeUpdate>'
	FROM
		[sys].[foreign_keys] [fk]
		INNER JOIN [sys].[tables] [ref_t] ON [fk].[referenced_object_id]=[ref_t].[object_id]
		INNER JOIN [sys].[tables] [child_t] ON [fk].[parent_object_id]=[child_t].[object_id]
	WHERE
		[fk].[parent_object_id]=@objectId

	-- fk columns
	INSERT INTO @results (
		[Type], [Name], [Parent], [Position], [Definition]
	) SELECT					
		'ForeignKeyColumn', [child_col].[name], [fk].[name], [fkcol].[constraint_column_id],		
		'<referencedColumn>' + [ref_col].[name] + '</referencedColumn>'		
	FROM
		[sys].[foreign_key_columns] [fkcol]
		INNER JOIN [sys].[foreign_keys] [fk] ON [fkcol].[constraint_object_id]=[fk].[object_id]
		INNER JOIN [sys].[tables] [child_t] ON [fkcol].[parent_object_id]=[child_t].[object_id]
		INNER JOIN [sys].[columns] [child_col] ON
			[child_t].[object_id]=[child_col].[object_id] AND
			[fkcol].[parent_column_id]=[child_col].[column_id]
		INNER JOIN [sys].[tables] [ref_t] ON [fkcol].[referenced_object_id]=[ref_t].[object_id]
		INNER JOIN [sys].[columns] [ref_col] ON
			[ref_t].[object_id]=[ref_col].[object_id] AND
			[fkcol].[referenced_column_id]=[ref_col].[column_id]
	WHERE
		[fk].[parent_object_id]=@objectId

	-- check constraints
	INSERT INTO @results (
		[Type], [Name], [Definition]
	) SELECT
		'CheckConstraint', [ck].[name] AS [Name],		
		'<expression>' + [ck].[definition] + '</expression>'		
	FROM
		[sys].[check_constraints] [ck]
	WHERE
		[ck].[parent_object_id]=@objectId AND
		[ck].[type]='C'

	RETURN
END
