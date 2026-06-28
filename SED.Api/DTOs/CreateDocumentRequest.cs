namespace SED.Api.DTOs;

public class CreateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}