using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SED.Api.Repositories;

namespace SED.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = _userRepository.GetAll()
            .Select(u => new { u.Id, u.Name, u.Email, u.Role })
            .ToList();

        return Ok(users);
    }
}