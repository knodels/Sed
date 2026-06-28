namespace SED.Api.Services;

public interface INotificationService
{
    void NotifyApprover(int documentId, int approverId);
    void NotifyAuthor(int documentId, string message);
}