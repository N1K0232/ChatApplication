using ChatApplication.Authentication.Extensions;
using ChatApplication.BusinessLayer.Contracts;

namespace ChatApplication;

public class HttpUserService : IUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public HttpUserService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public Guid GetId() => httpContextAccessor.HttpContext.User.GetId();

    public string GetUserName() => httpContextAccessor.HttpContext.User.GetUserName();
}