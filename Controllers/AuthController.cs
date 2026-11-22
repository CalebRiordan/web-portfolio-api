using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PanChatApi.Models;

namespace WebPortfolioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration config) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserDto request)
    {
        var expectedUser = config["Admin:Username"];
        var expectedHash = config["Admin:PasswordHash"];

        if (
            expectedHash == null
            || !string.Equals(request.Username, expectedUser)
            || !BCrypt.Net.BCrypt.Verify(request.Password, expectedHash)
        )
            return Unauthorized("Invalid credentials");

        var identity = new ClaimsIdentity([new(ClaimTypes.Name, request.Username)]);
        await HttpContext.SignInAsync("AdminCookie", new ClaimsPrincipal(identity));
        return Ok();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("AdminCookie");
        return Ok();
    }
}
