This is an approach to tracking changes to SQL Server objects automatically using several components:

- some [tables](https://github.com/adamfoneil/ChangeLogUtil/blob/master/Tables.sql) you must create
- a [table function](https://github.com/adamfoneil/ChangeLogUtil/blob/master/TableComponents.sql), and a 
- a [DDL trigger](https://github.com/adamfoneil/ChangeLogUtil/blob/master/Trigger.sql) that must be enabled

I've made a little Razor Pages app for viewing diffs
