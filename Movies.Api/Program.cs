using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Movies.Api;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application;
using Movies.Application.Database;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
{
    ConfigurationManager config = builder.Configuration;
    
    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue<string>("Jwt:Key"))),
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = config.GetValue<string>("Jwt:Issuer"),
            ValidAudience = config.GetValue<string>("Jwt:Audience"),
            ValidateAudience = false,
            ValidateIssuer = false,
        };
    });

    builder.Services.AddAuthorization(x =>
    {
        x.AddPolicy(AuthConstants.AdminUserPolicyName, p => 
            p.RequireClaim(AuthConstants.AdminUserClaimName, "true"));

        x.AddPolicy(AuthConstants.TrustedMemberPolicyName, p =>
            p.RequireAssertion(c =>
                c.User.HasClaim(m => m is { Type: AuthConstants.AdminUserClaimName, Value: "true" }) ||
                c.User.HasClaim(m => m is { Type: AuthConstants.TrustedMemberClaimName, Value: "true" })));
    });

    builder.Services.AddApiVersioning(x =>
    {
        x.DefaultApiVersion = new ApiVersion(1.0);
        x.AssumeDefaultVersionWhenUnspecified = true;
        x.ReportApiVersions = true;
        x.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
    }).AddMvc();
    
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
    builder.Services.AddApplication();
    builder.Services.AddDatabase(config.GetConnectionString("Default"));
}

WebApplication app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<ValidationMappingMiddleware>();
    app.MapControllers();

    DbInitializer dbInitializer = app.Services.GetRequiredService<DbInitializer>();
    await dbInitializer.InitalizeAsync();
    
    app.Run();
}