using DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

IConfigurationRoot configuration = new ConfigurationBuilder()
         .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
         .AddJsonFile("appsettings.json")
         .Build();


builder.Services.AddDbContext<VizimIntegracijaDb_Context>(options =>
{
    options.UseSqlServer(@"Data Source = TEST-SQL; Initial Catalog = VizimIntegracijaDb_Test; User ID = sqluser; Password = GlobosTest1; TrustServerCertificate=True;Encrypt=true");
});


builder.Services.Configure<GlobosSftpSetting>(builder.Configuration.GetSection("GlobosSftpSettings"));
builder.Services.AddSingleton<GlobosSftpSetting>(); // SftpService kao singleton

var app = builder.Build();






// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthorization();

app.UseCors(x => x
         .AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader());

app.MapControllers();

app.Run();
