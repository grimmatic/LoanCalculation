using Microsoft.EntityFrameworkCore;
using Serilog;
using LoanCalculation.Data;
using LoanCalculation.Business.Interfaces;
using LoanCalculation.Business.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .WriteTo.Console();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IKrediHesaplamaService, KrediHesaplamaService>();
builder.Services.AddScoped<IKrediBasvuruService, KrediBasvuruService>();
builder.Services.AddScoped<IKrediOnayService, KrediOnayService>();
builder.Services.AddScoped<IBankaUrunService, BankaUrunService>();
builder.Services.AddScoped<IMusteriService, MusteriService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.Run();