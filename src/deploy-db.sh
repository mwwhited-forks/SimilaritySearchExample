#!/bin/bash

set -e

SCRIPT_ROOT=$(dirname "$0")
pushd "$SCRIPT_ROOT"

DATABASE_NAME="GreenOnionDb"
DATABASE_USER="sa"
DATABASE_PASSWORD="Gr33n0n!on"
COLLECTION_ASSEMBLY="GreenOnion.DB"
PROJECT_ASSEMBLY="GreenOnion.Entities"

# CALL generate-db.bat (uncomment and convert if needed)

echo "Deploy Database Schema"
dotnet publish --configuration Release \
    /p:TargetServerName=127.0.0.1 \
    /p:TargetDatabaseName="$DATABASE_NAME" \
    /p:TargetUser="$DATABASE_USER" \
    /p:TargetPassword="$DATABASE_PASSWORD" \
    /p:DeployOnPublish=true \
    "$COLLECTION_ASSEMBLY"

if [ $? -ne 0 ]; then
    echo "Error during database schema deployment"
    popd
    exit $?
fi

echo "Deploy Data"
dotnet run --configuration Release --project Tools/GreenOnion.Dataloader -- \
    "--connection-string=Server=127.0.0.1;Database=$DATABASE_NAME;User ID=$DATABASE_USER;Password=$DATABASE_PASSWORD;TrustServerCertificate=True;" \
    "--data-path=../data" --action=Import

if [ $? -ne 0 ]; then
    echo "Error during data deployment"
    popd
    exit $?
fi

popd
exit 0
