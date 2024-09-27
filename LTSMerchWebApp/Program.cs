using LTSMerchWebApp.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Agregar el contexto de base de datos MySQL
builder.Services.AddDbContext<LtsMerchStoreContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("LtsMerchStoreContext"),
    new MySqlServerVersion(new Version(8, 0, 26)))); // Cambia la versión a la de tu servidor MySQL

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
