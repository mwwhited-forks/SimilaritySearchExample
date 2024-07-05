
@ECHO OFF

SET DATABASE_TYPES=npgsql
REM mssql

CALL :execute %DATABASE_TYPES%
GOTO :EOF

:execute
SET CURRENT_DATABASE_TYPE=%1
SHIFT
IF "%CURRENT_DATABASE_TYPE%"=="" EXIT /B

SET DBTYPE=%CURRENT_DATABASE_TYPE%
dotnet-ef dbcontext script --output "database-%CURRENT_DATABASE_TYPE%.sql"

GOTO execute