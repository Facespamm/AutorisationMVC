using System.Security.Claims;
using Autorisation.Context;
using Autorisation.Enum;
using Autorisation.Models;
using AutorisationMVC.Dto;
using AutorisationMVC.Mappers;
using Microsoft.AspNetCore.Authentication.Cookies;
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

    public async Task<List<Autorisations>> GetUsers()
    {
        var users = await _context.Autorisations
            .ToListAsync();
        return users;
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
                if (users.Count == 0) { return "No users found."; }
                foreach (var user in users)
                {
                    user.Status = parsedStatus;
                }
               await _context.SaveChangesAsync();
                return  "Successfully changed status";
    }

    public async Task<bool> CheckEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }
        var check = await _context.Autorisations
            
            .AnyAsync(x=>x.Email == email);
       
        return await _context.Autorisations
            
            .AnyAsync(x=>x.Email == email);
    }

    public async Task<string> DeleteUnverified()
    {
        var del =await _context.Autorisations.Where(x=>x.Status == StatusEnum.Unverified).ToListAsync();
        if (del.Count == 0)
        {
            return "No unverified users found.";
        }
        _context.Autorisations.RemoveRange(del);
        await _context.SaveChangesAsync();
        
        return  "Successfully deleted unverified user.";
    }

    public async Task<ClaimsPrincipal> LoginWithClaims(string email, string password)
    {
        if(string.IsNullOrEmpty(email)|| string.IsNullOrEmpty(password)){return null;}
        var user = await _context.Autorisations.FirstOrDefaultAsync(x => x.Email == email);
        if(user == null || user.password!= password||user.Status==StatusEnum.Blocked || user.Status == StatusEnum.Unverified)
        {return  null;}
            
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim(ClaimTypes.Email,user.Email.ToString()),
            new Claim(ClaimTypes.Name,user.Name.ToString()),
        };
        
        var claimIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(claimIdentity);
        
    }

    public async Task<IActionResult> SendConfirmEmail(string email, string token)
    {
        var mail = email;
        var subject = "Подтверждение регистрации";
        var message = $"Вы успешно зарегистрировались на сайте. Пожалуйста," +
                      $" подтвердите свою регистрацию, перейдя по " +
                      $"ссылке: https://localhost:5149/api/auth/confirm?token={token}";
        
        await _emailSender.SendEmailAsync(mail, subject, message);
        return new OkResult();
    }
}