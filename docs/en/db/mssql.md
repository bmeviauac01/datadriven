# Using Microsoft SQL Server

Microsoft SQL Server is accessed using SQL Server Management Studio. We are using the so-called LocalDB version that hosts the server locally for development purposes, but you can also use the Express edition (any version).

Download links:

- LocalDB is installed with Visual Studio
- <https://www.microsoft.com/en-us/sql-server/sql-server-editions-express>
- <https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms>

!!! example "Video guide of the tools"
    How to use these tools: <https://web.microsoftstream.com/video/98a6697d-daec-4a5f-82b6-8e96f06302e8>

## Using SQL Server Management Studio

In university computer laboratories, you can start the software from the start menu. Connection details are configured when the application starts. When using LocalDB, the _Server name_ is `(localdb)\mssqllocaldb`; for Express Edition the name is `.\sqlexpress` (if installed with default settings). Either case use _Windows Authentication_.

When the connection is established, the databases are listed on the left in the _Object Explorer_ window under _Databases_. A database can be expanded, and the tables (along with other schema elements) are listed here.

SQL code can be executed in a new _Query_ window opened using ![New query button](../db/images/new-query-button.png) on the toolbar. The commands in the _Query_ window are executed on the currently selected database. This database is selected from a dropdown in the toolbar (see in the image below with yellow). We can open multiple _Query_ windows.

The SQL command is executed using the ![Execute button](../db/images/execute-button.png) button on the toolbar. Only the selection is executed if any text is selected; otherwise, the entire window content is sent to the server. The result and errors are printed under the script.

![SQL Server Management Studio](../db/images/object-explorer-db-query.png)

### Creating a new database

If we have no database, we must create one first. In _Object Explorer_ right-click _Databases_ to open a dialog. We need to specify a name and leave all other options as-is. After creating a new database, we shall not forget to select the toolbar's current database for any _Query_ window we have open!

![Create new database](../db/images/uj-adatbazis.png)

### Concurrent transactions

To simulate concurrent transactions, we need two _Query_ windows; open two by pressing the _New Query_ button twice. You can align these windows next to each other by right-clicking the _Query_ tab title and selecting _New Vertical Tab Group_.

![Query windows next to each other](../db/images/query-window-tab-group.png)

### Listing and editing table content

To view any database table's content, open the database in _Object Explorer_, locate the table under _Tables_, then right-click and chose _Select Top 1000 Rows_ or _Edit Top 200 Rows_.

![View table content](../db/images/select-top-1000.png)

### Intellisense reload

Intellisense often does not work in SQL Management Studio query windows. Press Control+Shift+R-t to trigger a reload of Intellisense cache. We also need to use this after creating a new object (e.g., a new stored procedure).

### Creating stored procedures and triggers

We can create new stored procedures or triggers by writing the T-SQL code to create them in a _Query_ window. Once an item with the same name exists, we cannot create it but have to modify the existing one using the proper command.

Existing stored procedures are listed in _Object Explorer_ under our database in the _Programability/Stored Procedures_ folder. (Newly created items do not appear in the folder, but we have to refresh the folder content by right-clicking and choosing _Refresh_.)

![Stored procedures](../db/images/tarolt-eljaras.png)

Triggers are found in _Object Explorer_ under the table on which they are defined in the _Triggers_ folder (system-level triggers are in the _Programability_ folder under the database itself).

![Trigger](../db/images/trigger.png)

The code of any existing stored procedure or trigger can be opened by locating them (see above), then right-clicking and choosing the _Modify_ command. This will open a new _Query_ window with an `alter` command and the current code of the program.
