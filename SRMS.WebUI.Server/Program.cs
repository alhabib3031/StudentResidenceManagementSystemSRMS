using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using SRMS.Application;
using SRMS.Domain.Identity;
using SRMS.Domain.Identity.Constants;
using SRMS.Infrastructure;
using SRMS.Infrastructure.Configurations.Data;
using SRMS.WebUI.Server.Components;
using SRMS.WebUI.Server.Services;
using SRMS.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Localization services
builder.Services.AddLocalization();

// ═══════════════════════════════════════════════════════════
// 1️⃣ MudBlazor Services
// ═══════════════════════════════════════════════════════════
builder.Services.AddMudServices();

// ═══════════════════════════════════════════════════════════
// 2️⃣ Razor Components + Interactive Server
// ═══════════════════════════════════════════════════════════
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

// ═══════════════════════════════════════════════════════════
// 3️⃣ Application & Infrastructure Layers
// ═══════════════════════════════════════════════════════════
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ═══════════════════════════════════════════════════════════
// 4️⃣ Identity Configuration
// ═══════════════════════════════════════════════════════════
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

// Authentication State Provider for Blazor
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

// ═══════════════════════════════════════════════════════════
// 5️⃣ JWT Authentication (for API)
// ═══════════════════════════════════════════════════════════
var jwtKey = builder.Configuration["Jwt:Key"] ?? "a6d581f7ff0e7b038a292fc245128b1a8e47dab02f02c6a7f6299c7862cf08da2b3a04aa6773532151d84ee147b6707a74949344d5513135155b78a467a9be2f";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SRMS";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SRMS-Users";

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    });

// ═══════════════════════════════════════════════════════════
// 6️⃣ Authorization Policies
// ═══════════════════════════════════════════════════════════
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperRootOnly", policy => policy.RequireRole(Roles.SuperRoot));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.SuperRoot, Roles.Admin));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole(Roles.SuperRoot, Roles.Admin, Roles.Manager));
    options.AddPolicy("StudentOnly", policy => policy.RequireRole(Roles.Student));

    var allPermissions = SRMS.Application.Identity.Constants.Permissions.GetAllPermissions();
    foreach (var permission in allPermissions)
    {
        options.AddPolicy(permission, policy => policy.RequireClaim("Permission", permission));
    }
});

// ═══════════════════════════════════════════════════════════
// 7️⃣ Cookie Settings
// ═══════════════════════════════════════════════════════════
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// ═══════════════════════════════════════════════════════════
// 8️⃣ CORS
// ═══════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ═══════════════════════════════════════════════════════════
// Supported cultures
// ═══════════════════════════════════════════════════════════
string[] supportedCultures = ["en-US", "ar-LY"];
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);


// ═══════════════════════════════════════════════════════════
// 9️⃣ Database Seeding
// ═══════════════════════════════════════════════════════════
var sw = new Stopwatch();
sw.Start();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        await IdentityDataSeeder.SeedAsync(userManager, roleManager);

        Console.WriteLine("✅ Database initialized successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database initialization error: {ex.Message}");
    }
}

sw.Stop();
Console.WriteLine($"⏱️ Startup took: {sw.ElapsedMilliseconds} ms");

// ═══════════════════════════════════════════════════════════
// 🔟 HTTP Pipeline
// ═══════════════════════════════════════════════════════════
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();