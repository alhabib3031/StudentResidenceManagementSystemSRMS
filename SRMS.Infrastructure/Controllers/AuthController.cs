using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SRMS.Application.Identity.DTOs;
using SRMS.Application.Identity.Interfaces;

namespace SRMS.Infrastructure.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpGet("google-login")]
    public IActionResult GoogleLogin(string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl });
        
        var properties = new AuthenticationProperties 
        { 
            RedirectUri = redirectUrl,
            Items = { { "scheme", GoogleDefaults.AuthenticationScheme } }
        };
        
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
    
    [HttpGet("google-callback")]
    public async Task<RedirectResult> GoogleCallback(string? returnUrl = null)
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        
        if (!authenticateResult.Succeeded)
        {
            return Redirect("/login?error=google-auth-failed");
        }
        
        var googleToken = authenticateResult.Properties?.GetTokenValue("access_token");
        
        if (string.IsNullOrEmpty(googleToken))
        {
            return Redirect("/login?error=no-google-token");
        }
        
        var result = await _authService.GoogleLoginAsync(new GoogleAuthDto 
        { 
            GoogleToken = googleToken 
        });
        
        if (result.Success)
        {
            // Store JWT token in cookie or localStorage via JavaScript
            Response.Cookies.Append("AuthToken", result.Token!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
            
            return Redirect(returnUrl ?? "/");
        }
        
        return Redirect($"/login?error={result.Message}");
    }
}