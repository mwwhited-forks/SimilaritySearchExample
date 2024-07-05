namespace SimilaritySearchExample.Dataloader;

public interface IDataLoaderFunctions
{
    Task ActionAsync(DataloaderActions? action = default);
    Task ExportDataAsync();
    Task ImportDataAsync();
    Task ExecuteScriptAsync();
}
