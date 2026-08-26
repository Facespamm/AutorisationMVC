using System.Net;
using System.Net.Mail;
using Autorisation.Context;
using Autorisation.Enum;
using AutorisationMVC.Dto;
using AutorisationMVC.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutorisationMVC.Services;

public class RegisterServices:IEmailSender
{
    private AppDbContext _context;
    public RegisterServices(AppDbContext context)
    {
        _context = context;
    }
    public Task SendEmailAsync(string email, string subject, string message)
    {
        var mail = "aerfceje@gmail.com";
        var password = "cxis hajo mviy yhwa";
        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(mail, password),
        };
        return client.SendMailAsync(new MailMessage(from:mail,to:email,subject,message));
    }
    public async Task<IActionResult> SendConfirmEmail(string email, string token)
    {
        var mail = email;
        var subject = "Подтверждение регистрации";
        var message = $"Вы успешно зарегистрировались на сайте. Пожалуйста," +
                      $" подтвердите свою регистрацию, перейдя по " +
                      $"ссылке: http://localhost:5149/confirm?token={token}";
        await SendEmailAsync(mail, subject, message);
        return new OkResult();
    }
    public async Task<string> Register(string email, string password, string name)
    {
        var emailExists = await CheckEmail(email);
        if (emailExists)
        {
            return "Email is busy";
        }
        PasswordHasher hasher = new PasswordHasher();
        var registerDto = new RegistrationDto
        {
            Email = email,
            password = hasher.HashPassword(password),
            Name = name,
            ConfirmationToken = Guid.NewGuid().ToString(),
            Status = StatusEnum.Unverified
        };
        var newUser = registerDto.ToCreateRegistration();
        await _context.AddAsync(newUser);
        await _context.SaveChangesAsync();

        await SendConfirmEmail(newUser.Email, newUser.ConfirmationToken);
        return "Successfully registered.";
    }
    public async Task<string> ConfirmToken(string token)
    {
        var result = _context.Autorisations.FirstOrDefault(x => x.ConfirmationToken == token);
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
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }
        return await _context.Autorisations.AnyAsync(x => x.Email == email);
    }


}
