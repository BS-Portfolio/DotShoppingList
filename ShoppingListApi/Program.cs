using NLog;
using NLog.Extensions.Logging;
using ShoppingListApi.Services;
using Newtonsoft.Json;

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
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ConnectionStringService>();
builder.Services.AddTransient<DatabaseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();