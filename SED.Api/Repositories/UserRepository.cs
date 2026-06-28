using SED.Api.Data;
using SED.Api.Models;

namespace SED.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public User? GetByEmail(string email)
    {
        return _context.Users.FirstOrDefault(u => u.Email == email);
    }

    public User? GetById(int id)
    {
        return _context.Users.Find(id);
    }

    public List<User> GetAll()
    {
        return _context.Users.ToList();
    }
}