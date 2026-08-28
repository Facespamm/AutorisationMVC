using System.Security.Claims;
using Autorisation.Context;
using Autorisation.Enum;
using AutorisationMVC.Dto;
using AutorisationMVC.Mappers;
using AutorisationMVC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutorisationMVC.Services;

public class UserServices
{
    private AppDbContext _context;

    public UserServices(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Autorisations>> GetUsers()
    {
        var users = await _context.Autorisations.OrderByDescending(x => x.LastLogin).ToListAsync();
        return users;
    }

    public async Task<bool> IsUserActiveAsync(int id)
    {
        var user = await _context.Autorisations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        return user != null && user.Status != StatusEnum.Blocked;
    }

    public async Task<Autorisations> GetByEmail(string email)
    {
        var user = await _context.Autorisations.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null)
        {
            return null;
        }
        return user;
    }

    public async Task<(string Message, bool AffectsSelf)> ChangeStatus(string status, List<int> ids, int currentId)
    {
        // note: защита на уровне сервиса — если сам вызывающий УЖЕ был заблокирован
        // до этого действия (например, куки ещё не успели инвалидироваться), отказываем сразу
        if (!await IsUserActiveAsync(currentId))
        {
            return ("You have been blocked.", true);
        }

        if (!System.Enum.TryParse<StatusEnum>(status, true, out var parsedStatus))
        {
            return ("Invalid status value.", false);
        }

        var users = await _context.Autorisations.Where(x => ids.Contains(x.Id)).ToListAsync();
        if (users.Count == 0)
        {
            return ("No users found.", false);
        }

        foreach (var user in users)
        {
            user.Status = parsedStatus;
        }
        await _context.SaveChangesAsync();

        // note: заодно проверяем — не заблокировал ли пользователь СЕЙЧАС самого себя
        var affectsSelf = parsedStatus == StatusEnum.Blocked && ids.Contains(currentId);

        return ("Successfully changed status", affectsSelf);
    }

    public async Task<(string Message, bool AffectsSelf)> DeleteUnverified(int currentId)
    {
        if (!await IsUserActiveAsync(currentId))
        {
            return ("You have been blocked.", true);
        }

        var del = await _context.Autorisations.Where(x => x.Status == StatusEnum.Unverified).ToListAsync();
        if (del.Count == 0)
        {
            return ("No unverified users found.", false);
        }

        var affectsSelf = del.Any(u => u.Id == currentId);

        _context.Autorisations.RemoveRange(del);
        await _context.SaveChangesAsync();

        return ("Successfully deleted unverified user.", affectsSelf);
    }

    public async Task<ClaimsPrincipal> LoginWithClaims(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return null;
        }
        var user = await _context.Autorisations.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null || user.Status == StatusEnum.Blocked)
        {
            return null;
        }
        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email.ToString()),
            new Claim(ClaimTypes.Name, user.Name.ToString()),
        };
        var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(claimIdentity);
    }

    public async Task<(IActionResult Result, bool AffectsSelf)> DeleteUsers(List<int> ids, int currentId)
    {
        if (!await IsUserActiveAsync(currentId))
        {
            return (new ForbidResult(), true);
        }

        var user = await _context.Autorisations.Where(x => ids.Contains(x.Id)).ToListAsync();

        if (user.Count == 0)
        {
            return (new NotFoundResult(), false);
        }

        var affectsSelf = ids.Contains(currentId);

        _context.Autorisations.RemoveRange(user);
        await _context.SaveChangesAsync();

        return (new OkResult(), affectsSelf);
    }
}