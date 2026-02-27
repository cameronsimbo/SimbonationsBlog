using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Learn.Application.Common.Interfaces;
using Learn.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Learn.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILearnDbContext _db;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        ILearnDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _db = db;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResultVm>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        IdentityUser user = new()
        {
            UserName = request.Email,
            Email = request.Email
        };

        IdentityResult result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        // Create a UserStreak for the new user
        UserStreak streak = UserStreak.Create(user.Id);
        _db.UserStreaks.Add(streak);
        await _db.SaveChangesAsync(cancellationToken);

        AuthResultVm tokenResult = GenerateToken(user);
        return Ok(tokenResult);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResultVm>> Login(LoginRequest request)
    {
        IdentityUser? user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Unauthorized(new { Error = "Invalid email or password." });
        }

        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager
            .CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Unauthorized(new { Error = "Invalid email or password." });
        }

        AuthResultVm tokenResult = GenerateToken(user);
        return Ok(tokenResult);
    }

    private AuthResultVm GenerateToken(IdentityUser user)
    {
        string jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key not configured.");
        string issuer = _configuration["Jwt:Issuer"] ?? "LearnPlatform";
        string audience = _configuration["Jwt:Audience"] ?? "LearnPlatformApp";
        int expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out int mins) ? mins : 10080;

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
        DateTime expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        JwtSecurityToken token = new(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResultVm
        {
            Token = tokenString,
            ExpiresAt = expires,
            UserId = user.Id,
            Email = user.Email ?? string.Empty
        };
    }
}

public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
}

public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record AuthResultVm
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
