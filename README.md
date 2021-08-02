This is an approach to tracking changes to SQL Server objects automatically using several components you must create in a database:

- some [tables](https://github.com/adamfoneil/ChangeLogUtil/blob/master/Tables.sql),
- a [table function](https://github.com/adamfoneil/ChangeLogUtil/blob/master/TableComponents.sql), and a 
- a [DDL trigger](https://github.com/adamfoneil/ChangeLogUtil/blob/master/Trigger.sql) that must be enabled

Once implemented, when you execute a DDL create or alter statement on any object, for example:

![image](https://user-images.githubusercontent.com/4549398/127789248-29c52b3c-64db-4abd-b737-0b1eb34737f3.png)

You can then view the history of changes to the object with a little [Razor Pages app](https://github.com/adamfoneil/ChangeLogUtil/tree/master/ChangeLog.Web) in this repo:

![image](https://user-images.githubusercontent.com/4549398/127789297-5bda234a-72a7-423d-a685-54e580ede26c.png)

This uses [DiffPlex](https://github.com/mmanela/diffplex) to achieve the diff view. Amazing project!

## Why?
Source control of database objects is a notoriously fraught subject because database objects don't play nicely as ordinary source code. There are lots of solutions out there already for this, but I wanted to see what could be achieved using a reactive, **tracking** mindset instead of a **control** mindset.

## Points of Interest
- My DDL trigger was adapted from [this example](https://docs.microsoft.com/en-us/sql/t-sql/functions/eventdata-transact-sql?view=sql-server-ver15#b-creating-a-log-table-with-event-data-in-a-ddl-trigger) from Microsoft's documentation.

- "Standalone" objects like views and procs are relatively easy to track because they are safely deleted and recreated with every change. (For tracking purposes, altering objects is the same as dropping and creating them.) Tables are different because we're usually adding and modifying columns, indexes, and constraints, rather than dropping and rebuilding them outright. Tracking incremental changes to tables in this way required special handling. I needed a [table function](https://github.com/adamfoneil/ChangeLogUtil/blob/master/TableComponents.sql) to collect the relevant metadata about a table. I store this as [XML](https://github.com/adamfoneil/ChangeLogUtil/blob/master/Trigger.sql#L48-L49). The reason for this is that I want to present the table metadata in a plain text format, and that plain text is too complex to convert from XML within T-SQL code. I have a [dedicated XML renderer](https://github.com/adamfoneil/ChangeLogUtil/blob/master/ChangeLog.Services/TableDefRenderer.cs). You can see [sample output](https://github.com/adamfoneil/ChangeLogUtil/blob/master/ChangeLog.Test/Resources/Appointment.txt) in the test project. I could've generated a CREATE TABLE statement for this, but I didn't because a CREATE assumes a DROP beforehand, which is normally not appropriate for tables.

- The [Index](https://github.com/adamfoneil/ChangeLogUtil/blob/master/ChangeLog.Web/Pages/Index.cshtml) page in the web app uses [ChangeLogRepository](https://github.com/adamfoneil/ChangeLogUtil/blob/master/ChangeLog.Services/ChangeLogRepository.cs) to query the change log data.

- The connections available to the web app are regular connection strings in [appsettings.json](https://github.com/adamfoneil/ChangeLogUtil/blob/master/ChangeLog.Web/appsettings.json#L2-L5). I imagine you'd host this in a private domain so I use Windows authentication for the app.
