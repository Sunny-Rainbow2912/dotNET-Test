using AutoMapper;
using Test;
using Microsoft.EntityFrameworkCore;
using Test.Utils;
using Test.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb"));


IMapper mapper = MappingConfg.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//Add services to the container.
builder.Services.AddControllers();

//Configure Middleware
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// app.UseMiddleware<LoggingMiddleware>();
// app.UseMiddleware<ValidationMiddleware>();

app.MapControllers();

app.Run();

