using DZO_IntegracijaUstanovaDokumentacija_API.Errors;
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using DZO_IntegracijaUstanovaDokumentacija_API.Managers;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var defaultConnectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing connection string 'DefaultConnection' in appsettings");

builder.Services.AddDbContextPool<RazmenaDokumentacijeDb_Context>(options =>
{
    options.UseSqlServer(defaultConnectionString, sql =>
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
});

// options
builder.Services.AddOptions<GlobosSftpSetting>()
    .Bind(configuration.GetSection("GlobosSftpSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<CorisSftpSetting>()
    .Bind(configuration.GetSection("CorisSftpSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// services
builder.Services.AddTransient<GlobosSftpService>();
builder.Services.AddTransient<CorisSftpService>();
builder.Services.AddScoped<Logovi>();

builder.Services.AddScoped<SpecifikacijaManager>();
builder.Services.AddScoped<PrebacivanjeFajlovaManager>();
builder.Services.AddScoped<PrebacivanjeFolderaCorisuManager>();

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    o.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// 👇 OVO JE KLJUČ
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// 👇 u DEV koristi developer page, u PROD global handler
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(); // koristi GlobalExceptionHandler
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
