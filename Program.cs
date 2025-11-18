using Facturas.Components;
using Microsoft.EntityFrameworkCore;
using SistemaFacturas.Data;
using SistemaFacturas.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<FacturasDbContext>(options =>
    options.UseSqlite("Data Source=facturas.db")
           .EnableSensitiveDataLogging()  // Para ver más detalles del error
           .LogTo(Console.WriteLine));     // Para logging

builder.Services.AddScoped<ConsultasSistema>();

var app = builder.Build();

// Mover esto DESPUÉS de crear el app
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<FacturasDbContext>();
        // Usar Migrate en lugar de EnsureCreated
        db.Database.Migrate();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error al inicializar DB: {ex.Message}");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();