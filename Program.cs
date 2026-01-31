using WebFarmaciaMiguetti.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Servicios Esenciales
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor(); // Necesario para obtener datos en las Vistas

var app = builder.Build();

// 2. Configuración de Errores y Seguridad
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // app.UseHsts(); // Comentado para evitar problemas de redirección en Railway sin dominio
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession(); // ¡Importante! Debe ir antes de los controladores

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 3. DETECCIÓN AUTOMÁTICA DEL PUERTO DE RAILWAY
var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrEmpty(port))
{
    // Si Railway nos da un puerto, lo usamos (Producción)
    app.Run($"http://0.0.0.0:{port}");
}
else
{
    // Si estamos en tu PC, corre normal (Local)
    app.Run();
}