using Autorisation.Context;
using Autorisation.Enum;
using AutorisationMVC.Dto;
using AutorisationMVC.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resend;

namespace AutorisationMVC.Services;

public class RegisterServices : IEmailSender
{
    private readonly AppDbContext _context;
    private readonly IResend _resend;
    private readonly IConfiguration _configuration;

    public RegisterServices(
        AppDbContext context,
        IResend resend,
        IConfiguration configuration)
    {
        _context = context;
        _resend = resend;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(
        string email,
        string subject,
        string message)
    {
        Console.WriteLine($"Sending email to: {email}");

        var fromAddress = _configuration["Resend:FromEmail"] ?? "noreply@xyzs.click";

        await _resend.EmailSendAsync(
            new EmailMessage
            {
                From = fromAddress,
                To = email.Trim(),
                Subject = subject,
                TextBody = message
            });

        Console.WriteLine($"Email sent to: {email}");
    }
    public async Task<IActionResult> SendConfirmEmail(
        string email,
        string token)
    {
        var subject = "Подтверждение регистрации";

        var appUrl = "https://autorisationmvc.onrender.com";

        var message =
            "Вы успешно зарегистрировались на сайте.\n\n" +
            "Пожалуйста, подтвердите свою регистрацию, " +
            "перейдя по ссылке:\n\n" +
            $"{appUrl}/confirm?token={Uri.EscapeDataString(token)}";

        await SendEmailAsync(
            email,
            subject,
            message);

        return new OkResult();
    }
    public async Task<string> Register(
        string email,
        string password,
        string name)
    {
        var emailExists = await CheckEmail(email);

        if (emailExists)
        {
            return "Email is busy";
        }

        var hasher = new PasswordHasher();

        var registerDto = new RegistrationDto
        {
            Email = email.Trim(),
            password = hasher.HashPassword(password),
            Name = name,
            ConfirmationToken = Guid.NewGuid().ToString(),
            Status = StatusEnum.Unverified
        };

        var newUser = registerDto.ToCreateRegistration();

        await _context.AddAsync(newUser);
        await _context.SaveChangesAsync();

        try
        {
            await SendConfirmEmail(
                newUser.Email,
                newUser.ConfirmationToken);

            return "Successfully registered.";
        }
        catch (Exception ex)
        {
            Console.WriteLine("=== RESEND ERROR ===");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("====================");

            return "Ошибка отправки письма: " + ex.Message;
        }
    }
    public async Task<string> ConfirmToken(string token)
    {
        var result = await _context.Autorisations
            .FirstOrDefaultAsync(
                x => x.ConfirmationToken == token);

        if (result == null)
        {
            return "Invalid or expired confirmation link.";
        }

        if (result.Status == StatusEnum.Unverified)
        {
            result.Status = StatusEnum.Active;
            result.ConfirmationToken = null;

            await _context.SaveChangesAsync();
        }

        return "Email confirmed successfully.";
    }

    public async Task<bool> CheckEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return await _context.Autorisations
            .AnyAsync(x => x.Email == email);
    }
}