using SED.Api.DTOs;

namespace SED.Api.Services;

public interface IApprovalService
{
    void SendForApproval(int documentId, List<ApproverDto> approvers, string routeType);
    void Sign(int documentId, int userId, string? comment);
    void Reject(int documentId, int userId, string comment);
    void RequestRevise(int documentId, int userId);
}