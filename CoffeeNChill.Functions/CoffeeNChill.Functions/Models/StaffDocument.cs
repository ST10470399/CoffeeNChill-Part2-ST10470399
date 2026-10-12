namespace CoffeeNChill.Functions.Models;

/// <summary>
/// Metadata describing a single file stored in the "staff-docs" Azure File Share.
/// </summary>
public class StaffDocument
{
    public string FileName { get; set; } = string.Empty;

    public long SizeInBytes { get; set; }

    public string ContentType { get; set; } = "application/octet-stream";

    public DateTimeOffset? LastModified { get; set; }
}
