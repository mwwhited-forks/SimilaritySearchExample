# SimilaritySearchExample Services

## Summary

## Notes

To run the application execute the [build](../containers/build.bat) and then [up](../containers/up.bat) 
scripts to start the dependant docker containers.  Then run [run](../src/run.bat) to build and start
the application.  To access the running application navigate to [swagger](http://localhost:5107/swagger/index.html)

## Processes

### Updated Database Schema

After updating the entity framework project `SimilaritySearchExample.Persistence` with your 
changes.  Run the `generate-db.bat` script in the `.\src` folder to updated
the database generation project.  This will replace the `.\Generated\ResourceProfilerDbContext.sql` 
in the `SimilaritySearchExample.DB` project.  Additional SQL objects may be defined in the 
`SimilaritySearchExample.DB` as required.  This project generates a dacpac for use with 
`sqlpackage` for deploying schema changes to MS SQL servers.  

To apply these changes to you local database you can run the `deploy-db.bat`
which will apply schema updates and master/test database to your local database.

If you have having blocked schema changes you might need to use the `deploy-db.bat clean` 
this will enable the schema deployment without blocking on data loss.  

## Application Scripts

`.\src` folder

| Script            | Usage                                                                       |
|-------------------|-----------------------------------------------------------------------------|
| build.bat         | build the .Net source code                                                  | 
| generate-db.bat   | generate database creation scripts to use with dacpac deployment            |
| deploy-db.bat     | deploy the database schema, test and master data                            |
| generate-docs.bat | generate documentation related to the application in to the .\docs folders  |
| test.bat          | run unit tests                                                              |

## Container Scripts

`.\containers\`

| Script            | Usage                                                                     |
|-------------------|---------------------------------------------------------------------------|
| build.bat         | run docker compose build (no gpu acceleration)                            | 
| build-cuda.bat    | run docker compose build (gpu acceleration using Nvidia/CUDA)             | 
| up.bat            | run docker containers (no gpu acceleration)                               | 
| up-cuda.bat       | run docker containers (gpu acceleration using Nvidia/CUDA)                | 
| stop.bat          | shutdown the docker containers                                            |
| down.bat          | shutdown and remove the docker containers                                 |
| down.bat clean    | shutdown, remove docker containers and cleanup persistence volumes        |

- [docker-compose-cpu](../containers/docker-compose-cpu.yml) composite docker compose without GPU acceleration
- [docker-compose-cuda](../containers/docker-compose-cuda.yml) composite docker compose with Nvidia/CUDA GPU acceleration
- [docker-compose.apache-tika](../containers/docker-compose.apache-tika.yml) Apache Tika is a Document Conversion Service
- [docker-compose.azurite](../containers/docker-compose.azurite.yml) Azure Storage Emulator (blobs and queues)
- [docker-compose.ollama](../containers/docker-compose.ollama.yml) Ollama host (LLM provider)
- [docker-compose.open-webui](../containers/docker-compose.open-webui.yml) OpenWeb-UI (Ollama/LLM Frontend)
- [docker-compose.qdrant](../containers/docker-compose.qdrant.yml) Qdrant Vector Database
- [docker-compose.sbert](../containers/docker-compose.sbert.yml) Sentence Transformer host (SBERT/Text Embedding)
- [docker-compose.smtp4dev](../containers/docker-compose.smtp4dev.yml) Development Email Server
- [docker-compose.sql-server](../containers/docker-compose.sql-server.yml) MS SQl Server

