
@ECHO OFF
SETLOCAL EnableDelayedExpansion

SET SCRIPT_ROOT=%~dp0
PUSHD "%SCRIPT_ROOT%"

SET SolutionDir=%~dp0
ECHO SolutionDir %SolutionDir%
SET PublishPath=%SolutionDir%publish\libs\
ECHO PublishPath %PublishPath%

REM Start -- Configuration this section Only
CALL config.bat
REM End -- Configuration this section Only 

SET TEMPLATE_COMMAND=templateengine

IF /I "%1"=="docs" (
    CALL :GENERATE_CODE_DOCS
    EXIT /B
)
IF /I "%1"=="libs" (
    CALL :GENERATE_LIBRARY_DOCS
    EXIT /B
)

ECHO "restore current .net tools"
dotnet tool restore

echo "Git fetch"
git fetch --prune
FOR /F "tokens=* USEBACKQ" %%g IN (`dotnet gitversion /output json /showvariable FullSemVer`) DO (SET BUILD_VERSION=%%g)
if "%BUILD_VERSION%"=="" GOTO error
ECHO Building Version=  "%BUILD_VERSION%"

@REM CALL :FORMAT_SOURCE_CODE
@REM 
@REM CALL build.bat
@REM SET TEST_ERR=%ERRORLEVEL%
@REM IF NOT "%TEST_ERR%"=="0" (
@REM 	ECHO "Build Failed! %TEST_ERR%"
@REM 	GOTO :skiptoend
@REM )

CALL :BUILD_SWAGGER_DOCS
@REM CALL :GENERATE_ENDPOINTS_REPORT
@REM CALL :GENERATE_CODE_DOCS
@REM CALL :GENERATE_LIBRARY_DOCS

@REM CALL test.bat --no-start
@REM SET TEST_ERR=%ERRORLEVEL%
@REM IF NOT "%TEST_ERR%"=="0" (
@REM 	ECHO "Tests Failed! %TEST_ERR%"
@REM 	GOTO :skiptoend
@REM )

@REM CALL :GENERATE_TEST_REPORTS
@REM CALL :GENERATE_SOFTWARE_BOM
@REM CALL :GENERATE_SOFTWARE_BOM_REPORT

ECHO TEST_ERR=%TEST_ERR%
:skiptoend
IF "%TEST_ERR%"=="0" (
	ECHO "No Errors :)"
)
POPD
EXIT /B %TEST_ERR%
:EOF
POPD
ENDLOCAL
EXIT /B

REM ===============================

EXIT /B

:FORMAT_SOURCE_CODE
dotnet format ^
--verbosity detailed ^
--report %PublishPath%..\reports\format.json
EXIT /B

:BUILD_SWAGGER_DOCS
ECHO "Generate - swagger docs"
dotnet build /T:BuildSwagger %PROJECT_ASSEMBLY%
EXIT /B

:GENERATE_ENDPOINTS_REPORT

ECHO "Generate Service-Endpoints"
dotnet %TEMPLATE_COMMAND% ^
--configuration Release ^
--input ..\docs\swagger.json ^
--output ..\docs\Service-Endpoints.md ^
--Template Service-Endpoints ^
--file-template-path ..\docs\templates
EXIT /B

:GENERATE_CODE_DOCS
ECHO "Generate - Library code docs"
RMDIR ..\docs\code /S/Q
dotnet build /T:GetDocumentation
EXIT /B

:GENERATE_LIBRARY_DOCS
ECHO "Generate - Library Docs"
RMDIR ..\docs\Libraries /S/Q
dotnet %TEMPLATE_COMMAND% ^
--configuration Release ^
--input %PublishPath%*.xml ^
--output ..\docs\Libraries\[file].md ^
--Template Documentation.md ^
--file-template-path ..\docs\templates
SET TEST_ERR=%ERRORLEVEL%
IF NOT "%TEST_ERR%"=="0" (
	ECHO "SBOM Failed! %TEST_ERR%"
    EXIT /B %TEST_ERR%
)
DEL ..\docs\Libraries\Microsoft*.* /Q
EXIT /B

:GENERATE_TEST_REPORTS
ECHO "Generate - Test Docs"
RMDIR ..\docs\Tests /S/Q
MKDIR ..\docs\Tests
ECHO "Copy Code Coverage Results"
COPY .\TestResults\Cobertura.coverage ..\docs\Tests\Cobertura.coverage /Y
ECHO "Copy Code Test Results"
COPY .\TestResults\Coverage\Reports\LatestTestResults.trx ..\docs\Tests\LatestTestResults.trx /Y
ECHO "Copy Code Coverage Report"
COPY .\TestResults\Coverage\Reports\Summary.md ..\docs\Tests\Summary.md /Y
ECHO "Generate - Test Result"
dotnet %TEMPLATE_COMMAND% ^
--configuration Release ^
--input .\TestResults\Coverage\Reports\*.trx ^
--output ..\docs\Tests\[file].md ^
--Template TestResultsToMarkdown.md ^
--file-template-path ..\docs\templates ^
--input-type XML
SET TEST_ERR=%ERRORLEVEL%
IF NOT "%TEST_ERR%"=="0" (
	ECHO "SBOM Failed! %TEST_ERR%"
    EXIT /B %TEST_ERR%
)
EXIT /B

:GENERATE_SOFTWARE_BOM
ECHO "Generate - Software Bill of Materials (bom.xml)"
RMDIR ..\docs\sbom /S/Q
REM https://github.com/CycloneDX/cyclonedx-dotnet
dotnet CycloneDX ^
--output ..\docs\sbom ^
--set-version %BUILD_VERSION% ^
--set-name %PROJECT_SOLUTION_NAME% ^
--exclude-test-projects ^
--disable-package-restore ^
--exclude-dev ^
%PROJECT_SOLUTION%
SET TEST_ERR=%ERRORLEVEL%
IF NOT "%TEST_ERR%"=="0" (
	ECHO "SBOM Failed! %TEST_ERR%"
    EXIT /B %TEST_ERR%
)
EXIT /B

:GENERATE_SOFTWARE_BOM_REPORT
ECHO "Generate - Software Bill of Materials (report)"
dotnet %TEMPLATE_COMMAND% ^
--configuration Release ^
--input ..\docs\sbom\bom.xml ^
--output ..\docs\sbom\BillOfMaterials.md ^
--Template SoftwareBillOfMaterials.md ^
--file-template-path ..\docs\templates ^
--input-type XML
SET TEST_ERR=%ERRORLEVEL%
IF NOT "%TEST_ERR%"=="0" (
	ECHO "SBOM Failed! %TEST_ERR%"
    EXIT /B %TEST_ERR%
)
EXIT /B
