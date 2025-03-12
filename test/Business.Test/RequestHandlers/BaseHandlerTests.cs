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
    protected readonly ContainerBuilder ContainerBuilder; //  Autofac ile bağımlılıkları kaydetmek için kullanılır.
    protected          IMediator        Mediator; // istekleri göndermek için kullanılıyor
    protected          PostgresContext  PostgresContext; // InMemory DB kullanarak testler için veritabanı işlemlerini yürütmek için kullanılır.
    protected          IContainer       Container; // Autofac bağımlılıklarını içeren ana kapsayıcıdır.

    protected BaseHandlerTest()
    {
        var services = new ServiceCollection(); // .NET'in IServiceCollection yapısını kullanarak bağımlılıkları eklemek için oluşturulur

        var builder = new ContainerBuilder(); // Autofac bağımlılıklarını kaydetmek için oluşturulur.

        // Transient when testing to avoid collisions on tracking
        services.AddDbContext<PostgresContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()), ServiceLifetime.Transient);

        var log = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        services.AddSingleton<ILogger>(log);

        services.AddMediatR(cfg =>
        {
            // MediatR kullanımı, testlerde CQRS deseninin uygulanmasını sağlar.
            cfg.RegisterServicesFromAssembly(typeof(BusinessModule).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(InfrastructureModule).Assembly);

            // Behaviours
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>)); // MediatR yapılandırılıyor ve ValidationBehavior middleware olarak ekleniyor.
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
        builder.RegisterModule<BusinessModule>(); // BusinessModule bağımlılıkları sisteme kaydediliyor.
        builder.RegisterModule<InfrastructureModule>(); // InfrastructureModule bağımlılıkları sisteme kaydediliyor.

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
