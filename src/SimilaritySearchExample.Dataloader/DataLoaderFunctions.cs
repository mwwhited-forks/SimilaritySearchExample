using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimilaritySearchExample.Persistence;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimilaritySearchExample.Dataloader;

public class DataLoaderFunctions : IDataLoaderFunctions
{
    private readonly ResourceProfilerContext _context;
    private readonly IOptions<DataloaderOptions> _options;
    private readonly ILogger _logger;
    private readonly IExpressionBuilder _expression;

    public DataLoaderFunctions(
        ResourceProfilerContext context,
        IOptions<DataloaderOptions> options,
        ILogger<DataLoaderFunctions> logger,
        IExpressionBuilder expression
        )
    {
        _context = context;
        _options = options;
        _logger = logger;
        _expression = expression;
    }

    public Task ActionAsync(DataloaderActions? action = default) =>
        (action ?? _options.Value.Action) switch
        {
            DataloaderActions.Import => ImportDataAsync(),
            DataloaderActions.Execute => ExecuteScriptAsync(),
            _ => ExportDataAsync(),
        };

    public async Task ExportDataAsync()
    {
        var targetPath = Path.GetFullPath(_options.Value.Path);
        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

        var properties = _context.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
        var dbSets = properties.Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        var exceptions = new List<Exception>();
        foreach (var dbSet in dbSets)
        {
            try
            {
                var set = dbSet.GetValue(_context, null);
                using var ms = new MemoryStream();
                await JsonSerializer.SerializeAsync(ms, set, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreReadOnlyFields = true,
                    IgnoreReadOnlyProperties = true,
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                });
                ms.Position = 0;
                using var reader = new StreamReader(ms);
                var json = await reader.ReadToEndAsync();

                var entityType = _context.Model.FindEntityType(dbSet.PropertyType.GetGenericArguments()[0]);
                var schema = entityType?.GetSchema() ?? "dbo";
                var table = entityType?.GetTableName();

                var fileName = $"{schema}.{table}.json";

                _logger.LogInformation("{Name} -> {fileName}", dbSet.Name, fileName);

                var filePath = Path.Combine(targetPath, fileName);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Name} -> {message}", dbSet.Name, Inner(ex).Message);
                exceptions.Add(ex);
            }
        }
        if (exceptions.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }

    public async Task ImportDataAsync()
    {
        var targetPath = Path.GetFullPath(_options.Value.Path);
        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

        var properties = _context.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
        var dbSets = properties.Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        var exceptions = new List<Exception>();
        foreach (var dbSet in dbSets)
        {
            var baseType = dbSet.PropertyType.GetGenericArguments()[0];
            var entityType = _context.Model.FindEntityType(baseType) ?? throw new NotSupportedException($"unable to find entity type for {baseType}");

            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null)
            {
                _logger.LogWarning("Skipping: There is no Primary Key for {entity}", baseType);
                continue;
            }

            var hasKeys = primaryKey.Properties
                .SelectMany(c => c.GetAnnotations())
                .FirstOrDefault(c => c.Name == "SqlServer:ValueGenerationStrategy" &&
                                    c.Value is SqlServerValueGenerationStrategy strategy &&
                                    strategy == SqlServerValueGenerationStrategy.IdentityColumn
                                    )
                != null;

            var schema = entityType.GetSchema() ?? "dbo";
            var table = entityType.GetTableName();

            var fileName = $"{schema}.{table}.json";
            var filePath = Path.Combine(targetPath, fileName);
            var tablename = $"[{schema}].[{table}]";

            if (!File.Exists(filePath))
            {
                _logger.LogInformation("No Data: ({entity}) {Name} -> {fileName}", entityType.ClrType, dbSet.Name, fileName);
                continue;
            }

            _logger.LogInformation("Start: ({entity}) {Name} -> {fileName}", entityType.ClrType, dbSet.Name, fileName);

            using var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                var json = await File.ReadAllTextAsync(filePath);

                using var ms = new MemoryStream();
                using var writer = new StreamWriter(ms, leaveOpen: true) { AutoFlush = true };
                writer.Write(json);
                await writer.FlushAsync();
                ms.Position = 0;

                var data = await JsonSerializer.DeserializeAsync(ms, typeof(List<>).MakeGenericType(baseType), new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreReadOnlyFields = true,
                    IgnoreReadOnlyProperties = true,
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                }) as IEnumerable ?? throw new NotSupportedException($"Unable to Deserialize to {typeof(List<>).MakeGenericType(baseType)}");

                var set = dbSet.GetValue(_context, null) as IEnumerable ??
                    throw new NotSupportedException($"Unable to read to {typeof(DbSet<>).MakeGenericType(baseType)} from {_context}");

                var missingData = _expression.ExcludeFrom(data, set, entityType);

                var method = typeof(DbSet<>).MakeGenericType(baseType)
                    .GetMethod(
                        nameof(DbSet<object>.AttachRange),
                        BindingFlags.Public | BindingFlags.Instance,
                        [typeof(IEnumerable<>).MakeGenericType(baseType)]
                        ) ?? throw new NotSupportedException();
                method.Invoke(set, [missingData]);

                _context.ChangeTracker.DetectChanges();

                var added = _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList();
                if (added.Any())
                {
                    var cnt = await _context.SaveChangesAsync();

                    _logger.LogInformation("New: ({entity}) {Name} -> {fileName} ({count})", entityType.ClrType, dbSet.Name, fileName, cnt);

                    foreach (var entry in added)
                    {
                        entry.State = EntityState.Detached;
                    }
                }

                var unchanged = _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Unchanged).ToList();
                if (unchanged.Any())
                {
                    if (hasKeys)
                    {
                        var command = $"SET IDENTITY_INSERT {tablename} ON";
                        await _context.Database.ExecuteSqlRawAsync(command);
                    }
                    foreach (var entry in unchanged)
                    {
                        entry.State = EntityState.Added;
                    }

                    var cnt = await _context.SaveChangesAsync();

                    _logger.LogInformation("({entity}) {Name} -> {fileName} ({count})", entityType.ClrType, dbSet.Name, fileName, cnt);

                    if (hasKeys)
                    {
                        var command = $"SET IDENTITY_INSERT {tablename} OFF";
                        await _context.Database.ExecuteSqlRawAsync(command);
                    }
                }

                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError("({entity}) {Name} -> {message}", entityType.ClrType, dbSet.Name, Inner(ex).Message);

                await transaction.RollbackAsync();

                exceptions.Add(ex);
            }

            if (_context.ChangeTracker.HasChanges())
            {
                _logger.LogWarning("({entity}) {Name} -> {message}", entityType.ClrType, dbSet.Name, "Changes should be clear at the bottom of the run");
            }
            _context.ChangeTracker.Clear();
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }

    public async Task ExecuteScriptAsync()
    {
        var path = Path.GetFullPath(_options.Value.Path);
        _logger.LogInformation("Execute .sql Scripts in {path}", path);
        var scripts = Directory.GetFiles(path, "*.sql");

        var exceptions = new List<Exception>();
        foreach (var script in scripts)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Executing \"{script}\"", script);

                var scriptContent = await File.ReadAllTextAsync(script);
                await _context.Database.ExecuteSqlRawAsync(scriptContent);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {script} -> {message}", script, ex.Message);
                await transaction.RollbackAsync();
                exceptions.Add(ex);
            }
        }
        if (exceptions.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }

    private static Exception Inner(Exception ex) => ex.InnerException == null ? ex : Inner(ex.InnerException);
}
