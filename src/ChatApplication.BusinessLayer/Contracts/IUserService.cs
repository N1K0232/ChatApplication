namespace ChatApplication.BusinessLayer.Contracts;

public interface IUserService
{
    Guid GetId();

    string GetUserName();
}