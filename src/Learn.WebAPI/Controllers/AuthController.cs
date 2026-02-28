using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.Results;
using Google.Apis.Auth;
using Learn.Application.Common.Interfaces;
using Learn.Domain.Entities;
using Learn.WebAPI.Controllers.Models;
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
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        ILearnDbContext db,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _db = db;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResultVm>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(validationResult);
        }

        IdentityUser user = new()
        {
            UserName = request.Email,
            Email = request.Email
        };

        IdentityResult result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return ValidationProblem(result);
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
        ValidationResult validationResult = await _loginValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(validationResult);
        }

        IdentityUser? user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Unauthorized(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    { "Credentials", new[] { "The email or password you entered is incorrect." } }
                })
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2"
            });
        }

        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager
            .CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Unauthorized(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    { "Credentials", new[] { "The email or password you entered is incorrect." } }
                })
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2"
            });
        }

        AuthResultVm tokenResult = GenerateToken(user);
        return Ok(tokenResult);
    }

    [HttpPost("google")]
    public async Task<ActionResult<AuthResultVm>> GoogleLogin(
        GoogleLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        GoogleJsonWebSignature.Payload payload;

        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return Unauthorized(new ProblemDetails { Detail = "Invalid Google token." });
        }

        try
        {
            string googleClientId = _configuration["Google:ClientId"]
                ?? throw new InvalidOperationException("Google ClientId not configured.");

            GoogleJsonWebSignature.ValidationSettings settings = new()
            {
                Audience = new[] { googleClientId }
            };

            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (InvalidJwtException)
        {
            return Unauthorized(new ProblemDetails { Detail = "Invalid Google token." });
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or HttpRequestException)
        {
            return Unauthorized(new ProblemDetails { Detail = "Invalid Google token." });
        }

        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            return BadRequest(new ProblemDetails { Detail = "Google account has no email address." });
        }

        IdentityUser? user = await _userManager.FindByEmailAsync(payload.Email);

        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = payload.Email,
                Email = payload.Email,
                EmailConfirmed = true
            };

            IdentityResult createResult = await _userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                return ValidationProblem(createResult);
            }

            await _userManager.AddLoginAsync(
                user,
                new UserLoginInfo("Google", payload.Subject, "Google"));

            UserStreak streak = UserStreak.Create(user.Id);
            _db.UserStreaks.Add(streak);
            await _db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Link Google login if not already linked
            System.Collections.Generic.IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);
            bool hasGoogleLogin = logins.Any(l => l.LoginProvider == "Google");

            if (!hasGoogleLogin)
            {
                await _userManager.AddLoginAsync(
                    user,
                    new UserLoginInfo("Google", payload.Subject, "Google"));
            }
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

    private ActionResult ValidationProblem(ValidationResult validationResult)
    {
        Dictionary<string, string[]> errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        ValidationProblemDetails details = new(errors)
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
        };

        return BadRequest(details);
    }

    private ActionResult ValidationProblem(IdentityResult identityResult)
    {
        Dictionary<string, string[]> errors = identityResult.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray());

        ValidationProblemDetails details = new(errors)
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
        };

        return BadRequest(details);
    }
}
