using Application;
using Application.Interface;
using Common.Configurations;
using Domain.Entities;
using Infrastructure.Persistance;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Configuration
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);


// Configure AppSettings
builder.Services.Configure<ApplicationConfiguration>(configuration.GetSection(ApplicationConfiguration.SECTION_NAME));
builder.Services.Configure<CorsConfiguration>(configuration.GetSection(CorsConfiguration.SECTION_NAME));
builder.Services.Configure<EmailConfiguration>(configuration.GetSection(EmailConfiguration.SECTION_NAME));
builder.Services.Configure<JwtConfiguration>(configuration.GetSection(JwtConfiguration.SECTION_NAME));

// Options Configuration
builder.Services.AddOptions();
builder.Services.Configure<EmailConfiguration>(
    builder.Configuration.GetSection(EmailConfiguration.SECTION_NAME));

// Database Context
builder.Services.AddDbContext<TemplateDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddScoped<ITemplateDbContext>(provider =>
    provider.GetService<TemplateDbContext>());

// Adding Services
builder.Services.AddApplication();


// Identity Configuration
builder.Services.AddIdentity<User, Role>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<TemplateDbContext>()
.AddDefaultTokenProviders();

// Authentication Configuration
const string ACCOUNT_MFA_SCHEME_NAME = "MfaBearer";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(ACCOUNT_MFA_SCHEME_NAME, options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = CreateTokenValidationParameters(builder.Configuration);
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = CreateTokenValidationParameters(builder.Configuration);
});



// Authorization Configuration
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Mfa Endpoint", policy =>
    {
        policy.AddAuthenticationSchemes(ACCOUNT_MFA_SCHEME_NAME);
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var mfaClaim = context.User.Claims
                .FirstOrDefault(fd => fd.Type == ClaimTypes.AuthenticationMethod)?.Value;
            return !string.IsNullOrEmpty(mfaClaim) && mfaClaim == "mfa";
        });
    });
});

// SMTP Client Configuration
builder.Services.AddTransient<SmtpClient>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new SmtpClient
    {
        Host = config.GetValue<string>("SMTP:Host"),
        Port = config.GetValue<int>("SMTP:Port"),
        Credentials = new System.Net.NetworkCredential(
            config.GetValue<string>("SMTP:Sender"),
            config.GetValue<string>("SMTP:Password")
        )
    };
});

// Service Registrations
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IErrorLogService, ErrorLogService>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(corsBuilder =>
    {
        var corsSection = builder.Configuration.GetSection(CorsConfiguration.SECTION_NAME);
        var corsConfig = new CorsConfiguration();
        corsSection.Bind(corsConfig);

        corsBuilder
            .WithOrigins(corsConfig.AllowedOrigins ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders(corsConfig.ExposedHeaders);
    });
});
// Standard ASP.NET Core Service Registrations
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = $"Template - {builder.Environment.EnvironmentName}"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Description = "Please enter a valid token. Token is generated from Login Methods.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference=new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{ }
        }
    });
    options.CustomSchemaIds(x => x.FullName);
});


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static TokenValidationParameters CreateTokenValidationParameters(IConfiguration configuration)
{
    return new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = configuration["JWT:Audience"],
        ValidIssuer = configuration["JWT:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["JWT:Key"])),
        ClockSkew = TimeSpan.Zero
    };
}