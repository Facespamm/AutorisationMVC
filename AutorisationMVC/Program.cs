using System;
using Autorisation.Context;
using AutorisationMVC;
using AutorisationMVC.Components;
using AutorisationMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IEmailSender, EmailSender>(); 
builder.Services.AddScoped<AutorisationUsers>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
 
app.MapPost("/api/auth/logout", async (HttpContext http) =>
{
     await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
     return Results.Redirect("/login");
});
app.MapPost("/api/auth/login", async (HttpContext http, AutorisationUsers user, HttpRequest req) =>
{
    var form = await req.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    
    var result = await user.LoginWithClaims(email, password);
    
    if (result == null)
    {
        return Results.Redirect("/login?error=1");
    }
    
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result);
    return Results.Redirect("/users");
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();
