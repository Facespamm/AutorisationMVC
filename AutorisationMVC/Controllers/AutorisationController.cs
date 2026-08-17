using Autorisation.Context;
using AutorisationMVC.Dto;
using AutorisationMVC.Mappers;
using AutorisationMVC.Services;
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

        [HttpPost]
        public async Task<IActionResult> Registration([FromBody] RegistrationDto registrationDto)
        {
            var registration = registrationDto.ToCreateRegistration();
            await _context.AddRangeAsync(registration);
            await _context.SaveChangesAsync();
            return Ok(registration);
        }

        [HttpPost("api/autorisation/login")]
        public async Task<IActionResult> Login([FromBody] string password, [FromRoute] string email)
        {
            AutorisationUsers autorisationUsers = new AutorisationUsers(_context);
            var result = autorisationUsers.Login(email, password);
            if (result == "Successfully logged in.")
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}

