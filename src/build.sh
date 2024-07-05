#!/bin/bash

set -x
set -e

SCRIPT_ROOT=$(dirname "$0")
pushd "$SCRIPT_ROOT"

# Start -- Configuration this section Only

PROJECT_ASSEMBLY="GreenOnion.API"
PROJECT_SOLUTION="GreenOnion.API.sln"
DATALOADER_ASSEMBLY="Tools/GreenOnion.Dataloader"
DATA_PROJECT="GreenOnion.DB"

# End -- Configuration this section Only

# dotnet format (uncomment the following lines if needed)
# dotnet format
# if [ $? -ne 0 ]; then
#     echo "Error during dotnet format"
#     popd
#     exit $?
# fi

dotnet build --configuration Release "$PROJECT_SOLUTION"
if [ $? -ne 0 ]; then
    echo "Error during dotnet build"
    popd
    exit $?
fi

rm -rf ./Publish/WebApi
dotnet publish --configuration Release --output ./Publish/WebApi "$PROJECT_ASSEMBLY"
if [ $? -ne 0 ]; then
    echo "Error during dotnet publish for WebApi"
    popd
    exit $?
fi

rm -rf ./Publish/Dataloader
dotnet publish --configuration Release --output ./Publish/Dataloader "$DATALOADER_ASSEMBLY"
if [ $? -ne 0 ]; then
    echo "Error during dotnet publish for Dataloader"
    popd
    exit $?
fi

rm -rf ./Publish/Database
dotnet publish --configuration Release --output ./Publish/Database "$DATA_PROJECT"
if [ $? -ne 0 ]; then
    echo "Error during dotnet publish for Database"
    popd
    exit $?
fi

popd
exit 0
