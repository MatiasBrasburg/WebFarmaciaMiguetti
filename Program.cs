var builder = WebApplication.CreateBuilder(args);

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
// Esto habilita la inyección de IHttpContextAccessor en el Layouta
// para poder leer la URL y poner los títulos bonitos.
builder.Services.AddHttpContextAccessor();
// ====================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
// 🚨 CAMBIO TEMPORAL PARA VER EL ERROR REAL EN RAILWAY:
// Al sacar el "if", forzamos a que muestre el error completo,
// lo cual es CLAVE para el diagnóstico del HTTP 500.
app.UseDeveloperExceptionPage(); 
// 🚨 FIN DEL CAMBIO TEMPORAL

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 🔹 Importante: va antes de MapControllerRoute
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();