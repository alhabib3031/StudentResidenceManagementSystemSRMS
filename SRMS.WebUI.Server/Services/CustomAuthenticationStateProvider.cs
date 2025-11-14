// SRMS.WebUI.Server/Services/CustomAuthenticationStateProvider.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using SRMS.Domain.Identity;

namespace SRMS.WebUI.Server.Services;

public class CustomAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        var user = authenticationState.User;
        
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
            return false;

        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        if (!Guid.TryParse(userId, out var userGuid))
            return false;
            
        var appUser = await userManager.FindByIdAsync(userGuid.ToString());

        return appUser != null && appUser.IsActive;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    // ✅ Login Method
    public async Task LoginAsync(string email)
    {
        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            Console.WriteLine($"[AUTH] User not found: {email}");
            return;
        }

        var roles = await userManager.GetRolesAsync(user);
        
        Console.WriteLine($"[AUTH] Logging in user: {email}");
        Console.WriteLine($"[AUTH] User ID: {user.Id}");
        Console.WriteLine($"[AUTH] Roles: {string.Join(", ", roles)}");
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new("FullName", user.FullName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, "SRMS-Auth");
        _currentUser = new ClaimsPrincipal(identity);

        Console.WriteLine($"[AUTH] Claims created: {claims.Count}");
        Console.WriteLine($"[AUTH] Identity authenticated: {identity.IsAuthenticated}");

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        
        Console.WriteLine("[AUTH] Authentication state changed!");
    }

    // ✅ Logout Method
    public void Logout()
    {
        Console.WriteLine("[AUTH] Logging out...");
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }
}