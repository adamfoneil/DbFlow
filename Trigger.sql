CREATE TRIGGER [TR_OnDDL_Event] ON DATABASE 
FOR DDL_DATABASE_LEVEL_EVENTS   
AS  
DECLARE @data XML = EVENTDATA()
DECLARE @schema nvarchar(50), @objectName nvarchar(100), @objectType nvarchar(50), @version int
SET @schema = @data.value('(/EVENT_INSTANCE/SchemaName)[1]', 'nvarchar(50)')
SET @objectName = @data.value('(/EVENT_INSTANCE/ObjectName)[1]', 'nvarchar(100)')
SET @objectType = @data.value('(/EVENT_INSTANCE/ObjectType)[1]', 'nvarchar(50)')

-- get the current version num of object
SELECT @version = [Version]
FROM [changelog].[Object] [obj]
WHERE [Schema]=@schema AND [Name]=@objectName

IF @version IS NULL
BEGIN
	INSERT INTO [changelog].[Object] ([Schema], [Name], [Type], [Version])
	VALUES (@schema, @objectName, @objectType, 0)
	SET @version = 0
END

-- increment version for next change
UPDATE [obj] SET [Version]=[Version]+1
FROM [changelog].[Object] [obj]
WHERE [Schema]=@schema AND [Name]=@objectName

DECLARE @eventType nvarchar(100), @userName nvarchar(50), @script nvarchar(max)
SET @eventType = @data.value('(/EVENT_INSTANCE/EventType)[1]', 'nvarchar(100)')
SET @userName = @data.value('(/EVENT_INSTANCE/UserName)[1]', 'nvarchar(50)')
SET @script = @data.value('(/EVENT_INSTANCE/TSQLCommand)[1]', 'nvarchar(max)')  

INSERT INTO [changelog].[Event] (
	[EventType], [UserName], [Schema], [ObjectName], [Script], [Version]
) VALUES (
	@eventType, @userName, @schema, @objectName, @script, @version
)

DECLARE @eventId int
SET @eventId = SCOPE_IDENTITY()

IF @objectType = 'TABLE'
BEGIN
	DECLARE @tableDef xml
	SET @tableDef = (SELECT * FROM [changelog].[TableComponents](@schema, @objectName) FOR XML AUTO)

	INSERT INTO [changelog].[Table] ([EventId], [Xml]) 
	VALUES (@eventId, @tableDef)
END
GO