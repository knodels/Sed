using SED.Api.DTOs;
using SED.Api.Models;
using SED.Api.Repositories;
using SED.Api.Services;

namespace SED.Api.Services;

public class ApprovalService : IApprovalService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public ApprovalService(
        IDocumentRepository documentRepository,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _documentRepository = documentRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public void SendForApproval(int documentId, List<ApproverDto> approvers, string routeType)
    {
        var document = _documentRepository.GetById(documentId)
            ?? throw new InvalidOperationException("Документ не найден");

        if (document.Status != "Draft")
            throw new InvalidOperationException($"Нельзя отправить документ в статусе {document.Status}. Ожидается статус Draft");

        if (document.ApprovalRoutes.Any())
            throw new InvalidOperationException("Маршрут согласования уже создан для этого документа");

        if (approvers == null || approvers.Count == 0)
            throw new InvalidOperationException("Не выбран ни один согласующий");

        document.Status = "Pending";
        document.RouteType = routeType;

        foreach (var approver in approvers)
        {
            var route = new ApprovalRoute
            {
                DocumentId = documentId,
                ApproverId = approver.UserId,
                Order = approver.Order,
                Status = (routeType == "Parallel" || approver.Order == 1) ? "Ожидает" : "Не начат"
            };
            document.ApprovalRoutes.Add(route);
        }

        _documentRepository.Update(document);
        _documentRepository.SaveChanges();

        var firstApprovers = document.ApprovalRoutes
            .Where(r => r.Status == "Ожидает")
            .Select(r => r.ApproverId)
            .ToList();

        foreach (var approverId in firstApprovers)
            _notificationService.NotifyApprover(documentId, approverId);
    }

    public void Sign(int documentId, int userId, string? comment)
    {
        var document = _documentRepository.GetById(documentId)
            ?? throw new InvalidOperationException("Документ не найден");

        if (document.Status != "Pending")
            throw new InvalidOperationException($"Нельзя подписать документ в статусе {document.Status}");

        var route = document.ApprovalRoutes
            .FirstOrDefault(r => r.ApproverId == userId && r.Status == "Ожидает");

        if (route == null)
        {
            var existingRoute = document.ApprovalRoutes
                .FirstOrDefault(r => r.ApproverId == userId);
            var user = _userRepository.GetById(userId);
            var userName = user?.Name ?? "Неизвестный";

            if (existingRoute != null)
                throw new InvalidOperationException($"{userName} уже выполнил действие: {existingRoute.Status}");
            else
                throw new InvalidOperationException($"{userName} не является согласующим для этого документа");
        }

        route.Status = "Подписан";
        route.Comment = comment;
        route.SignedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        _documentRepository.Update(document);
        _documentRepository.SaveChanges();

        ProcessAfterSign(document);
    }

    public void Reject(int documentId, int userId, string comment)
    {
        var document = _documentRepository.GetById(documentId)
            ?? throw new InvalidOperationException("Документ не найден");

        var route = document.ApprovalRoutes
            .FirstOrDefault(r => r.ApproverId == userId && r.Status == "Ожидает")
            ?? throw new InvalidOperationException("Нет прав на отклонение");

        route.Status = "Отклонён";
        route.Comment = comment;
        document.Status = "Rejected";

        _documentRepository.Update(document);
        _documentRepository.SaveChanges();

        _notificationService.NotifyAuthor(documentId, "Документ отклонён: " + comment);
    }

    public void RequestRevise(int documentId, int userId)
    {
        var document = _documentRepository.GetById(documentId)
            ?? throw new InvalidOperationException("Документ не найден");

        var route = document.ApprovalRoutes
            .FirstOrDefault(r => r.ApproverId == userId && r.Status == "Ожидает")
            ?? throw new InvalidOperationException("Нет прав на запрос доработки");

        route.Status = "Требуется доработка";
        document.Status = "Revision";

        _documentRepository.Update(document);
        _documentRepository.SaveChanges();

        _notificationService.NotifyAuthor(documentId, "Запрошена доработка документа");
    }

    private void ProcessAfterSign(Document document)
    {
        if (document.RouteType == "Sequential")
        {
            var nextRoute = document.ApprovalRoutes
                .FirstOrDefault(r => r.Status == "Не начат");

            if (nextRoute != null)
            {
                nextRoute.Status = "Ожидает";
                _documentRepository.Update(document);
                _documentRepository.SaveChanges();
                _notificationService.NotifyApprover(document.Id, nextRoute.ApproverId);
            }
            else
            {
                document.Status = "Approved";
                _documentRepository.Update(document);
                _documentRepository.SaveChanges();
                _notificationService.NotifyAuthor(document.Id, "Документ утверждён");
            }
        }
        else
        {
            var allSigned = document.ApprovalRoutes.All(r => r.Status == "Подписан");

            if (allSigned)
            {
                document.Status = "Approved";
                _documentRepository.Update(document);
                _documentRepository.SaveChanges();
                _notificationService.NotifyAuthor(document.Id, "Документ утверждён");
            }
        }
    }
}