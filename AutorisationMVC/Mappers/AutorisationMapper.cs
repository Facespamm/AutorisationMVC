using Autorisation.Enum;
using Autorisation.Migrations;
using Autorisation.Models;
using AutorisationMVC.Dto;

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