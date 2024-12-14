using AutoMapper;
using ChatApplication.Authentication.Entities;
using ChatApplication.Shared.Models;
using ChatApplication.Shared.Models.Requests;

namespace ChatApplication.BusinessLayer.Mapping;

public class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<RegisterRequest, ApplicationUser>();
        CreateMap<ApplicationUser, User>();
    }
}