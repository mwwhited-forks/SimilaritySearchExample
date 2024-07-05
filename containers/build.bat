
IF "%APP_PROJECT%"=="" SET APP_PROJECT=similarity-search

docker compose --project-name %APP_PROJECT% --file docker-compose-cpu.yml build