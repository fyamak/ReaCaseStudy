using Autofac;
using Business.Services.Security.Auth.Jwt;
using Business.Services.Security.Auth.Jwt.Interface;
using Business.Services.Security.Auth.UserPassword;
using Business.Services.Security.Auth.UserPassword.Interface;
using Business.Services.Validation;
using Business.Services.Validation.Interface;
using FluentValidation;

namespace Business;

public class BusinessModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        //Scoped

        //Singleton
        builder.RegisterType<ValidationService>().As<IValidationService>().SingleInstance();
        builder.RegisterType<JwtTokenService>().As<IJwtTokenService>().SingleInstance();
        builder.RegisterType<UserPasswordHashingService>().As<IUserPasswordHashingService>().SingleInstance();

        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(t => typeof(IValidator).IsAssignableFrom(t) && !t.IsAbstract)
            .As<IValidator>()
            .SingleInstance();
    }
}
