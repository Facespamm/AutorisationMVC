using Autorisation.Enum;

namespace AutorisationMVC.Models
{
    public class Autorisations
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public string password { get; set; }

        public string Email { get; set; }
        
        public string? ConfirmationToken   { get; set; }

        public StatusEnum Status { get; set; }

        public DateTime LastLogin { get; set; }
    }
}
