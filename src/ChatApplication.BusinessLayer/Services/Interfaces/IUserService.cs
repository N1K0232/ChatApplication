using ChatApplication.Shared.Models;
using OperationResults;

namespace ChatApplication.BusinessLayer.Services.Interfaces;

public interface IUserService
{
    Task<Result<User>> GetAsync();
}