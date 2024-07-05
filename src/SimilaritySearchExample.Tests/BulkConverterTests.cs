using Eliassen.Common;
using Eliassen.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceProfiler.Tests;

[TestClass]
public class BulkConverterTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public async Task ConvertAll()
    {
        var sourcePath = @"C:\Repos\Nucleus\Net.Libs\docs\";

        var files = Directory.EnumerateFiles(sourcePath, "*.md", SearchOption.AllDirectories);

        var config = new ConfigurationBuilder()
            .Build()
            ;

        var services = new ServiceCollection()
            .TryAllCommonExtensions(config)
            .BuildServiceProvider()
            ;

        var converter = services.GetRequiredService<IDocumentConversion>();
        var tools = services.GetRequiredService<IDocumentTypeTools>();

        var markdown = tools.GetByFileExtension(".md") ?? throw new NotSupportedException();
        var pdf = tools.GetByFileExtension(".pdf") ?? throw new NotSupportedException();

        foreach (var file in files)
        {
            var outFile = Path.ChangeExtension(file, pdf.FileExtensions.First());
            if (File.Exists(outFile)) continue;

            try
            {
                TestContext.WriteLine(new string('-', 20));
                TestContext.WriteLine($"In: {file}");

                using var input = File.OpenRead(file);
                using var output = File.OpenWrite(outFile);

                await converter.ConvertAsync(input, markdown.ContentTypes.First(), output, pdf.ContentTypes.First());

                TestContext.WriteLine($"Out: {outFile}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Err: {ex.Message}");
            }
        }
    }
}
