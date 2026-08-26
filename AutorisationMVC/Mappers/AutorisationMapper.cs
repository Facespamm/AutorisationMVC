using Autorisation.Enum;
using AutorisationMVC.Dto;
using AutorisationMVC.Models;

namespace AutorisationMVC.Mappers;

public static class AutorisationMapper
{
    public static Autorisations ToCreateRegistration(this RegistrationDto requestDto)
    {
        return new()
        {
            Name = requestDto.Name,
            password = requestDto.password,
            Email = requestDto.Email,
            Status = requestDto.Status,
            ConfirmationToken =  requestDto.ConfirmationToken   
        };
    }
}