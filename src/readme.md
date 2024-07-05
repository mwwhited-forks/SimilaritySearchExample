# SimilaritySearchExample Services

## Summary

## Notes

## Processes

### Updated Database Schema

After updating the entity framework project `SimilaritySearchExample.Persistence` with your 
changes.  Run the `generate-db.bat` script in the `.\src` folder to updated
the database generation project.  This will replace the `.\Generated\GreenOnionDbContext.sql` 
in the `SimilaritySearchExample.DB` project.  Additional SQL objects may be defined in the 
`SimilaritySearchExample.DB` as required.  This project generates a dacpac for use with 
`sqlpackage` for deploying schema changes to MS SQL servers.  

To apply these changes to you local database you can run the `deploy-db.bat`
which will apply schema updates and master/test database to your local database.

If you have having blocked schema changes you might need to use the `deploy-db.bat clean` 
this will enable the schema deployment without blocking on dataloss.  

## Scripts

| Script      | Usage          |
|-------------|----------------|
| 
