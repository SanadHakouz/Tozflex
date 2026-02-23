using Microsoft.EntityFrameworkCore;
using MyMovies.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ──── SERVICES ────
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")!)
);

// CORS — let Angular talk to us
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ──── MIDDLEWARE ────
app.UseCors("AllowAngular");    // Must be before MapControllers
app.UseAuthorization();
app.MapControllers();

app.Run();