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
// Esto habilita la inyección de IHttpContextAccessor en el Layout
// para poder leer la URL y poner los títulos bonitos.
builder.Services.AddHttpContextAccessor();
// ====================================================================

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

// 🔹 Importante: va antes de MapControllerRoute
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();