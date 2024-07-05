using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Dodavanje konfiguracije iz appsettings.json
IConfigurationRoot? config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                                .Build();

builder.Configuration.AddEnvironmentVariables();

string? defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<VizimIntegracijaDb_Context>(options => options.UseSqlServer(defaultConnectionString));

// Konfiguracija GlobosSftpSetting
builder.Services.Configure<GlobosSftpSetting>(builder.Configuration.GetSection("GlobosSftpSettings"));
builder.Services.Configure<CorisSftpSetting>(builder.Configuration.GetSection("CorisSftpSettings"));

// Dodavanje GlobosSftpService kao singleton
builder.Services.AddSingleton<GlobosSftpService>();
builder.Services.AddSingleton<CorisSftpService>();


builder.Services.AddScoped<Logovi>();

// Dodavanje ostalih servisa
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1"));
}

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();