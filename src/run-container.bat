
@ECHO OFF

SETLOCAL ENABLEEXTENSIONS

SET CONTAINER_STORE=%~dp0
PUSHD "%CONTAINER_STORE%"

IF "%APP_PROJECT%"=="" SET APP_PROJECT=green-onion-dev

SET EXTRA_ARGS=
IF /I "%1" EQU "DETACH" (
    SET EXTRA_ARGS=--detach 
)

CALL build.bat
@IF NOT %ERRORLEVEL%==0 GOTO :error

docker compose --project-name %APP_PROJECT% --file docker-compose.webapi.yml build --no-cache
@REM @IF NOT %ERRORLEVEL%==0 GOTO :error
docker compose --project-name %APP_PROJECT% --file docker-compose.webapi.yml up %EXTRA_ARGS%
@REM @IF NOT %ERRORLEVEL%==0 GOTO :error

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