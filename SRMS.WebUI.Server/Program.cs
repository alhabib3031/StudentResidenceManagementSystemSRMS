// SRMS.WebUI.Server/Program.cs
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
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

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════
// 1️⃣ MudBlazor Services
// ═══════════════════════════════════════════════════════════
// 🎨 MudBlazor = مكتبة UI Components
builder.Services.AddMudServices();

// ═══════════════════════════════════════════════════════════
// 2️⃣ Razor Components + Interactive Server
// ═══════════════════════════════════════════════════════════
// 🖥️ لتشغيل Blazor Server-Side
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllersWithViews(); // للـ API Controllers

// ═══════════════════════════════════════════════════════════
// 3️⃣ Application & Infrastructure Layers
// ═══════════════════════════════════════════════════════════
// 📦 تسجيل خدمات الـ Application (MediatR, Mapster, etc.)
builder.Services.AddApplication();

// 🗄️ تسجيل خدمات الـ Infrastructure (DbContext, Repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// ═══════════════════════════════════════════════════════════
// 4️⃣ Identity Configuration ← المهم جداً!
// ═══════════════════════════════════════════════════════════
// 👤 تسجيل Identity System (User Management)
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // 🔒 Password Requirements
    options.Password.RequireDigit = true;              // يجب رقم واحد على الأقل
    options.Password.RequireLowercase = true;          // يجب حرف صغير
    options.Password.RequireUppercase = true;          // يجب حرف كبير
    options.Password.RequireNonAlphanumeric = true;    // يجب رمز خاص (!@#$%)
    options.Password.RequiredLength = 8;               // طول 8 أحرف على الأقل
    
    // 🔐 Lockout Settings (قفل الحساب بعد محاولات فاشلة)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);  // مدة القفل
    options.Lockout.MaxFailedAccessAttempts = 5;                       // عدد المحاولات
    options.Lockout.AllowedForNewUsers = true;                         // تفعيل للمستخدمين الجدد
    
    // ✉️ User Settings
    options.User.RequireUniqueEmail = true;           // كل email فريد
    options.SignIn.RequireConfirmedEmail = true;      // يجب تأكيد الإيميل
})
.AddEntityFrameworkStores<ApplicationDbContext>()     // ربط Identity بـ EF Core
.AddDefaultTokenProviders();                          // لتوليد Tokens (Reset Password, etc.)

// builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

// ═══════════════════════════════════════════════════════════
// 5️⃣ JWT Authentication ← للـ API
// ═══════════════════════════════════════════════════════════
// 🔑 JWT = JSON Web Token (للمصادقة عبر API)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere123456789!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SRMS";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SRMS-Users";

builder.Services.AddAuthentication(options =>
{
    // 🎯 Default Authentication Scheme = JWT
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;                          // حفظ الـ Token
    options.RequireHttpsMetadata = false;              // السماح بـ HTTP (للتطوير)
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,                         // التحقق من المُصدر
        ValidateAudience = true,                       // التحقق من الجمهور
        ValidateLifetime = true,                       // التحقق من الصلاحية
        ValidateIssuerSigningKey = true,               // التحقق من المفتاح
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(   // المفتاح السري
            Encoding.UTF8.GetBytes(jwtKey)
        ),
        ClockSkew = TimeSpan.Zero                      // عدم السماح بتأخير
    };
});

// ═══════════════════════════════════════════════════════════
// 6️⃣ Authorization Policies ← هذا الحل للمشكلة! 🎯
// ═══════════════════════════════════════════════════════════
// 🛡️ Policies = سياسات الصلاحيات
builder.Services.AddAuthorizationBuilder()
    // 👑 SuperRoot فقط
    .AddPolicy("SuperRootOnly", policy => 
        policy.RequireRole(Roles.SuperRoot))
    
    // 👔 Admin أو SuperRoot
    .AddPolicy("AdminOnly", policy => 
        policy.RequireRole(Roles.SuperRoot, Roles.Admin))
    
    // 🏢 Manager أو أعلى
    .AddPolicy("ManagerOnly", policy => 
        policy.RequireRole(Roles.SuperRoot, Roles.Admin, Roles.Manager))
    
    // 🎓 Student فقط
    .AddPolicy("StudentOnly", policy => 
        policy.RequireRole(Roles.Student));

// ═══════════════════════════════════════════════════════════
// 7️⃣ Cookie Settings (للـ Blazor Authentication)
// ═══════════════════════════════════════════════════════════
// 🍪 إعدادات الـ Cookie للمصادقة في Blazor
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";                      // صفحة تسجيل الدخول
    options.LogoutPath = "/logout";                    // صفحة تسجيل الخروج
    options.AccessDeniedPath = "/access-denied";       // صفحة الوصول المرفوض
    options.ExpireTimeSpan = TimeSpan.FromDays(7);     // مدة صلاحية الـ Cookie
    options.SlidingExpiration = true;                  // تجديد تلقائي
});

// ═══════════════════════════════════════════════════════════
// 8️⃣ CORS (للسماح بـ API Requests من نطاقات أخرى)
// ═══════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()                        // أي نطاق
              .AllowAnyMethod()                        // أي Method (GET, POST, etc.)
              .AllowAnyHeader();                       // أي Header
    });
});

var app = builder.Build();

// ═══════════════════════════════════════════════════════════
// 9️⃣ Database Connection Test + Seeding
// ═══════════════════════════════════════════════════════════
var sw = new Stopwatch();
sw.Start();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        // ✅ التأكد من إنشاء قاعدة البيانات
        await context.Database.EnsureCreatedAsync();
        
        // 🧪 Test Query
        await context.Students.FirstOrDefaultAsync();
        
        // 🌱 Seed Identity Data (إنشاء Roles + SuperRoot)
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        await IdentityDataSeeder.SeedAsync(userManager, roleManager);
        
        Console.WriteLine("✅ Database connection successful!");
        Console.WriteLine("✅ SuperRoot created: superroot@srms.edu.ly / SuperRoot@123!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database error: {ex.Message}");
    }
}

sw.Stop();
Console.WriteLine($"⏱️ Startup DB Query took: {sw.ElapsedMilliseconds} ms");

// ═══════════════════════════════════════════════════════════
// 🔟 HTTP Request Pipeline (ترتيب مهم جداً!)
// ═══════════════════════════════════════════════════════════
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();  // تفعيل HTTPS Strict Transport Security
}

app.UseHttpsRedirection();   // تحويل HTTP إلى HTTPS
app.UseStaticFiles();         // السماح بالملفات الثابتة (CSS, JS, Images)
app.UseRouting();             // تفعيل Routing

// ⚠️ الترتيب مهم جداً!
app.UseCors();                // ✅ CORS أولاً
app.UseAuthentication();      // ✅ ثم Authentication (من أنت؟)
app.UseAuthorization();       // ✅ ثم Authorization (ماذا يمكنك أن تفعل؟)

app.UseAntiforgery();         // حماية من CSRF Attacks
app.MapStaticAssets();

// 🎯 Mapping Razor Components + API
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();         // للـ API Controllers

app.Run();