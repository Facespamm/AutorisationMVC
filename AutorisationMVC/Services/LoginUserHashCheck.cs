    using System.Security.Claims;
    using Autorisation.Context;
    using AutorisationMVC.Models;

    namespace AutorisationMVC.Services;

    public sealed class LoginUserHashCheck(
        IPasswordHasher passwordHasher,
        AppDbContext appDbContext)
    {
        public record Request(string Email, string Password);

        public async Task<ClaimsPrincipal> Handle(Request request)
        {
            UserServices users = new UserServices(appDbContext);

            var user = await users.GetByEmail(request.Email);

            if (user == null)
            {
                throw new Exception($"User {request.Email} not found");
            }

            bool verified = passwordHasher.Verify(
                request.Password,
                user.password);

            if (!verified)
            {
                throw new Exception($"Wrong password");
            }

            var claims = await users.LoginWithClaims(
                request.Email,
                request.Password);

            return claims;
        }
    }