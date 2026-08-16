using Autorisation.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Autorisation.Controllers
{
    public class AutorisationControllerP : Controller
    {
        private AppDbContext _context;

        public AutorisationControllerP(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetStatus")]
        public async Task<ActionResult> GetStatus()
        {
            var status = await _context.Autorisations.ToListAsync();
            return Ok();
        }
    }
}
