using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SRMS.Application.Identity.DTOs;
using SRMS.Domain.Identity;

namespace SRMS.WebUI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
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
    public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded)
        {
            return Redirect("/login?error=google-auth-failed");
        }

        var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Redirect("/login?error=no-email-from-google");
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Optional: Auto-register the user
            return Redirect("/login?error=user-not-found");
        }

        // Sign in the user with a persistent cookie
        await _signInManager.SignInAsync(user, isPersistent: true);

        return Redirect(returnUrl ?? "/");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] LoginDto model)
    {
        if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
        {
            return Redirect("/login?error=missing-fields");
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Redirect("/login?error=invalid-credentials");
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            var redirectUrl = role switch
            {
                "SuperRoot" => "/superroot",
                "Admin" => "/admin",
                "Manager" => "/manager",
                "Student" => "/student",
                _ => "/"
            };

            return Redirect(redirectUrl);
        }

        if (result.IsLockedOut)
        {
            return Redirect("/login?error=locked-out");
        }

        return Redirect("/login?error=invalid-credentials");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/login");
    }
}