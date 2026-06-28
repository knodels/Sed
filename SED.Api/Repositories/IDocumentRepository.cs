using SED.Api.Models;

namespace SED.Api.Repositories;

public interface IDocumentRepository
{
    Document? GetById(int id);
    IQueryable<Document> GetAll();
    void Add(Document document);
    void Update(Document document);
    void SaveChanges();
}