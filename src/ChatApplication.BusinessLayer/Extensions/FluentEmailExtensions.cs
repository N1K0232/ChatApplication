using FluentEmail.Core.Models;

namespace ChatApplication.BusinessLayer.Extensions;

public static class FluentEmailExtensions
{
    public static string GetErrors(this SendResponse response)
        => string.Join(",", response.ErrorMessages);
}