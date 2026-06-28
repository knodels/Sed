using Microsoft.EntityFrameworkCore;
using SED.Api.Data;
using SED.Api.Models;

namespace SED.Api.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Document? GetById(int id)
    {
        return _context.Documents
            .Include(d => d.Author)
            .Include(d => d.ApprovalRoutes).ThenInclude(r => r.Approver)
            .Include(d => d.DocumentVersions)
            .FirstOrDefault(d => d.Id == id);
    }

    public IQueryable<Document> GetAll()
    {
        return _context.Documents.Include(d => d.Author);
    }

    public void Add(Document document)
    {
        _context.Documents.Add(document);
    }

    public void Update(Document document)
    {
        _context.Documents.Update(document);
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}