using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Extensions.Logging;
using ShoppingListApi.Services;
using Newtonsoft.Json;
using ShoppingListApi.Authentication;
using ShoppingListApi.Data.Contexts;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddNLog();

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    // Optional: Customize settings for Newtonsoft.Json
    options.SerializerSettings.Formatting = Formatting.Indented; // Pretty-print JSON
    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; // Ignore null values in JSON output
});
;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Shopping List API", Version = "v1" });

    // 🔐 API Key Authentication (Only API Key)
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-API-KEY", // 🔹 API Key header for basic auth
        Type = SecuritySchemeType.ApiKey,
        Description = "Enter your API key",
        Scheme = "ApiKeyScheme"
    });

    // 🔐 API Key + User ID Authentication
    options.AddSecurityDefinition("UserKeyAndUserId", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "USER-KEY", // 🔹 User API Key header
        Type = SecuritySchemeType.ApiKey,
        Description = "Enter your User API Key (Requires USER-ID as well)",
        Scheme = "UserKeyAndUserIdScheme"
    });

    options.AddSecurityDefinition("UserId", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "USER-ID", // 🔹 User ID header
        Type = SecuritySchemeType.ApiKey,
        Description = "Enter your User ID",
        Scheme = "UserIdScheme"
    }
    );

    // 🔐 Apply Security Policies
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        // Policy 1: Requires API Key
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            new string[] { }
        },
        // Policy 2: Requires USER-KEY + USER-ID
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "UserKeyAndUserId" }
            },
            new string[] { }
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "UserId" }
            },
            new string[] { }
        }
    });
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DotShoppingListStoreLocal")));

builder.Services.AddTransient<ConnectionStringService>();
builder.Services.AddTransient<DatabaseService>();
builder.Services.AddTransient<MyAuthenticationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.AllowAnyOrigin() // allow specific origin
            .WithMethods("GET").WithHeaders("USER-ID", "USER-KEY", "X-API-KEY");
        policy.AllowAnyOrigin()
            .WithMethods("POST", "PATCH", "DELETE")
            .WithHeaders("X-Frontend", "accept", "content-type");
    });
});

var app = builder.Build();

app.UseCors("AllowSpecificOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseMiddleware<MyAuthenticationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();