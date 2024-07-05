
SETLOCAL ENABLEEXTENSIONS

SET SCRIPT_ROOT=%~dp0
PUSHD "%SCRIPT_ROOT%"

REM Start -- Configuration this section Only
CALL config.bat
REM End -- Configuration this section Only 

SET SCRIPT_COLLECTION=.\%DATA_PROJECT%\Generated\%DATABASE_NAME%Context.sql

IF "%1"=="JUST_PUBLISH" GOTO JUST_PUBLISH
IF "%1"=="NO_BUILD" GOTO NO_BUILD

dotnet tool restore
@IF NOT %ERRORLEVEL%==0 GOTO :error
dotnet build
@IF NOT %ERRORLEVEL%==0 GOTO :error

:NO_BUILD

dotnet ef dbcontext script --output "%SCRIPT_COLLECTION%" --project "%DATA_PROJECT_ASSEMBLY%" --no-build
@IF NOT %ERRORLEVEL%==0 GOTO :error

GOTO done

:error
SET REAL_ERROR=%ERRORLEVEL%
@ECHO OFF
POPD
ENDLOCAL
EXIT /B %REAL_ERROR%

:done
POPD
ENDLOCAL
EXIT /B 0