using JWT_Token_Implementation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWT_Token_Implementation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private IConfiguration _configuration;
    // dictionary to store refresh tokens 
    private static Dictionary<string, string> _refreshTokens = new Dictionary<string, string>();
    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    // login
    [HttpPost("Auth")]
    public IActionResult Auth([FromBody] LoginModel model)
    {
        // Check user credentials (in a real application, you'd authenticate against a database)
        if (model is { Username: "demo", Password: "password" })
        {
            // generate token for user
            var token = GenerateAccessToken(model.Username);

            var refreshToken = Guid.NewGuid().ToString();

            // Store the refresh token (in-memory for simplicity)
            _refreshTokens[refreshToken] = model.Username;

            // return access token for user's use
            return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                            RefreshToken= refreshToken});

        }
        // unauthorized user
        return Unauthorized("Invalid credentials");
    }
    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] Models.RefreshRequest request)
    {
        if (_refreshTokens.TryGetValue(request.RefreshToken, out var userId))
        {
            // Generate a new access token
            var token = GenerateAccessToken(userId);

            // Return the new access token to the client
            return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        return BadRequest("Invalid refresh token");
    }
    // Example code to revoke a refresh token
    [HttpPost]
    public IActionResult Revoke([FromBody] RevokeRequest request)
    {
        if (_refreshTokens.ContainsKey(request.RefreshToken))
        {
            // Remove the refresh token to revoke it
            _refreshTokens.Remove(request.RefreshToken);
            return Ok("Token revoked successfully");
        }

        return BadRequest("Invalid refresh token");
    }
    // Generating token based on user information
    private JwtSecurityToken GenerateAccessToken(string userName)
    {
        // Create user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
            // Add additional claims as needed (e.g., roles, etc.)
        };

        // Create a JWT
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1), // Token expiration time
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}
