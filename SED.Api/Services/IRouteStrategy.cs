public interface IRouteStrategy
{
    void ProcessStep(int documentId, int currentUserId);
}