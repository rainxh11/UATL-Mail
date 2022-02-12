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

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme.Colored)
            .WriteTo.File(AppContext.BaseDirectory + @"\Log\UATLMail_Log.log", 
                rollingInterval: RollingInterval.Day, 
                rollOnFileSizeLimit: true, 
                retainedFileCountLimit:365,
                shared: true
            )
            .CreateLogger();

builder.Host.UseSerilog();


await DatabaseHelper.InitDb(builder.Configuration["MongoDB:DatabaseName"], builder.Configuration["MongoDB:ConnectionString"]); 

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddControllers(/*options => options.Filters.Add(new ValidationFilter())*/).AddNewtonsoftJson(o =>
{
    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    o.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
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

/*builder.Services
    .AddHostedService<DatabaseChangeService>();*/

builder.Services
    .AddScoped<IIdentityService, IdentityService>()
    .AddScoped<ITokenService, TokenService>();

builder.Services
    .AddScoped<SakonyConsoleMiddleware>()
    .AddScoped<MailAttachementVerificationMiddleware>();

builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
});

//---------------------------------------------------------------//
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseHttpLogging();
app.UseAuthorization();

app.MapControllers();
app
    .UseMiddleware<SakonyConsoleMiddleware>()
    .UseMiddleware<MailAttachementVerificationMiddleware>();

app.Run();
