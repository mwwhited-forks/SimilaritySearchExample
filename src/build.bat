
@ECHO OFF
SETLOCAL EnableDelayedExpansion

IF "%SolutionDir%"=="" (
SET SolutionDir=%~dp0
)
IF "%PublishPath%"=="" (
SET PublishPath=%SolutionDir%publish\libs\
)

ECHO "Build Web Project"
RMDIR "%PublishPath%" /S/Q
MKDIR "%PublishPath%"
dotnet build ^
--configuration Release ^
--output %PublishPath% ^
 /bl:logfile=.\docs\build\solution.binlog
SET TEST_ERR=%ERRORLEVEL%

IF NOT "%TEST_ERR%"=="0" (
	ECHO "Build Failed! %TEST_ERR%"
)
EXIT /B %TEST_ERR%
:EOF
ENDLOCAL
