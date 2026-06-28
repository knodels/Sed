namespace SED.Api.Models;

public class ApprovalRoute
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public int ApproverId { get; set; }
    public User Approver { get; set; } = null!;
    public int Order { get; set; }
    public string Status { get; set; } = "Не начат";
    public string? Comment { get; set; }
    public DateTime? SignedAt { get; set; }
}