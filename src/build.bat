
@ECHO OFF

SETLOCAL ENABLEEXTENSIONS

SET SCRIPT_ROOT=%~dp0
PUSHD "%SCRIPT_ROOT%"

REM Start -- Configuration this section Only
CALL config.bat
REM End -- Configuration this section Only 

@REM dotnet format
@REM @IF NOT %ERRORLEVEL%==0 GOTO :error

ECHO "Build Project"
dotnet build --configuration Release %PROJECT_SOLUTION%
@IF NOT %ERRORLEVEL%==0 GOTO :error

ECHO "Publish Project"
IF EXIST .\Publish\WebApi rmdir /S/Q .\Publish\WebApi
dotnet publish --configuration Release --output .\Publish\WebApi %PROJECT_ASSEMBLY%
@IF NOT %ERRORLEVEL%==0 GOTO :error

ECHO "Publish Dataloader"
IF EXIST .\Publish\Dataloader rmdir /S/Q .\Publish\Dataloader
dotnet publish --configuration Release --output .\Publish\Dataloader %DATALOADER_ASSEMBLY%
@IF NOT %ERRORLEVEL%==0 GOTO :error

ECHO "Publish Database"
IF EXIST .\Publish\Database rmdir /S/Q .\Publish\Database
dotnet build --configuration Release --output .\Publish\Database %DATA_PROJECT%
@IF NOT %ERRORLEVEL%==0 GOTO :error

ECHO "Build Complete!"
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