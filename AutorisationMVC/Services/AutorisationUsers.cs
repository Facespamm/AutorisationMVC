using Autorisation.Context;
using Autorisation.Enum;
using Autorisation.Migrations;
using AutorisationMVC.Dto;
using AutorisationMVC.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutorisationMVC.Services;

public class AutorisationUsers
{
    private AppDbContext _context;
    private readonly IEmailSender _emailSender;

    public AutorisationUsers(AppDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;

    }

    public async Task<string> Register(string email, string password, string name)
    {
        RegistrationDto registerDto = new RegistrationDto();

        registerDto = new()
        {
            Email = email,
            password = password,
            Name = name,
            ConfirmationToken =  Guid.NewGuid().ToString(),
            Status = StatusEnum.Unverified
            
        };
        var newUser = registerDto.ToCreateRegistration();
        await _context.AddAsync(newUser);
        await _context.SaveChangesAsync();
        var send = SendConfirmEmail(newUser.Email, newUser.ConfirmationToken);
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
public string Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return "Email and password are required.";
        }

        var user = _context.Autorisations.FirstOrDefault(x => x.Email == email);

        if (user == null || user.password != password)
        {
            return "Invalid email or password.";
        }
        else
        {
            return "Successfully logged in.";
        }

    }

    public async Task<string> ChangeStatus(string status, List<int> ids)
    {
        if( !System.Enum.TryParse<StatusEnum>(status,true, out var parsedStatus))
        {
            return "Invalid status value.";
        }
        var users = await _context.Autorisations.Where(x => ids.Contains(x.Id)).ToListAsync();
                if (users.Count == 0)
                {
                    return "No users found.";
                }
                foreach (var user in users)
                {
                    user.Status = parsedStatus;
                }
                _context.SaveChangesAsync();
                return  "Successfully changed status";
    }

    public async Task<IActionResult> SendConfirmEmail(string email, string token)
    {
        var mail = email;
        var subject = "Подтверждение регистрации";
        var message = $"Вы успешно зарегистрировались на сайте. Пожалуйста," +
                      $" подтвердите свою регистрацию, перейдя по " +
                      $"ссылке: https://твой-домен/api/auth/confirm?token={token}";
        
        await _emailSender.SendEmailAsync(mail, subject, message);
        return new OkResult();
    }
}