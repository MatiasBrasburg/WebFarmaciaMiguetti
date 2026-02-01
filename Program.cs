using WebFarmaciaMiguetti.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 🛠️ ARQUITECTO: CONFIGURACIÓN SEGÚN MANUAL DE RAILWAY
// -----------------------------------------------------------------------------
// 1. Obtenemos el puerto que Railway nos inyecta (PORT). Si no existe, usamos 8080.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

// 2. Forzamos a la aplicación a escuchar en 0.0.0.0 (IPv4) y en ese puerto.
// Esto soluciona el error 502 porque alinea el servidor con el Proxy de Railway.
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
// -----------------------------------------------------------------------------

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 🚫 DESACTIVADO: Railway ya maneja el HTTPS
// app.UseHttpsRedirection(); 

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();