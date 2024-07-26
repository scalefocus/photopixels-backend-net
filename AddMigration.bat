echo Adding Migration...
@echo off
cd /d "%~dp0/src/SF.PhotosApp.API"
@echo on
dotnet run -- db-patch ..\SF.PhotosApp.Infrastructure\Migrations\%1.sql
@echo off
cd /d "%~dp0/src/SF.PhotosApp.Infrastructure/Migrations"
move %1.drop.sql ./Rollback/%1.drop.sql