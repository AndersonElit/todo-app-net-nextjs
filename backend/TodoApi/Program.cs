using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TodoApi.Application.UseCases;
using TodoApi.Domain.Ports.Out;
using TodoApi.Domain.Ports.In;
using TodoApi.Infrastructure.MysqlDb.DrivenAdapters;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Get MySQL connection string from environment variable or use default
var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING");

// Register services
var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseMySql(connectionString, serverVersion,
        options => options.EnableRetryOnFailure()));
builder.Services.AddScoped<TodoRepository, TodoAdapter>();
builder.Services.AddScoped<TodoPort, TodoUseCase>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
