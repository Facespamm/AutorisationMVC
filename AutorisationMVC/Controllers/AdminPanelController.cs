using Autorisation.Context;
using AutorisationMVC.Services;
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
        [HttpDelete("api/autorisation/deleteUsers/")]
        public async Task<IActionResult> DeleteUsers(List<int> ids)
        {
            var user = await _context.Autorisations.Where(x=> ids.Contains(x.Id)).ToListAsync();
                
            if (user.Count == 0)
            {
                return NotFound();
            }
            _context.Autorisations.RemoveRange(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPut("api/autorisation/updateStatus/")]
        public async Task<IActionResult> UpdateStatus(List<int> ids, [FromBody] string status)
        {
            AutorisationUsers autorisationUsers = new AutorisationUsers(_context);

            var result = autorisationUsers.ChangeStatus(status, ids);
            return Ok(result);
        }
    }
}
