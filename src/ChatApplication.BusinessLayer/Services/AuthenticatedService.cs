using AutoMapper;
using ChatApplication.Authentication.Entities;
using ChatApplication.BusinessLayer.Contracts;
using ChatApplication.BusinessLayer.Services.Interfaces;
using ChatApplication.Shared.Models;
using ChatApplication.StorageProviders;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace ChatApplication.BusinessLayer.Services;

public class AuthenticatedService : IAuthenticatedService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IUserService userService;
    private readonly IStorageProvider storageProvider;
    private readonly IMapper mapper;

    public AuthenticatedService(UserManager<ApplicationUser> userManager, IUserService userService,
        IStorageProvider storageProvider, IMapper mapper)
    {
        this.userManager = userManager;
        this.userService = userService;
        this.storageProvider = storageProvider;
        this.mapper = mapper;
    }

    public async Task<Result<User>> GetAsync()
    {
        var dbUser = await userManager.FindByNameAsync(userService.GetUserName());
        if (dbUser == null)
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        var user = mapper.Map<User>(dbUser);
        return user;
    }
}