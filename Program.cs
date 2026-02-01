using WebFarmaciaMiguetti.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 🛠️ ARQUITECTO: VOLVEMOS A LO NATIVO
// -----------------------------------------------------------------------------
// Tus logs confirmaron que Railway ya inyecta la variable "ASPNETCORE_URLS" 
// correctamente apuntando a 0.0.0.0 (IPv4).
// Eliminamos el bloque manual "ConfigureKestrel" porque estaba forzando IPv6 ([::])
// y causando el conflicto 502. Ahora confiamos 100% en la plataforma.
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
    // El HSTS ayuda a la seguridad, lo mantenemos.
    app.UseHsts();
}

// -----------------------------------------------------------------------------
// 🚫 HTTPS REDIRECTION: DESACTIVADO
// -----------------------------------------------------------------------------
// Railway maneja el HTTPS fuera de la app. Si lo activamos aquí, rompemos el bucle.
// app.UseHttpsRedirection(); 
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