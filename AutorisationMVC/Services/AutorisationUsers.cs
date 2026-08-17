using Autorisation.Context;
using Autorisation.Enum;
using Autorisation.Migrations;
using Microsoft.EntityFrameworkCore;

namespace AutorisationMVC.Services;

public class AutorisationUsers
{
    private AppDbContext _context;

    public AutorisationUsers(AppDbContext context)
    {
        _context = context;
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
                    user.Status = parsedStatus;
                }
                _context.SaveChanges();
                return  "Successfully changed status";
            
    }
}