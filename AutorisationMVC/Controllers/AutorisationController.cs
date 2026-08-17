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

        [HttpPut("api/autorisation/updateStatus/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            AutorisationUsers autorisationUsers = new AutorisationUsers(_context);

            var result = autorisationUsers.ChangeStatus(status, id);
            return Ok(result);
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
        }
    }

