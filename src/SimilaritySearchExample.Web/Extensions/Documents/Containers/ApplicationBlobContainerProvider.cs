using Eliassen.Documents.Containers;
using Eliassen.Documents.Models;
using Eliassen.System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SimilaritySearchExample.Persistence;

namespace SimilaritySearchExample.Web.Extensions.Documents.Containers;

public class ApplicationBlobContainerProvider : IBlobContainerProvider
{
    private readonly ResourceProfilerContext _db;
    private readonly IHash _hash;

    public ApplicationBlobContainerProvider(
        string containerName,
        ResourceProfilerContext db,
        IHash hash
        )
    {
        ContainerName = containerName;
        _db = db;
        _hash = hash;
    }

    public string ContainerName { get; set; }

    public async Task DeleteContentAsync(string path) =>
        await _db.Documents
                 .Where(d => d.FileName == path && d.ContainerName == ContainerName)
                 .ExecuteDeleteAsync();

    public async Task<ContentReference?> GetContentAsync(string path)
    {
        //TODO: fix this
        throw new NotImplementedException();
        //var doc = await _db.Documents.FirstOrDefaultAsync(d => d.FileName == path && d.ContainerName == ContainerName);

        //if (doc == null) return null;

        //var ms = new MemoryStream();
        //ms.Write(doc.Content, 0, doc.Content.Length);
        //ms.Position = 0;

        //return new ContentReference
        //{
        //    ContentType = doc.ContentType,
        //    FileName = doc.FileName,
        //    Content = ms,
        //};
    }

    public async Task<ContentMetaDataReference?> GetContentMetaDataAsync(string path)
    {
        //TODO: fix this
        throw new NotImplementedException();
        //var doc = await _db.Documents
        //                   .Include(i => i.Data)
        //                   .FirstOrDefaultAsync(d => d.FileName == path && d.ContainerName == ContainerName);
        //return doc == null ? null : new ContentMetaDataReference
        //{
        //    FileName = doc.FileName,
        //    ContentType = doc.ContentType,
        //};
    }

    public IQueryable<ContentMetaDataReference> QueryContent() =>
        from document in _db.Documents
        select new ContentMetaDataReference
        {
            ContentType = document.ContentType,
            FileName = document.FileName,
        };

    public async Task StoreContentAsync(ContentReference reference, Dictionary<string, string>? metaData = null, bool overwrite = false)
    {
        //TODO: fix this
        throw new NotImplementedException();

        //var doc = await _db.Documents.FirstOrDefaultAsync(d => d.FileName == reference.FileName && d.ContainerName == ContainerName);
        //if (doc != null && !overwrite) throw new InvalidOperationException();

        //using var ms = new MemoryStream();
        //await reference.Content.CopyToAsync(ms);

        //if (doc != null)
        //{
        //    //doc.FileName = reference.FileName;
        //    doc.ContentType = reference.ContentType;
        //    //doc.Hash = _hash.GetHash(reference.FileName);
        //    doc.ContainerName = ContainerName;
        //    doc.Content = ms.ToArray();
        //}
        //else
        //{
        //    doc ??= new()
        //    {
        //        ContainerName = ContainerName,
        //        Content = ms.ToArray(),
        //        ContentType = reference.ContentType,
        //        FileName = reference.FileName,
        //        Hash = _hash.GetHash(reference.FileName),
        //    };
        //    _db.Documents.Add(doc);
        //}

        //await _db.SaveChangesAsync();

        //if (metaData != null)
        //{
        //    await StoreContentMetaDataAsync(new()
        //    {
        //        ContentType = reference.ContentType,
        //        FileName = reference.FileName,
        //        MetaData = metaData,
        //    });
        //}
    }

    public async Task<bool> StoreContentMetaDataAsync(ContentMetaDataReference reference)
    {
        //TODO: fix this
        throw new NotImplementedException();

        //var doc = await _db.Documents
        //                   .Include(i => i.Data)
        //                   .FirstOrDefaultAsync(d => d.FileName == reference.FileName && d.ContainerName == ContainerName);
        //if (doc == null) return false;

        ////TODO: do something smarter

        //doc.Data.Clear();
        //var data = from item in reference.MetaData
        //           select new DocumentData
        //           {
        //               Name = item.Key,
        //               Value = item.Value,
        //           };
        //foreach (var item in data)
        //{
        //    doc.Data.Add(item);
        //}
        //await _db.SaveChangesAsync();

        //return true;
    }
}
