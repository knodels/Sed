using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SED.Api.Data;
using SED.Api.Repositories;
using SED.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SED API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Введите JWT токен",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    SeedData(context);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static void SeedData(AppDbContext context)
{
    if (context.Users.Any())
        return;

    context.Users.AddRange(
        new SED.Api.Models.User
        {
            Name = "Иванов Иван",
            Email = "admin@company.ru",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin"
        },
        new SED.Api.Models.User
        {
            Name = "Петров Пётр",
            Email = "petrov@company.ru",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("petrov123"),
            Role = "Employee"
        },
        new SED.Api.Models.User
        {
            Name = "Сидорова Анна",
            Email = "sidorova@company.ru",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("sidorova123"),
            Role = "Approver"
        },
        new SED.Api.Models.User
        {
            Name = "Козлов Дмитрий",
            Email = "kozlov@company.ru",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("kozlov123"),
            Role = "Approver"
        },
        new SED.Api.Models.User
        {
            Name = "Морозова Ольга",
            Email = "morozova@company.ru",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("morozova123"),
            Role = "Manager"
        }
    );
    context.SaveChanges();
}