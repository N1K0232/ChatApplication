using AutoMapper;
using ChatApplication.Authentication.Entities;
using ChatApplication.Shared.Models;

namespace ChatApplication.BusinessLayer.MapperProfiles;

public class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<ApplicationUser, User>();
    }
}