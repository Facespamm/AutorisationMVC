using System.Security.Claims;
using Autorisation.Context;
using Autorisation.Enum;
using Autorisation.Models;
using AutorisationMVC.Dto;
using AutorisationMVC.Mappers;
using AutorisationMVC.Models;
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
            var users = await _context.Autorisations.OrderByDescending(x=>x.LastLogin).ToListAsync();
            return users;
        }
        public async Task<string> Register(string email, string password, string name)
        {
            var emailExists = await CheckEmail(email);
            if (emailExists)
            {
                return "Email is busy";
            }

            var registerDto = new RegistrationDto
            {
                Email = email,
                password = password,
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
            public async Task <string> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return "Email and password are required.";
            }
             var user = await _context.Autorisations
            .FirstOrDefaultAsync(x => x.Email == email);
            if (user == null || user.password != password)
            {
                return "Invalid email or password.";
            }
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return "Successfully logged in.";


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
            return await _context.Autorisations.AnyAsync(x=>x.Email == email);
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
            if(user == null || user.password!= password||user.Status==StatusEnum.Blocked)
            {return  null;}
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Email,user.Email.ToString()),
                new Claim(ClaimTypes.Name,user.Name.ToString()),
            };
            var claimIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimIdentity);
        }
        public async Task<IActionResult> DeleteUsers(List<int> ids)
        {
            var user = await _context.Autorisations.Where(x=> ids.Contains(x.Id)).ToListAsync();
                    
            if (user.Count == 0)
            {
                return new NotFoundResult();
            }
            _context.Autorisations.RemoveRange(user);
            await _context.SaveChangesAsync();

            return new OkResult();
        }

        public async Task<IActionResult> SendConfirmEmail(string email, string token)
        {
            var mail = email;
            var subject = "Подтверждение регистрации";
            var message = $"Вы успешно зарегистрировались на сайте. Пожалуйста," +
                          $" подтвердите свою регистрацию, перейдя по " +
                          $"ссылке: http://localhost:5149/confirm?token={token}";
            
            await _emailSender.SendEmailAsync(mail, subject, message);
            return new OkResult();
        }
    }