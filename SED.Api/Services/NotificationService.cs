namespace SED.Api.Services;

public class NotificationService : INotificationService
{
    private readonly List<Notification> _notifications = new();

    public void NotifyApprover(int documentId, int approverId)
    {
        _notifications.Add(new Notification
        {
            UserId = approverId,
            Message = $"Новый документ на подпись. ID документа: {documentId}",
            CreatedAt = DateTime.UtcNow
        });
    }

    public void NotifyAuthor(int documentId, string message)
    {
        _notifications.Add(new Notification
        {
            UserId = 0,
            Message = $"Документ {documentId}: {message}",
            CreatedAt = DateTime.UtcNow
        });
    }

    public List<Notification> GetNotifications(int userId)
    {
        return _notifications.Where(n => n.UserId == userId || n.UserId == 0).ToList();
    }
}

public class Notification
{
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}