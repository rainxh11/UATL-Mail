using UATL.MailSystem.Models.Request;
using UATL.MailSystem.Models.Validations;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using UATL.MailSystem.Services;
using UATL.MailSystem.Helpers;
using Microsoft.OpenApi.Models;
using UATL.MailSystem.Models;
using UATL.MailSystem;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.AspNetCore;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Newtonsoft.Json.Serialization;
using Jetsons.JetPack;
using Microsoft.AspNetCore.SignalR;
using UATL.Mail.Hubs;
using Serilog.Enrichers;
using Serilog.Enrichers.AspNetCore;
using Hangfire.AspNetCore;
using Hangfire;
using Hangfire.Mongo;
using MongoDB.Entities;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using UATL.Mail.Services;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
            .Enrich.FromLogContext()
            .Enrich.WithClientIp()
            .Enrich.WithClientAgent()
            .Enrich.WithExceptionData()
            .Enrich.WithMemoryUsage()
            .Enrich.WithProcessName()
            .Enrich.WithThreadName()
            .WriteTo.Seq(builder.Configuration["SeqUrl"])
            .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme.Colored, restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File(AppContext.BaseDirectory + @"\Log\[DEBUG]_UATLMail_Log_.log", 
                rollingInterval: RollingInterval.Day, 
                rollOnFileSizeLimit: true, 
                retainedFileCountLimit:365,
                shared: true,
                restrictedToMinimumLevel: LogEventLevel.Debug)
            .WriteTo.File(AppContext.BaseDirectory + @"\Log\[VERBOSE]_UATLMail_Log_.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 365,
                shared: true,
                restrictedToMinimumLevel: LogEventLevel.Verbose)
            .WriteTo.File(AppContext.BaseDirectory + @"\Log\[ERROR]_UATLMail_Log_.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 365,
                shared: true,
                restrictedToMinimumLevel: LogEventLevel.Error)
            .WriteTo.File(AppContext.BaseDirectory + @"\Log\[WARNING]_UATLMail_Log_.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 365,
                shared: true,
                restrictedToMinimumLevel: LogEventLevel.Warning)
            .WriteTo.File(AppContext.BaseDirectory + @"\Log\[INFO]_UATLMail_Log_.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 365,
                shared: true,
                restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();

builder.Host.UseSerilog();

var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"];
var mongoDbName = builder.Configuration["MongoDB:DatabaseName"];
await DatabaseHelper.InitDb(mongoDbName, mongoConnectionString);
DatabaseHelper.InitCache();

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMongoStorage(mongoConnectionString, mongoDbName, new MongoStorageOptions
        {
            MigrationOptions = new MongoMigrationOptions
            {
                MigrationStrategy = new MigrateMongoMigrationStrategy(),
                BackupStrategy = new CollectionMongoBackupStrategy()
            },
            Prefix = "uatl.backgroundjobs.server",
            CheckConnection = true
        }));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddControllers(/*options => options.Filters.Add(new ValidationFilter())*/).AddNewtonsoftJson(o =>
{
    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    o.SerializerSettings.Formatting = Formatting.Indented;
    o.SerializerSettings.ContractResolver = null;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme()
    {
        BearerFormat = "JWT",
        Description = "Past JWT Bearer Token",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Reference = new OpenApiReference()
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
            }
        , Array.Empty<string>() }
    });
});
builder.Services.AddSwaggerGenNewtonsoftSupport();

builder.Services
    .AddFluentValidation(fv => fv
        .RegisterValidatorsFromAssemblyContaining<SignupValidator>(lifetime: ServiceLifetime.Singleton)
    );

builder.Services
    .AddHostedService<DatabaseChangeService>();

builder.Services
    .AddScoped<IIdentityService, IdentityService>()
    .AddScoped<ITokenService, TokenService>();



/*builder.Services
    //.AddScoped<SakonyConsoleMiddleware>()
    .AddScoped<MailAttachementVerificationMiddleware>();*/

builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
});

builder.Services.AddCors(options =>
{
    //options.AddDefaultPolicy(x => x.AllowAnyOrigin().AllowAnyMethod().AllowCredentials());
});

builder.Services.AddSignalR();
builder.Services.AddHangfireServer(serverOptions =>
{
    serverOptions.ServerName = "UATL Background Jobs Server 1";
});

builder.Services
    .AddSingleton<NotificationService>();
//---------------------------------------------------------------//
builder.WebHost
    .UseKestrel(o => {
        o.ListenAnyIP(builder.Configuration["Port"].ToInt());
        o.Limits.MaxRequestBodySize = int.MaxValue;
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(config => config
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins("http://127.0.0.1:8080")
    .AllowCredentials()
    );
app.UseAuthentication();
app.UseHttpLogging();

app.MapControllers();
/*app
    //.UseMiddleware<SakonyConsoleMiddleware>()
    .UseMiddleware<MailAttachementVerificationMiddleware>();*/

app.UseRouting();
app.UseAuthorization(); 
app.UseEndpoints(endpoint =>
{
    endpoint.MapHub<MailHub>("/hubs/mail");
    endpoint.MapHub<ChatHub>("/hubs/chat");
});

app.UseHangfireDashboard("/backgroundjobs");

app.Run();
