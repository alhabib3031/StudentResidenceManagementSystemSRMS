using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using SRMS.Application;
using SRMS.Infrastructure;
using SRMS.WebUI.Server.Components;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//================my code====================//
builder.Services.AddControllersWithViews();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

//================this code to test db connection and query time====================//
var sw = new Stopwatch();
sw.Start();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Students.FirstOrDefaultAsync();
}

sw.Stop();
Console.WriteLine($"⏱️ Startup DB Query took: {sw.ElapsedMilliseconds} ms");


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

//=================my code====================//
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();

//================تعديل لاحقا====================//
// async Task SeedRolesAsync(IServiceProvider serviceProvider)
// {
//     var roleManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
//     
//     string[] roles = { "Admin", "User", "Teacher", "Student" };
//     
//     foreach (var role in roles)
//     {
//         if (!await roleManager.RoleExistsAsync(role))
//         {
//             await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(role));
//             Console.WriteLine($"✅ Role '{role}' created");
//         }
//     }
// }