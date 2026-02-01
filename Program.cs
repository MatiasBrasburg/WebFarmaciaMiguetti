using WebFarmaciaMiguetti.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 🛠️ ARQUITECTO: FORZAR PUERTO Y PROTOCOLO (FIX 502 TIMEOUT)
// -----------------------------------------------------------------------------
var portVar = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 ARQUITECTO: Iniciando en puerto {portVar}"); // Para ver en logs

builder.WebHost.ConfigureKestrel(options =>
{
    // Escuchamos en 0.0.0.0 (IPv4) para asegurar compatibilidad con el proxy
    options.ListenAnyIP(int.Parse(portVar)); 
});
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

// 🚫 HTTPS REDIRECTION: DESACTIVADO (Crucial en Railway)
// app.UseHttpsRedirection(); 

app.UseStaticFiles();
app.UseRouting();

// 🔹 ORDEN CRÍTICO: Session debe ir antes de Authorization y el mapeo de rutas
app.UseSession(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();