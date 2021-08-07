This is a SQL Server source control solution that you host as a Razor Pages web app. There are two parts:

- DDL trigger components that log ongoing changes in your databases:
  - the [trigger](https://github.com/adamfoneil/ChangeLogUtil/blob/master/Trigger.sql) itself
  - a set of [tables](https://github.com/adamfoneil/ChangeLogUtil/blob/master/Tables.sql)
  - a [table function](https://github.com/adamfoneil/ChangeLogUtil/blob/master/TableComponents.sql)

- A [web app](https://github.com/adamfoneil/DbFlow/tree/master/ChangeLog.Web) that offers
  - diff view of changes to database objects, powered by [DiffPlex](https://github.com/mmanela/diffplex)
  - **coming soon:** a pull request workflow for requesting, approving, and deploying database changes from one environment to another

Once implemented, executing a DDL create or alter statement on any object, for example:

![image](https://user-images.githubusercontent.com/4549398/127789248-29c52b3c-64db-4abd-b737-0b1eb34737f3.png)

You can then view the history of changes to the object with a little [Razor Pages app](https://github.com/adamfoneil/ChangeLogUtil/tree/master/ChangeLog.Web) in this repo:

![image](https://user-images.githubusercontent.com/4549398/127789297-5bda234a-72a7-423d-a685-54e580ede26c.png)


## Why?
Source control of database objects is a fraught subject because database objects don't play nicely as ordinary source code. There are lots of solutions out there already for this, but I wanted to see what could be achieved using a reactive, **tracking** mindset instead of a **control** mindset. I believe that reliable, visible change tracking removes the need to maintain a separate code repository of database objects. The database itself is the "source of truth." But historically what's been missing is a continuous diff view of changes over the lifetime of objects. This is the feature gap this project fills.

## Limitations
With Windows logins enabled, it's easy to tell who made a change to an object, since that is [tracked](https://github.com/adamfoneil/ChangeLogUtil/blob/master/Tables.sql#L9). However, in an environment where a shared account is used, as may be likely in Azure SQL Database, for example, you'd wouldn't have good visibility on who's making changes.

## Points of Interest
- My DDL trigger was adapted from [this example](https://docs.microsoft.com/en-us/sql/t-sql/functions/eventdata-transact-sql?view=sql-server-ver15#b-creating-a-log-table-with-event-data-in-a-ddl-trigger) from Microsoft's documentation.

- "Standalone" objects like views and procs are relatively easy to track because they are safely deleted and recreated with every change. (For tracking purposes, altering objects is the same as dropping and creating them.) Tables are different because we're usually adding and modifying columns, indexes, and constraints, rather than dropping and rebuilding them outright. Tracking incremental changes to tables in this way required special handling. I needed a [table function](https://github.com/adamfoneil/ChangeLogUtil/blob/master/TableComponents.sql) to collect the relevant metadata about a table. I store this as [XML](https://github.com/adamfoneil/ChangeLogUtil/blob/master/Trigger.sql#L48-L49). The reason for this is that I want to present the table metadata in a plain text format, and that plain text is too complex to convert from XML within T-SQL code. I have a [dedicated XML renderer](https://github.com/adamfoneil/ChangeLogUtil/blob/master/ChangeLog.Services/TableDefRenderer.cs). You can see [sample output](https://github.com/adamfoneil/ChangeLogUtil/blob/master/ChangeLog.Test/Resources/Appointment.txt) in the test project. I could've generated a CREATE TABLE statement for this, but I didn't because a CREATE assumes a DROP beforehand, which is normally not appropriate for tables.

- The [Index](https://github.com/adamfoneil/ChangeLogUtil/blob/master/ChangeLog.Web/Pages/Index.cshtml) page in the web app uses [ChangeLogRepository](https://github.com/adamfoneil/ChangeLogUtil/blob/master/ChangeLog.Services/ChangeLogRepository.cs) to query the change log data.

- The connections available to the web app are regular connection strings in [appsettings.json](https://github.com/adamfoneil/ChangeLogUtil/blob/master/ChangeLog.Web/appsettings.json#L2-L5). I imagine you'd host this in a private domain so I use Windows authentication for this app.

# What's Next?
In a typical workflow, you need to migrate objects across environments -- typically at minimum from QA to production. This involves comparing objects between databases and migrating select changes, usually changes related to a pull request. So, I envision the next phase of this adding the ability to compare and migrate objects across connections.

There are several fine database object comparison apps out there. I have a library of my own, [ModelSync](https://github.com/adamfoneil/ModelSync), but it's not fully ready for the use case I imagine here. (I have an [open issue](https://github.com/adamfoneil/ModelSync/issues/16) on the feature gaps.)


