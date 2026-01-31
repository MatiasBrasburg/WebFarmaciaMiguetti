using WebFarmaciaMiguetti.Models;

var builder = WebApplication.CreateBuilder(args);

// Servicios
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

// Configuración de Errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // app.UseHsts(); // Comentado por seguridad en Railway si no tienes dominio propio
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ====================================================================
// CORRECCIÓN CRÍTICA PARA RAILWAY: PUERTO DINÁMICO
// ====================================================================
var portVar = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(portVar))
{
    // Railway nos pasó un puerto, lo usamos.
    app.Run($"http://0.0.0.0:{portVar}");
}
else
{
    // Entorno local
    app.Run();
}