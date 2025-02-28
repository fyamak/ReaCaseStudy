using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Business.Mediator.Behaviours;
using Infrastructure;
using Infrastructure.Data.Postgres.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Web.Extensions;


namespace Business.Test.RequestHandlers;

public abstract class BaseHandlerTest
{
    protected readonly ContainerBuilder ContainerBuilder;
    protected          IMediator        Mediator;
    protected          PostgresContext  PostgresContext;
    protected          IContainer       Container;

    protected BaseHandlerTest()
    {
        var services = new ServiceCollection();

        var builder = new ContainerBuilder();

        // Transient when testing to avoid collisions on tracking
        services.AddDbContext<PostgresContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()), ServiceLifetime.Transient);

        var log = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        services.AddSingleton<ILogger>(log);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(BusinessModule).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(InfrastructureModule).Assembly);

            // Behaviours
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        var inMemoryConfig = new Dictionary<string, string>
        {
            { "Jwt:SecretKey", "test_secret_key" },
            { "Jwt:Issuer", "test_issuer" },
            { "Jwt:Audience", "test_audience" },
            { "Jwt:SecurityKey", "56ew4r65asdf48dsge824389vvbcxhgf8ReaLayeredV28bvINTEGRATIONTESTS" },
            { "Smtp:Host", "smtp.test.com" },
            { "Smtp:Port", "25" },
            { "Smtp:User", "test_user" },
            { "Smtp:Password", "test" },
            { "FrontApp:BaseUrl", "http://localhost" },
            { "App:Environment", "IntegrationTest" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig!)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.CreateOptions(configuration);

        builder.Populate(services);
        builder.RegisterModule<BusinessModule>();
        builder.RegisterModule<InfrastructureModule>();

        ContainerBuilder = builder;
    }

    protected void BuildContainer()
    {
        var container = ContainerBuilder.Build();

        Mediator        = container.Resolve<IMediator>();
        PostgresContext = container.Resolve<PostgresContext>();
        Container       = container;
    }
}
