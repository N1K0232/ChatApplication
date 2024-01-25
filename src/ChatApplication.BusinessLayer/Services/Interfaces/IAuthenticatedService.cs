using ChatApplication.Shared.Models;
using OperationResults;

namespace ChatApplication.BusinessLayer.Services.Interfaces;
public interface IAuthenticatedService
{
    Task<Result<User>> GetAsync();
}