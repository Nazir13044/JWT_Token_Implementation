﻿using JWT_Token_Implementation.Dto;
using JWT_Token_Implementation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
    private IMemoryCache _memoryCache;
    // dictionary to store refresh tokens 
    private static Dictionary<string, string> _refreshTokens = new Dictionary<string, string>();
    public AuthController(IConfiguration configuration, IMemoryCache memoryCache)
    {
        _configuration = configuration;
        _memoryCache= memoryCache;
    }
    // login
    [HttpPost("Auth")]
    public IActionResult Auth([FromBody] LoginModel model)
    {
        DataModels dt = new DataModels();
        var userInformation = dt.UserInformation();
        // Check user credentials (in a real application, you'd authenticate against a database)
        if (userInformation.Any(x => x.Username == model.Username && x.Password == model.Password))
        {
            // generate token for user
            var token = GenerateAccessToken(model.Username);

            var refreshToken = Guid.NewGuid().ToString();

            // Store the refresh token (in-memory for simplicity)
            _refreshTokens[refreshToken] = model.Username;

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                .SetPriority(CacheItemPriority.NeverRemove)
                .SetSize(2048);

            _memoryCache.Set(refreshToken, model.Username, cacheOptions);

            // return access token for user's use
            return Ok(new
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken
            });

        }
        // unauthorized user
        return Unauthorized("Invalid credentials");
    }
    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] Models.RefreshRequest request)
    {
        //Dictionary
        //if (_refreshTokens.TryGetValue(request.RefreshToken, out var userId))
        //{
        //    // Generate a new access token
        //    var token = GenerateAccessToken(userId);

        //    // Return the new access token to the client
        //    return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token) });
        //}

        //In memeory cash
        if (_memoryCache.TryGetValue(request.RefreshToken, out var userId))
        {
            // Generate a new access token
            var token = GenerateAccessToken(userId.ToString());

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
