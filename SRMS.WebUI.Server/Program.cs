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
builder.Services.AddRazorComponents();
//================my code====================//
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


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

app.Run();