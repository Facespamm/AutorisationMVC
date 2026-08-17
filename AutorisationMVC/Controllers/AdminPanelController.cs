using Autorisation.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Autorisation.Controllers
{
    public class AdminPanelController : Controller
    {
        private AppDbContext _context;

        public AdminPanelController(AppDbContext context)
        {
            _context=context;
        }

        [HttpGet("api/adminpanel/getusers")]
        public async Task<IActionResult> GetUserInformations()
        {
            var users = await _context.Autorisations
                .Select(x=>new
                {
                    x.Id,
                    x.Name,
                    x.Email,
                    x.Status,
                    x.LastLogin
                })
                .ToListAsync();
            return Ok(users);
        }
    }
}
