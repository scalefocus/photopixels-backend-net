
## How to add new migrations
1. Open a terminal.
2. Navigate to the repo **root** folder.
3. In the terminal, execute the following command:  
	`./AddMigration.bat <no. of migration>.<name of migration>` 
	ex.  `./AddMigration.bat 5.add_users`
	This will create two new .sql files:
	- The **new migration** in `SF.PhotosApp.Infrastructure/Migrations`,
	- The rollback for the new migration in `SF.PhotosApp.Infrastructure/Migrations/Rollback`