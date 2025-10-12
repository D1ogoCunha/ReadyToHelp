using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using readytohelpapi.User.Data;
using readytohelpapi.User.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable string enum conversion for JSON
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
var pgUsername = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "readytohelp";
var pgPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "readytohelppwd";


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserServiceImpl>();

builder.Services.AddDbContext<UserContext>(options =>
    options.UseNpgsql(
        $"Host={postgresHost};Username={pgUsername};Password={pgPassword};Database=readytohelp_db"
    ));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

