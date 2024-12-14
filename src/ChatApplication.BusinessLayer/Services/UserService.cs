using AutoMapper;
using ChatApplication.Authentication.Entities;
using ChatApplication.BusinessLayer.Services.Interfaces;
using ChatApplication.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace ChatApplication.BusinessLayer.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IMapper mapper;

    public UserService(UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        this.userManager = userManager;
        this.httpContextAccessor = httpContextAccessor;
        this.mapper = mapper;
    }

    public async Task<Result<User>> GetAsync()
    {
        var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
        return mapper.Map<User>(user);
    }
}