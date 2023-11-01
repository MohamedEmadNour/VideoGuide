using ASU_Research_2022.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
//using PhoneLogApi.Services;
using VideoGuide.Configurations;
using VideoGuide.Data;
using VideoGuide.Models;
using VideoGuide.Repository;
using VideoGuide.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
builder.Services.AddDbContext<VideoGuidDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
builder.Services.AddDbContext<VideoGuideContext>(options =>
{
    options.UseSqlServer(connectionString);
});
// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
      .AddEntityFrameworkStores<VideoGuidDbContext>()
      .AddDefaultTokenProviders();
builder.Services.AddSignalR();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;         // Don't require a digit (number)
    options.Password.RequiredLength = 6;           // Minimum password length
    options.Password.RequireNonAlphanumeric = false; // Don't require special characters
    options.Password.RequireUppercase = false;     // Don't require an uppercase letter
    options.Password.RequireLowercase = false;     // Don't require an lowercase letter
});
builder.Services.AddScoped<UnitOfWork>();

builder.Services.AddScoped<IAuthManager, AuthManager>();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:4200", "http://localhost:59959", "http://172.16.118.6:59959", "http://Telephone_Directory.hos.asu.edu.eg")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MapperInitilizer));
// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
}
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PhoneLog API", Version = "v1" });
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
