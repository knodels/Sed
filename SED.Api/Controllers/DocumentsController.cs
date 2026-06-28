using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SED.Api.DTOs;
using SED.Api.Models;
using SED.Api.Repositories;
using SED.Api.Services;
using System.Security.Claims;

namespace SED.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IApprovalService _approvalService;

    public DocumentsController(
        IDocumentRepository documentRepository,
        IApprovalService approvalService)
    {
        _documentRepository = documentRepository;
        _approvalService = approvalService;
    }

    [HttpGet]
    public IActionResult GetDocuments([FromQuery] string? status, [FromQuery] string? type, [FromQuery] string? search)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role = User.FindFirst(ClaimTypes.Role)!.Value;

        var query = _documentRepository.GetAll();

        if (role == "Employee")
            query = query.Where(d => d.AuthorId == userId);
        else if (role == "Approver")
            query = query.Where(d => d.ApprovalRoutes.Any(r => r.ApproverId == userId));

        if (!string.IsNullOrEmpty(status))
            query = query.Where(d => d.Status == status);
        if (!string.IsNullOrEmpty(type))
            query = query.Where(d => d.Type == type);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(d => d.Title.Contains(search) || d.Content.Contains(search));

        var documents = query.Select(d => new
        {
            d.Id,
            d.Title,
            d.Type,
            d.Status,
            Author = d.Author.Name,
            d.CreatedAt
        });

        return Ok(documents.ToList());
    }

    [HttpGet("{id}")]
    public IActionResult GetDocument(int id)
    {
        var document = _documentRepository.GetById(id);
        if (document == null)
            return NotFound();

        return Ok(new
        {
            document.Id,
            document.Title,
            document.Type,
            document.Content,
            document.Status,
            document.RouteType,
            Author = document.Author.Name,
            document.CreatedAt,
            ApprovalRoutes = document.ApprovalRoutes.Select(r => new
            {
                r.Id,
                r.Order,
                r.Status,
                r.Comment,
                r.SignedAt,
                Approver = r.Approver.Name
            }),
            DocumentVersions = document.DocumentVersions.Select(v => new
            {
                v.Id,
                v.FilePath,
                v.VersionNumber,
                v.CreatedAt
            })
        });
    }
    [HttpPost]
    public IActionResult Create([FromBody] CreateDocumentRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var document = new Document
        {
            Title = request.Title,
            Type = request.Type,
            Content = request.Content,
            AuthorId = userId,
            Status = "Draft",
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
        };

        _documentRepository.Add(document);
        _documentRepository.SaveChanges();

        return Ok(new { document.Id, document.Title, document.Status, document.CreatedAt });
    }
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateDocumentRequest request)
    {
        var document = _documentRepository.GetById(id);
        if (document == null)
            return NotFound();
        if (document.Status != "Draft")
            return BadRequest(new { message = "Редактирование запрещено после отправки" });

        document.Title = request.Title;
        document.Content = request.Content;
        _documentRepository.Update(document);
        _documentRepository.SaveChanges();

        return Ok(document);
    }

    [HttpDelete("{id}")]
    public IActionResult Archive(int id)
    {
        var document = _documentRepository.GetById(id);
        if (document == null)
            return NotFound();

        document.Status = "Archived";
        _documentRepository.Update(document);
        _documentRepository.SaveChanges();

        return Ok();
    }

    [HttpPost("{id}/send")]
    public IActionResult Send(int id, [FromBody] SendRequest request)
    {
        _approvalService.SendForApproval(id, request.Approvers, request.RouteType);
        return Ok(new { message = "Документ отправлен на согласование" });
    }

    [HttpPost("{id}/sign")]
    public IActionResult Sign(int id, [FromBody] SignRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        _approvalService.Sign(id, userId, request.Comment);
        return Ok(new { message = "Документ подписан" });
    }

    [HttpPost("{id}/reject")]
    public IActionResult Reject(int id, [FromBody] RejectRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        _approvalService.Reject(id, userId, request.Comment);
        return Ok(new { message = "Документ отклонён" });
    }

    [HttpPost("{id}/revise")]
    public IActionResult Revise(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        _approvalService.RequestRevise(id, userId);
        return Ok(new { message = "Запрошена доработка" });
    }

    [HttpGet("{id}/history")]
    public IActionResult GetHistory(int id)
    {
        var document = _documentRepository.GetById(id);
        if (document == null)
            return NotFound();

        var versions = document.DocumentVersions
            .OrderByDescending(v => v.VersionNumber)
            .ToList();

        return Ok(versions);
    }
}