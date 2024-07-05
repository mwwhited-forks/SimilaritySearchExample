
IF "%APP_PROJECT%"=="" SET APP_PROJECT=similarity-search

SET EXTRA_ARGS=
IF /I "%1" EQU "ALL" (
    SET EXTRA_ARGS=--volumes 
)
docker compose --project-name %APP_PROJECT% down %EXTRA_ARGS%