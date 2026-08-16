using Autorisation.Enum;

namespace Autorisation.Models
{
    public class Autorisations
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string password { get; set; }

        public string Email { get; set; }

        public Status Status { get; set; }

        public DateTime LastLogin { get; set; }
    }
}
