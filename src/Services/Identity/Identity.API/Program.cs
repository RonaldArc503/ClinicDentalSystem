using Identity.Application;
using Identity.Application.Constants;
using Identity.Application.Interfaces;
using Identity.Infrastructure;
using Identity.Persistence;
using Identity.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SharedKernel.Middleware;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/identity-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console()
        .WriteTo.File("logs/identity-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity.API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var jwtSection = builder.Configuration.GetSection("Jwt");
    var jwtKey = jwtSection["Key"];

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
        ClockSkew = TimeSpan.Zero
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAdminRole", policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy("RequireDentistRole", policy =>
            policy.RequireRole("Dentist"));

        options.AddPolicy(Permissions.Patients.Read, policy =>
            policy.RequireClaim("permission", Permissions.Patients.Read));

        options.AddPolicy(Permissions.Patients.Write, policy =>
            policy.RequireClaim("permission", Permissions.Patients.Write));

        options.AddPolicy(Permissions.Appointments.Read, policy =>
            policy.RequireClaim("permission", Permissions.Appointments.Read));

        options.AddPolicy(Permissions.Appointments.Write, policy =>
            policy.RequireClaim("permission", Permissions.Appointments.Write));

        options.AddPolicy(Permissions.Billing.Read, policy =>
            policy.RequireClaim("permission", Permissions.Billing.Read));

        options.AddPolicy(Permissions.Billing.Write, policy =>
            policy.RequireClaim("permission", Permissions.Billing.Write));

        options.AddPolicy(Permissions.Treatments.Read, policy =>
            policy.RequireClaim("permission", Permissions.Treatments.Read));

        options.AddPolicy(Permissions.Treatments.Write, policy =>
            policy.RequireClaim("permission", Permissions.Treatments.Write));
    });

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<IdentityDbContext>(name: "SQL Server");

    builder.Services.AddIdentityPersistence(builder.Configuration);
    builder.Services.AddIdentityApplication();
    builder.Services.AddIdentityInfrastructure(builder.Configuration);

    var app = builder.Build();

    app.UseSharedExceptionHandler();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await context.Database.EnsureCreatedAsync();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var adminPassword = builder.Configuration["AdminPassword"] ?? "Admin123!";
        var adminPasswordHash = passwordHasher.Hash(adminPassword);
        await SeedData.InitializeAsync(context, adminPasswordHash);
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
