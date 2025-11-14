using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Passly.Domain;
using Passly.Options;
using Passly.Repositories;
using Passly.Repositories.Impl;
using Passly.Services;
using Passly.Services.Impl;
using Passly.Services.Impl.Otp;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<PasswordHashingOptions>(
    builder.Configuration.GetSection("PasswordHashing")).AddOptionsWithValidateOnStart<PasswordHashingOptions>();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt")).AddOptionsWithValidateOnStart<JwtOptions>();

builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection("Smtp")).AddOptionsWithValidateOnStart<SmtpOptions>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var jwtOptions = builder.Configuration
        .GetSection("Jwt")
        .Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt configuration is missing");

    var secretKeyBytes = Convert.FromBase64String(jwtOptions.Secret);
    var securityKey = new SymmetricSecurityKey(secretKeyBytes);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = securityKey,

        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var userId = context.Principal?.FindFirst("user_id")?.Value;
            logger.LogDebug("Token validated for user {UserId}", userId);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();


builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ITokenHasher, TokenHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVaultRepository, VaultRepository>();
builder.Services.AddScoped<IVaultService, VaultService>();

builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IOtpEmailService, OtpEmailService>();
builder.Services.AddHostedService<OtpCleanupService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:") 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddTransient<SmtpClient>(sp =>
{
    var smtpOptions = sp.GetRequiredService<IOptions<SmtpOptions>>().Value;
    return new SmtpClient(smtpOptions.Server, smtpOptions.Port)
    {
        EnableSsl = true,
        Credentials = new NetworkCredential(
            smtpOptions.Username,
            smtpOptions.Password
        )
    };
});


var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
