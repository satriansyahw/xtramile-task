using Microsoft.EntityFrameworkCore;
using WeatherApp.Api.Application.Interfaces;
using WeatherApp.Api.Infrastructure.Data;
using WeatherApp.Api.Infrastructure.Services;
using WeatherApp.Api.Infrastructure.Middleware;

using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Setup In-Memory Database
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseInMemoryDatabase("WeatherDb"));

// Register Services
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddTransient<GlobalExceptionMiddleware>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
