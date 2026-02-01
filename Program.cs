using WebFarmaciaMiguetti.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 🛠️ FIX 1: EL OÍDO ABSOLUTO (Configuración de Puerto Railway)
// -----------------------------------------------------------------------------
// Railway nos dice en qué puerto escuchar mediante la variable de entorno "PORT".
// Si no le hacemos caso explícitamente, Kestrel usa el 8080 o 5000 y Railway nos mata.
// Este bloque obliga a la app a usar el puerto que Railway quiere.
var portVar = Environment.GetEnvironmentVariable("PORT") ?? "8080";
if (!string.IsNullOrEmpty(portVar) && int.TryParse(portVar, out int port))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(port); // <--- ESTO ES LA CLAVE
    });
}
// -----------------------------------------------------------------------------

// 🔹 Necesario para Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// ====================================================================
// ✅ AGREGADO POR EL ARQUITECTO:
// Habilita IHttpContextAccessor para que funcionen tus vistas y Layouts
builder.Services.AddHttpContextAccessor();
// ====================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // El HSTS es bueno, lo dejamos.
    app.UseHsts();
}

// -----------------------------------------------------------------------------
// 🛠️ FIX 2: ELIMINAR LA REDIRECCIÓN HTTPS INTERNA
// -----------------------------------------------------------------------------
// Railway ya maneja el HTTPS "en la puerta" (Edge Proxy). 
// El tráfico llega a tu app como HTTP normal. Si forzamos la redirección aquí adentro,
// creamos un bucle infinito o un error de certificado, causando el error 502.
// app.UseHttpsRedirection();  <-- ¡COMENTADO INTENCIONALMENTE!
// -----------------------------------------------------------------------------

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 🔹 Importante: va antes de MapControllerRoute
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();