This is an approach to tracking changes to SQL Server objects automatically using several components you must create in a database:

- some [tables](https://github.com/adamfoneil/ChangeLogUtil/blob/master/Tables.sql),
- a [table function](https://github.com/adamfoneil/ChangeLogUtil/blob/master/TableComponents.sql), and a 
- a [DDL trigger](https://github.com/adamfoneil/ChangeLogUtil/blob/master/Trigger.sql) that must be enabled

Once implemented, when you execute a DDL create or alter statement on any object, for example:

![image](https://user-images.githubusercontent.com/4549398/127789248-29c52b3c-64db-4abd-b737-0b1eb34737f3.png)

You can view the history of changes to the object with a little [Razor Pages app](https://github.com/adamfoneil/ChangeLogUtil/tree/master/ChangeLog.Web) in this repo:

![image](https://user-images.githubusercontent.com/4549398/127789297-5bda234a-72a7-423d-a685-54e580ede26c.png)

This uses [DiffPlex](https://github.com/mmanela/diffplex) to achieve the diff view. Amazing project!
