using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TodoApi.Application.UseCases;
using TodoApi.Domain.Ports.Out;
using TodoApi.Domain.Ports.In;
using TodoApi.Infrastructure.DrivenAdapters;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Register services
var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseMySql("server=192.168.1.15;port=3306;database=tododb;user=root;password=12345", serverVersion,
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
