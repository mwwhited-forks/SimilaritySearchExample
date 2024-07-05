
SET TARGET_SOLUTION=Eliassen.Libs.sln
SET TARGET_SOLUTION_NAME=Eliassen.Libs
SET TARGET_WEB_PROJECT=.\Examples\Eliassen.WebApi\Eliassen.WebApi.csproj
SET TARGET_WEB_PROJECT_NAME=Eliassen.WebApi

SET SCRIPT_ROOT=%~dp0

IF "%1"=="--clean" (
	RMDIR %SCRIPT_ROOT%publish\packages /Q/S
    DEL publish\WebApp.zip /Y
	SHIFT
)

ECHO "restore current .net tools"
dotnet tool restore

echo "Git fetch"
git fetch --prune
FOR /F "tokens=* USEBACKQ" %%g IN (`dotnet gitversion /output json /showvariable FullSemVer`) DO (SET BUILD_VERSION=%%g)
if "%BUILD_VERSION%"=="" GOTO error
ECHO Building Version=  "%BUILD_VERSION%"

dotnet pack ^
--configuration Release ^
--output %SCRIPT_ROOT%publish\packages ^
%TARGET_SOLUTION%

dotnet build %TARGET_SOLUTION% ^
--configuration Release ^
--nologo ^
/p:DeployOnBuild=true ^
/p:WebPublishMethod=Package ^
/p:PackageAsSingleFile=true ^
/p:SkipInvalidConfigurations=true ^
/p:DesktopBuildPackageLocation="%SCRIPT_ROOT%publish\website\%BUILD_VERSION%\%TARGET_WEB_PROJECT_NAME%.zip" ^
/p:DeployIisAppPath="Default Web Site"
