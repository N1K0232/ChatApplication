using ChatApplication.BusinessLayer.Sendinblue;
using FluentEmail.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApplication.BusinessLayer.Extensions;

public static class ServiceCollectionExtensions
{
    public static FluentEmailServicesBuilder AddSendinblueSender(this FluentEmailServicesBuilder builder)
    {
        builder.Services.AddSingleton<ISender, SendinblueSender>();
        return builder;
    }
}