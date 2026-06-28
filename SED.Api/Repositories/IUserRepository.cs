using SED.Api.Models;

namespace SED.Api.Repositories;

public interface IUserRepository
{
    User? GetByEmail(string email);
    User? GetById(int id);
    List<User> GetAll();
}