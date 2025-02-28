using Autofac;
using Infrastructure.Data.Postgres;
using Infrastructure.Mail;
using Infrastructure.Mail.Interface;

namespace Infrastructure;

public class InfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        //Scoped
        builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();

        //Singleton
        builder.RegisterType<MailService>().As<IMailService>().SingleInstance();
    }
}
