using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JWT_Token_Implementation.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LoginController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Hellow Rokon");
    }
}
