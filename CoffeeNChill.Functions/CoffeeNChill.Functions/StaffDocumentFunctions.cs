using CoffeeNChill.Functions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions;

/// <summary>
/// HTTP endpoints for uploading, listing and downloading staff documents
/// stored in the "staff-docs" Azure File Share.
/// </summary>
public class StaffDocumentFunctions
{
    private readonly StaffDocumentService _staffDocumentService;
    private readonly ILogger<StaffDocumentFunctions> _logger;

    public StaffDocumentFunctions(
        StaffDocumentService staffDocumentService,
        ILogger<StaffDocumentFunctions> logger)
    {
        _staffDocumentService = staffDocumentService;
        _logger = logger;
    }

    [Function("UploadStaffDocument")]
    public async Task<IActionResult> UploadStaffDocument(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")]
        HttpRequest req)
    {
        try
        {
            if (!req.HasFormContentType)
            {
                return new BadRequestObjectResult(
                    "No file was supplied. Send the file as multipart/form-data using the 'file' field.");
            }

            var form = await req.ReadFormAsync();

            if (form.Files.Count == 0)
            {
                return new BadRequestObjectResult(
                    "No file was supplied. Send the file as multipart/form-data using the 'file' field.");
            }

            var file = form.Files["file"] ?? form.Files[0];

            if (file.Length == 0)
            {
                return new BadRequestObjectResult("The uploaded file is empty.");
            }

            // Guard against path traversal / invalid characters in the file name.
            var safeFileName = Path.GetFileName(file.FileName);

            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                return new BadRequestObjectResult("A valid file name is required.");
            }

            await using var stream = file.OpenReadStream();

            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType;

            var document = await _staffDocumentService.UploadDocumentAsync(
                safeFileName,
                stream,
                contentType);

            return new CreatedResult($"/api/documents/download/{safeFileName}", document);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid staff document upload request.");
            return new BadRequestObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading staff document.");
            return new ObjectResult($"Error uploading staff document: {ex.Message}")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("ListStaffDocuments")]
    public async Task<IActionResult> ListStaffDocuments(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")]
        HttpRequest req)
    {
        try
        {
            var documents = await _staffDocumentService.ListDocumentsAsync();

            return new OkObjectResult(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing staff documents.");
            return new ObjectResult($"Error listing staff documents: {ex.Message}")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("DownloadStaffDocument")]
    public async Task<IActionResult> DownloadStaffDocument(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")]
        HttpRequest req,
        string fileName)
    {
        try
        {
            var safeFileName = Path.GetFileName(fileName);

            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                return new BadRequestObjectResult("A valid file name is required.");
            }

            var result = await _staffDocumentService.DownloadDocumentAsync(safeFileName);

            if (result == null)
            {
                return new NotFoundObjectResult(
                    $"Staff document '{safeFileName}' was not found.");
            }

            var (content, contentType, _) = result.Value;

            return new FileStreamResult(content, contentType)
            {
                FileDownloadName = safeFileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading staff document '{FileName}'.", fileName);
            return new ObjectResult($"Error downloading staff document: {ex.Message}")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
