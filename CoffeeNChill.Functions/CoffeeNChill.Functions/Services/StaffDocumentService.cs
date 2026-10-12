using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Services;

/// <summary>
/// Handles upload, listing and download of staff documents stored in the
/// "staff-docs" Azure File Share.
/// </summary>
public class StaffDocumentService
{
    private const string ShareName = "staff-docs";

    private readonly ShareClient _shareClient;

    public StaffDocumentService(StorageService storageService)
    {
        var shareServiceClient = storageService.GetFileShareServiceClient();

        _shareClient = shareServiceClient.GetShareClient(ShareName);

        _shareClient.CreateIfNotExists();
    }

    private ShareDirectoryClient GetRootDirectory()
    {
        return _shareClient.GetRootDirectoryClient();
    }

    /// <summary>
    /// Streams the given content into the file share under the supplied file name.
    /// Overwrites the file if it already exists.
    /// </summary>
    public async Task<StaffDocument> UploadDocumentAsync(
        string fileName,
        Stream content,
        string contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        if (!content.CanSeek)
        {
            // ShareFileClient.CreateAsync needs the final length up front, so if the
            // incoming stream can't report its length/seek, buffer it first.
            var buffered = new MemoryStream();
            await content.CopyToAsync(buffered);
            buffered.Position = 0;
            content = buffered;
        }

        var directory = GetRootDirectory();
        var fileClient = directory.GetFileClient(fileName);

        var length = content.Length;

        await fileClient.CreateAsync(length);

        if (length > 0)
        {
            await fileClient.UploadRangeAsync(new HttpRange(0, length), content);
        }


        await fileClient.SetHttpHeadersAsync(
            new ShareFileSetHttpHeadersOptions
            {
                HttpHeaders = new ShareFileHttpHeaders
                {
                    ContentType = contentType
                }
            });


        var properties = await fileClient.GetPropertiesAsync();

        return new StaffDocument
        {
            FileName = fileName,
            SizeInBytes = properties.Value.ContentLength,
            ContentType = properties.Value.ContentType ?? contentType,
            LastModified = properties.Value.LastModified
        };
    }

    /// <summary>
    /// Lists every document currently stored in the root of the file share, with metadata.
    /// </summary>
    public async Task<List<StaffDocument>> ListDocumentsAsync()
    {
        var directory = GetRootDirectory();
        var documents = new List<StaffDocument>();

        await foreach (var item in directory.GetFilesAndDirectoriesAsync())
        {
            if (item.IsDirectory)
            {
                continue;
            }

            var fileClient = directory.GetFileClient(item.Name);

            ShareFileProperties properties;

            try
            {
                properties = await fileClient.GetPropertiesAsync();
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // File was removed between listing and reading its properties.
                continue;
            }

            documents.Add(new StaffDocument
            {
                FileName = item.Name,
                SizeInBytes = properties.ContentLength,
                ContentType = properties.ContentType ?? "application/octet-stream",
                LastModified = properties.LastModified
            });
        }

        return documents;
    }

    /// <summary>
    /// Downloads a document as a stream, or null if it doesn't exist.
    /// </summary>
    public async Task<(Stream Content, string ContentType, long Length)?> DownloadDocumentAsync(
        string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        var directory = GetRootDirectory();
        var fileClient = directory.GetFileClient(fileName);

        if (!await fileClient.ExistsAsync())
        {
            return null;
        }

        var download = await fileClient.DownloadAsync();

        return (
            download.Value.Content,
            download.Value.ContentType ?? "application/octet-stream",
            download.Value.ContentLength);
    }
}
