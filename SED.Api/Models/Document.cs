namespace SED.Api.Models;

public class Document
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public string Status { get; set; } = "Draft";
    public string RouteType { get; set; } = "Sequential";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ApprovalRoute> ApprovalRoutes { get; set; } = new();
    public List<DocumentVersion> DocumentVersions { get; set; } = new();
}