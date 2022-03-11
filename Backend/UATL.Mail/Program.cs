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
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using System.Reactive.Concurrency;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using FluentEmail;
using FluentEmail.Smtp;
using Hangfire.Dashboard;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using UATL.Mail;
using UATL.Mail.Helpers;
using UATL.Mail.Models;
using Ng.Services;


var builder = WebApplication.CreateBuilder(args);

//---------- Logging ---------------//
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
    .Enrich.FromLogContext()
    .Enrich.WithClientIp()
    .Enrich.WithClientAgent()
    .Enrich.WithExceptionData()
    .Enrich.WithMemoryUsage()
    .Enrich.WithProcessName()
    .Enrich.WithThreadName()
    .Enrich.WithDemystifiedStackTraces()
    .Enrich.WithRequestUserId()
    .WriteTo.Seq(builder.Configuration["SeqUrl"])
    .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme.Colored, restrictedToMinimumLevel: LogEventLevel.Information)
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
    .WriteTo.File(AppContext.BaseDirectory + @"\Log\[INFO]_UATLMail_Log_.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 365,
                shared: true,
                restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();

builder.Host.UseSerilog();

//---------- MongoDB & Caching ---------------//
var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"];
var mongoDbName = builder.Configuration["MongoDB:DatabaseName"];
await DatabaseHelper.InitDb(mongoDbName, mongoConnectionString);
DatabaseHelper.InitCache();

//---------- Background Jobs ---------------//
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMongoStorage(mongoConnectionString.Replace("UATL-Mail", "UATL-BackgroundService"), mongoDbName.Replace("UATL-Mail", "UATL-BackgroundService"), new MongoStorageOptions
    {
        MigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new MigrateMongoMigrationStrategy(),
            BackupStrategy = new CollectionMongoBackupStrategy()
        },
        Prefix = "uatl.backgroundjobs",
        CheckConnection = true,
    }));
    //.UseSQLiteStorage());


builder.Services.AddHangfireServer(serverOptions =>
{
    serverOptions.WorkerCount = builder.Configuration["BackgroundJobs:WorkerCount"].ToInt(20);
    serverOptions.ServerName = "UATL Background Jobs Server 1";
});
//---------- Authentication ---------------//
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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs")))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });




builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(ClaimTypes.NameIdentifier);
    });
});

//---------- JSON Serializer ---------------//
builder.Services.AddControllers(/*options => options.Filters.Add(new ValidationFilter())*/).AddNewtonsoftJson(o =>
{
    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    o.SerializerSettings.Formatting = Formatting.Indented;
    o.SerializerSettings.ContractResolver = null;
});

//---------- Swagger ---------------//
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

//---------- Validator ---------------//
builder.Services
    .AddFluentValidation(fv => fv
        .RegisterValidatorsFromAssemblyContaining<SignupValidator>(lifetime: ServiceLifetime.Singleton)
    );
//---------- MongoDB Change Stream ---------------//
builder.Services
    .AddHostedService<DatabaseChangeService>();

//---------- Token & Identity ---------------//
builder.Services
    .AddScoped<IIdentityService, IdentityService>()
    .AddScoped<ITokenService, TokenService>();

/*builder.Services
    //.AddScoped<SakonyConsoleMiddleware>()
    .AddScoped<MailAttachementVerificationMiddleware>();*/

builder.Services
    .AddScoped<TokenInjectionMiddleware>()
    .AddScoped<BasicAuthMiddleware>();

//---------- Attachments Limits ---------------//
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
});

//---------- CORS ---------------//
builder.Services.AddCors(options =>
{
    //options.AddDefaultPolicy(x => x.AllowAnyOrigin().AllowAnyMethod().AllowCredentials());
});

//---------- SignalR Websocket ---------------//

builder.Services.AddSignalR(x =>
{
    //x.EnableDetailedErrors = true;
});

//---------- NotificationService ---------------//
builder.Services
    .AddUserAgentService();
builder.Services
    .AddSingleton<LoginInfoSaver>()
    .AddSingleton<NotificationService>();

//---------- Email Notifications ---------------//
builder.Services
    .AddFluentEmail(builder.Configuration["SMTP:Email"], "Univesity Of Ammar Thlidji, LAGHOUAT - Mailing System")
    //.AddRazorRenderer()
    .AddSmtpSender(new SmtpClient(builder.Configuration["SMTP:Host"], builder.Configuration["SMTP:Port"].ToInt(587))
    {
        EnableSsl = true,
        Credentials = new NetworkCredential(builder.Configuration["SMTP:Email"],
            builder.Configuration["SMTP:Password"])
    });

//---------- Compression ---------------//
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes =
        ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "image/svg+xml+application/json" });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);
//---------- GraphQL Test ---------------//


//---------------------------------------------------------------------------------------------------------//
//------------- Hosting SPA App ------------------------------------------------------------------------//

builder.Services.AddSpaStaticFilesWithUrlRewrite(config =>
{
    config.RootPath = Path.Combine(AppContext.BaseDirectory, "dist");
});

//---------------------------------------------------------------------------------------------------------//
builder.WebHost
    .UseKestrel(o =>
    {
        o.ListenAnyIP(builder.Configuration["Port"].ToInt(), listenOptions =>
        {
            /*listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            listenOptions.UseHttps(cnxAdapter =>
            {
                /*cnxAdapter.SslProtocols = SslProtocols.Tls13;

                var certPath = Path.Combine(AppContext.BaseDirectory, builder.Configuration["Https:CertPemFile"]);
                var password = builder.Configuration["Https:Password"];
                cnxAdapter.ServerCertificate = X509Certificate2.CreateFromPemFile(certPath);
            });*/
        });
        o.Limits.MaxRequestBodySize = int.MaxValue;
    })
    .UseUrls();

var app = builder.Build();
//---------------------------------------------------------------------------------------------------------//
//---------------------------------------------------------------------------------------------------------//


app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.UseHttpsRedirection();
    //app.UseHsts();
}



app.UseCors(config => config
    .AllowAnyHeader()
    .AllowAnyMethod()
    //.AllowAnyOrigin()
    .WithOrigins(builder.Configuration["CorsHosts"].Split(";"))
    .AllowCredentials()
    );

app
    .UseMiddleware<TokenInjectionMiddleware>()
    .UseMiddleware<BasicAuthMiddleware>();
app.UseAuthentication();
app.UseDeveloperExceptionPage();

app.UseHttpLogging();
app.MapControllers();

/*app
    //.UseMiddleware<SakonyConsoleMiddleware>()
    .UseMiddleware<MailAttachementVerificationMiddleware>();*/

app.UseRouting();

app.UseWebSockets();
app.UseAuthorization();

/*app.MapGraphQL();
app.MapGraphQLSchema();
app.MapBananaCakePop();*/

app.UseEndpoints(endpoint =>
{
    endpoint.MapHub<MailHub>("/hubs/mail");
    endpoint.MapHub<ChatHub>("/hubs/chat");
});


app.UseHangfireDashboard("/backgroundjobs", new DashboardOptions()
{
    DashboardTitle = "UATL Mail Server - Background Jobs Dashboard",
    IsReadOnlyFunc = ctx => true,    
    AsyncAuthorization = new []
    {
        new DashboardAuthFilter()
    },
    AppPath = "/"
});

if (!app.Environment.IsDevelopment())
{
    app.UseSpaStaticFiles();
    app.UseSpa(config =>
    {
    });
}


app.Run();
