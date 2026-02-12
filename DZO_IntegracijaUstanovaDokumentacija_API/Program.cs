using System;
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configuration (koristi ugra?eni builder.Configuration)
var configuration = builder.Configuration;

// Connection string: fail-fast ako nedostaje
var defaultConnectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing connection string 'DefaultConnection' in appsettings");

// DbContext pool + SQL retry + detalji u Dev
builder.Services.AddDbContextPool<RazmenaDokumentacijeDb_Context>(options =>
{
    options.UseSqlServer(defaultConnectionString, sql =>
    {
        sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// Options binding + validacija na startu
builder.Services.AddOptions<GlobosSftpSetting>()
    .Bind(configuration.GetSection("GlobosSftpSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<CorisSftpSetting>()
    .Bind(configuration.GetSection("CorisSftpSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<MediGroupSftpSettings>()
    .Bind(configuration.GetSection("MediGroupSftpSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// SFTP servisi: Transient (ili Scoped) — bez Singleton
builder.Services.AddTransient<GlobosSftpService>();
builder.Services.AddTransient<CorisSftpService>();
builder.Services.AddTransient<MediGroupSftpService>();

builder.Services.AddScoped<Logovi>();

// Controllers + JSON
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    o.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler(_ => { /* ProblemDetails ?e generisati telo */ });

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1"));
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();