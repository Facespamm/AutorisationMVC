using System;
using System.Security.Claims;
using Resend;
using Autorisation.Context;
using Autorisation.Enum;
using AutorisationMVC;
using AutorisationMVC.Components;
using AutorisationMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.AddTransient<IResend>(sp =>
{
    var apiKey =
        Environment.GetEnvironmentVariable("RESEND_API_KEY");

    return ResendClient.Create(apiKey);
});
builder.Services.AddScoped<UserServices>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<RegisterServices>();
builder.Services.AddScoped<IEmailSender>(sp =>
    sp.GetRequiredService<RegisterServices>());
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;

        options.Events.OnValidatePrincipal = async context =>
        {
            var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(id, out var userId))
            {
                context.RejectPrincipal();
                return;
            }

            var db = context.HttpContext.RequestServices
                .GetRequiredService<AppDbContext>();

            var user = await db.Autorisations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || user.Status == StatusEnum.Blocked)
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<LoginUserHashCheck>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/login");
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
    await http.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);

    return Results.Redirect("/login");
});

app.MapPost("/api/auth/login",
    async (
        HttpContext http,
        HttpRequest req,
        LoginUserHashCheck check) =>
    {
        var form = await req.ReadFormAsync();

        var email = form["email"].ToString();
        var password = form["password"].ToString();

        var request = new LoginUserHashCheck.Request(email, password);

        ClaimsPrincipal result;

        try
        {
            result = await check.Handle(request);

            if (result?.Identity?.IsAuthenticated != true)
                return Results.Redirect("/login?error=1");
        }
        catch
        {
            return Results.Redirect("/login?error=1");
        }

        await http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            result);

        return Results.Redirect("/users");
    });

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();