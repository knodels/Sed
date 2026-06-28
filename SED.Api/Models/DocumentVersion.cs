namespace SED.Api.Models;

public class DocumentVersion
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public string FilePath { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}