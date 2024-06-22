using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Dodavanje konfiguracije iz appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Konfiguracija GlobosSftpSetting
builder.Services.Configure<GlobosSftpSetting>(builder.Configuration.GetSection("GlobosSftpSettings"));

// Dodavanje GlobosSftpService kao singleton
builder.Services.AddSingleton<GlobosSftpService>();

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