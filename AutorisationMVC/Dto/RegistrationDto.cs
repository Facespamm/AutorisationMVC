using Autorisation.Enum;

namespace AutorisationMVC.Dto;

public class RegistrationDto
{
    public string Name { get; set; }

    public string password { get; set; } 

    public string Email { get; set; }
    
    public string ConfirmationToken  { get; set; }

    public StatusEnum Status { get; set; }
}