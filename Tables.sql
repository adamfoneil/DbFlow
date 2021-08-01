CREATE SCHEMA [changelog]
GO

-- logs DDL events as they happen
CREATE TABLE [changelog].[Event] (
	[Id] int identity(1,1) PRIMARY KEY,
	[Timestamp] datetime NOT NULL DEFAULT (getutcdate()),
	[EventType] nvarchar(100) NOT NULL,
	[UserName] nvarchar(50) NOT NULL,
	[Schema] nvarchar(50) NOT NULL,
	[ObjectName] nvarchar(100) NOT NULL,	
	[Script] nvarchar(max) NOT NULL,
	[Version] int NOT NULL
)
GO

-- maintains the next version number of each object in order to compare script with the prior version
CREATE TABLE [changelog].[Object] (
	[Id] int identity(1,1) PRIMARY KEY,
	[Schema] nvarchar(50) NOT NULL,
	[Name] nvarchar(100) NOT NULL,
	[Type] nvarchar(50) NOT NULL,
	[Version] int NOT NULL,
	CONSTRAINT [U_changelog_Object] UNIQUE ([Schema], [Name])
)
GO

-- keeps a human-readable version of table script for text compare purposes
CREATE TABLE [changelog].[Table] (
	[Id] int identity(1,1) PRIMARY KEY,
	[EventId] int NOT NULL,
	[Xml] xml NOT NULL,
	[Text] nvarchar(max) NULL,
	CONSTRAINT [FK_changelogTable_EventId] FOREIGN KEY ([EventId]) REFERENCES [changelog].[Event] ([Id]),
	CONSTRAINT [U_changelogTable_EventId] UNIQUE ([EventId])
)
