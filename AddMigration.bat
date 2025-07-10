echo Adding Migration...
@echo off
cd /d "%~dp0/src/SF.PhotoPixels.API"
@echo on
dotnet run -- db-patch ..\SF.PhotoPixels.Infrastructure\Migrations\%1.sql
@echo off
cd /d "%~dp0/src/SF.PhotoPixels.Infrastructure/Migrations"
move %1.drop.sql ./Rollback/%1.drop.sql