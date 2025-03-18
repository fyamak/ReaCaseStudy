using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Business;
using Business.Mediator.Behaviours;
using Business.Services.Security.Auth.Jwt;
using Business.Services.Security.Auth.Jwt.Interface;
using Infrastructure;
using Infrastructure.Data.Postgres.EntityFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Web;
using Web.Extensions;
using Web.Middlewares;
using static Business.RequestHandlers.Product.AddSales;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var environment = configuration["App:EnvironmentAlias"] ?? "DEV";


builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", cBuilder =>
{
    cBuilder.WithOrigins("http://localhost:3000") 
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
}));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v01",
        new OpenApiInfo { Title = "Rea Layered Template V2", Version = "v0.1" });

    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name        = "Authorization",
            Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
            In          = ParameterLocation.Header,
            Type        = SecuritySchemeType.ApiKey,
            Scheme      = "Bearer"
        });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "Bearer", In = ParameterLocation.Header, Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
            },
            new List<string>()
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();
builder.Services.CreateOptions(builder.Configuration);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.Register(ctx =>
    {
        var httpContextAccessor = ctx.Resolve<IHttpContextAccessor>();
        var httpContext         = httpContextAccessor.HttpContext;
        var claims              = httpContext?.User.Claims.ToArray() ?? [];
        return new UserContext(claims);
    }).As<IUserContext>().InstancePerLifetimeScope();
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(WebModule).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(BusinessModule).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(InfrastructureModule).Assembly);

    // Behaviours
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});


var postgresConnectionString = builder.Configuration.GetConnectionString("PsqlConnection");

builder.Services.AddDbContext<PostgresContext>(options =>
    options.UseNpgsql(postgresConnectionString, x => x.MigrationsAssembly("Infrastructure"))
        .EnableSensitiveDataLogging().LogTo(Console.WriteLine, LogLevel.Information));

if (environment == "DEV")
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.Console()
        .CreateLogger();
}
else
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Enrich.WithProperty("Application", "ReaLayeredTemplateV2")
        .CreateLogger();
}


builder.Logging.AddSerilog();
builder.Host.UseSerilog();
builder.Services.AddSingleton(Log.Logger);

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new WebModule());
    containerBuilder.RegisterModule(new BusinessModule());
    containerBuilder.RegisterModule(new InfrastructureModule());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience         = true,
        ValidateIssuer           = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecurityKey"]!)),
        ClockSkew                = TimeSpan.Zero
    };
    /*
     For SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/chatHub")))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
    */
});

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseCors("AllowSpecificOrigin");


app.UseCors("CorsPolicy");

app.UseMiddleware<Rea404Middleware>();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseRouting();

app.UseHttpsRedirection();

if (environment != "PROD")
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.RoutePrefix = "swagger";
        o.SwaggerEndpoint("/swagger/v01/swagger.json", "Rea Layered V2");
    });

    app.UseDeveloperExceptionPage();
}

// it is necessarry for applying migrations while using docker
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PostgresContext>();
    dbContext.Database.Migrate();
}

app.MapControllers();

app.Run();
