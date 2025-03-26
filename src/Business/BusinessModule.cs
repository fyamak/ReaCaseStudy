using Autofac;
using Business.Services.Security.Auth.Jwt;
using Business.Services.Security.Auth.Jwt.Interface;
using Business.Services.Security.Auth.UserPassword;
using Business.Services.Security.Auth.UserPassword.Interface;
using Business.Services.Validation;
using Business.Services.Validation.Interface;
using FluentValidation;
using Business.Services.Kafka;
using Business.Services.Kafka.Interface;
using Microsoft.Extensions.Hosting;
using Business.Services.Background.Kafka;


namespace Business;

public class BusinessModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ValidationService>().As<IValidationService>().SingleInstance();
        builder.RegisterType<JwtTokenService>().As<IJwtTokenService>().SingleInstance();
        builder.RegisterType<UserPasswordHashingService>().As<IUserPasswordHashingService>().SingleInstance();

        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(t => typeof(IValidator).IsAssignableFrom(t) && !t.IsAbstract)
            .As<IValidator>()
            .SingleInstance();

        
        builder.RegisterType<KafkaProducer>().As<IKafkaProducer>().InstancePerDependency(); ;
        builder.RegisterType<KafkaConsumer>().As<IKafkaConsumer>().InstancePerDependency(); ;

        builder.RegisterType<CreateProductConsumer>().As<IHostedService>().InstancePerDependency(); ;
        builder.RegisterType<AddSupplyConsumer>().As<IHostedService>().InstancePerDependency(); ;
        builder.RegisterType<AddSaleConsumer>().As<IHostedService>().InstancePerDependency(); ;

        //builder.RegisterType<Consumer>().As<IHostedService>().SingleInstance();

    }
}
