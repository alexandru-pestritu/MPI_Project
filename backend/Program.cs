using System.Text;
using backend.Services;
using DbProvider.Database;
using DbProvider.Providers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["Key"];
var issuer = jwtSection["Issuer"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateLifetime = true,
            ValidateAudience = false
        };
    });

builder.Services.AddSingleton<IDatabaseConnectionDetails>(sp => 
    new DatabaseConnectionDetails("172.20.100.2", "MPI_Database", "sa", "Password123"));

builder.Services.AddSingleton<IDbManager>(sp =>
{
    IDatabaseConnectionDetails? connectionDetails = sp.GetService<IDatabaseConnectionDetails>();
    if (connectionDetails is null)
    {
        throw new InvalidOperationException("Database connection details not found");
    }

    return new DbManager(connectionDetails);
});

builder.Services.AddSingleton<IAuthProvider>(sp =>
{
    IDbManager? dbManager = sp.GetService<IDbManager>();
    if (dbManager is null)
    {
        throw new InvalidOperationException("Database manager not found");
    }

    return new AuthProvider(dbManager);
});

builder.Services.AddSingleton<IUserProvider>(sp =>
{
    IDbManager? dbManager = sp.GetService<IDbManager>();
    if (dbManager is null)
    {
        throw new InvalidOperationException("Database manager not found");
    }

    return new UserProvider(dbManager);
});

builder.Services.AddTransient<IEmailSender>(sp =>
{
    return new EmailSender(builder.Configuration);
});

builder.Services.AddSingleton<IAppEmailService>(sp =>
{
    IEmailSender? emailSender = sp.GetService<IEmailSender>();
    if (emailSender is null)
    {
        throw new InvalidOperationException("Email sender not found");
    }

    return new AppEmailService(emailSender);
});



builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAllOrigins",
        configurePolicy: policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MPI_Project", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your token:\n\nExample: Bearer abc123"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});



builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Index}/{id?}");

app.Run();