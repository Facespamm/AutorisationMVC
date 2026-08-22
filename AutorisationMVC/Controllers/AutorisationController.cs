using Autorisation.Context;
using AutorisationMVC;
using AutorisationMVC.Dto;
using AutorisationMVC.Mappers;
using AutorisationMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutorisationMVC.Controllers
{
    public class AutorisationController : Controller
    {
        private AppDbContext _context;
        private IEmailSender _emailSender;

        public AutorisationController(AppDbContext context,IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> Registration([FromBody] RegistrationDto registrationDto)
        {
            var registration = registrationDto.ToCreateRegistration();
            await _context.AddRangeAsync(registration);
            await _context.SaveChangesAsync();
            var token = Guid.NewGuid().ToString();
            IEmailSender emailSender = _emailSender;
            AutorisationUsers autorisationUsers = new AutorisationUsers(_context,_emailSender);
          await autorisationUsers.SendConfirmEmail(registration.Email, token);

            return Ok(registration);
        }

        [HttpPost("api/autorisation/login")]
        public async Task<IActionResult> Login([FromBody] string password, [FromRoute] string email)
        {
            AutorisationUsers autorisationUsers = new AutorisationUsers(_context,_emailSender);
            var result = await autorisationUsers.Login(email, password);
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

