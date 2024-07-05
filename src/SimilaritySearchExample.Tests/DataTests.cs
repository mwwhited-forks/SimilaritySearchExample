using Eliassen.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimilaritySearchExample.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimilaritySearchExample.Tests;

[TestClass]
public class DataTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ExportData()
    {
        var connectionString =
            "Server=127.0.0.1;Database=ResourceProfilerDb;User ID=sa;Password=S1m1l41tyS34rch;TrustServerCertificate=True;"
            ;

        var service = new ServiceCollection()
            .AddDbContext<ResourceProfilerContext>(opt => opt.UseSqlServer(connectionString))
            //.AddDbContext<ResourceProfilerContext>(opt => opt.UseSqlServer(connectionString))
            .BuildServiceProvider()
            ;

        var db = service.GetRequiredService<ResourceProfilerContext>();
        var targetPath = Path.GetFullPath(Path.Combine(TestContext.TestRunDirectory ?? ".", @"..\..\..\data"));
        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

        var properties = db.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
        var dbSets = properties.Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        foreach (var dbSet in dbSets)
        {
            try
            {
                var set = dbSet.GetValue(db, null);
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

                var entityType = db.Model.FindEntityType(dbSet.PropertyType.GetGenericArguments()[0]);
                var schema = entityType?.GetSchema() ?? "dbo";
                var table = entityType?.GetTableName();

                var fileName = $"{schema}.{table}.json";

                TestContext.WriteLine($"{dbSet.Name} -> {fileName}");

                var filePath = Path.Combine(targetPath, fileName);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"ERROR: {dbSet.Name} -> {Inner(ex).Message}");
            }
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ImportData()
    {
        var connectionString =
            "Server=127.0.0.1;Database=ResourceProfilerDb;User ID=sa;Password=S1m1l41tyS34rch;TrustServerCertificate=True;"
            ;

        var service = new ServiceCollection()
            .AddDbContext<ResourceProfilerContext>(opt => opt.UseSqlServer(connectionString))
            .BuildServiceProvider()
            ;

        var db = service.GetRequiredService<ResourceProfilerContext>();

        db.ChangeTracker.DetectingAllChanges += (s, e) =>
        {
            if (s is ChangeTracker ct)
            {
                var unchanged = ct.Entries().Where(e => e.State == EntityState.Unchanged);
                foreach (var un in unchanged)
                {
                    if (un.GetDatabaseValues() != null)
                    {
                        un.State = EntityState.Detached;
                    }
                }

                //var mi = ct.Context.GetType()
                //    .GetMethod(nameof(DbContext.Set), BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes)
                //    ?? throw new NotSupportedException("Unable to find Set<T>() method")
                //    ;
                //var types = unchanged.Select(e => e.Metadata).Distinct().ToArray();
                //foreach (var type in types)
                //{
                //    var set = mi.MakeGenericMethod(type.ClrType).Invoke(ct.Context, null); //DbSet<T>
                //    var pk = type.FindPrimaryKey();
                //    if (pk == null)
                //    {
                //        //TODO: do something here
                //        continue;
                //    }
                //    var entityKeys = (from ee in unchanged
                //                      where ee.Metadata == type
                //                      let propertyValues = from prop in pk.Properties
                //                                           let member = prop.GetMemberInfo(false, true) switch
                //                                           {
                //                                               PropertyInfo pi => pi.GetValue(ee.Entity, null),
                //                                               FieldInfo fi => fi.GetValue(ee.Entity),
                //                                               _ => throw new NotSupportedException("Unable to map value")
                //                                           }
                //                                           select member
                //                      let keys = propertyValues.ToArray()
                //                      let eo = ct.Context.Find(type.ClrType, keys)
                //                      select (ee, eo)
                //                      ).ToArray();
                //    //db.Set<UserMst>()
                //    //  .Where(u=>u.UserId == 1 && u.SchoolDistrictId == 2 || )
                //    //  .Select(u=>u.UserId)
                //}
            }
        };


        var targetPath = Path.GetFullPath(Path.Combine(TestContext.TestRunDirectory ?? ".", @"..\..\..\data"));
        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

        var properties = db.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
        var dbSets = properties.Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        foreach (var dbSet in dbSets)
        {
            var baseType = dbSet.PropertyType.GetGenericArguments()[0];
            var entityType = db.Model.FindEntityType(baseType);

            var hasKeys = false;
            //TODO: do something to support PGsql
            //entityType?.GetProperties()
            //                      .Where(c => c.IsPrimaryKey())
            //                      .SelectMany(c => c.GetAnnotations())
            //                      .FirstOrDefault(c => c.Name == "SqlServer:ValueGenerationStrategy" &&
            //                                          c.Value is SqlServerValueGenerationStrategy strategy &&
            //                                          strategy == SqlServerValueGenerationStrategy.IdentityColumn
            //                                          )
            //                      != null;

            var schema = entityType?.GetSchema() ?? "dbo";
            var table = entityType?.GetTableName();

            var fileName = $"{schema}.{table}.json";
            var filePath = Path.Combine(targetPath, fileName);
            var tablename = $"[{schema}].[{table}]";

            if (!File.Exists(filePath))
            {
                TestContext.WriteLine($"No Data: {dbSet.Name} -> {fileName}");
                continue;
            }

            TestContext.WriteLine($"Start: {dbSet.Name} -> {fileName}");

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
                });

                var set = dbSet.GetValue(db, null);
                var method = typeof(DbSet<>).MakeGenericType(baseType)
                    .GetMethod(
                        nameof(DbSet<object>.AttachRange),
                        BindingFlags.Public | BindingFlags.Instance,
                        [typeof(IEnumerable<>).MakeGenericType(baseType)]
                        );
                method.Invoke(set, [data]);

                db.ChangeTracker.DetectChanges();

                var added = db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList();
                if (added.Any() && db.ChangeTracker.HasChanges())
                {
                    var cnt = await db.SaveChangesAsync();
                    TestContext.WriteLine($"New: {dbSet.Name} -> {fileName} ({cnt})");

                    foreach (var entry in added)
                    {
                        entry.State = EntityState.Detached;
                    }
                }

                var unchanged = db.ChangeTracker.Entries().Where(e => e.State == EntityState.Unchanged).ToList();
                if (unchanged.Any())
                {
                    if (hasKeys)
                    {
                        db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
                        await db.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {tablename} ON");

                    }
                    foreach (var entry in unchanged)
                    {
                        entry.State = EntityState.Added;
                    }

                    var cnt = await db.SaveChangesAsync();

                    TestContext.WriteLine($"{dbSet.Name} -> {fileName} ({cnt})");

                    if (hasKeys && db.Database.CurrentTransaction != null)
                    {
                        await db.Database.CommitTransactionAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                db.ChangeTracker.Clear();
                TestContext.WriteLine($"ERROR: {dbSet.Name} -> {Inner(ex).Message}");

                if (hasKeys && db.Database.CurrentTransaction != null)
                {
                    await db.Database.RollbackTransactionAsync();
                }
            }
            finally
            {
                if (hasKeys)
                {
                    await db.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {tablename} OFF");
                }
            }
        }
    }

    private static Exception Inner(Exception ex) => ex.InnerException == null ? ex : Inner(ex.InnerException);
}
