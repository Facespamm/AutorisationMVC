using Autorisation.Context;
using Autorisation.Enum;
using Autorisation.Migrations;

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

    public string ChangeStatus(string status, int id)
    {
        if( !System.Enum.TryParse<StatusEnum>(status,true, out var parsedStatus))
        {
            return "Invalid status value.";
        }
        var user = _context.Autorisations.FirstOrDefault(x => x.Id == id);
        {
            if (user != null)
            {
                user.Status = parsedStatus;
                _context.SaveChanges();
                return  "Successfully changed status";
            }
        }
        return  "Failed to change status";
        
    }
}