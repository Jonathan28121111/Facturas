using Facturas.Components;
using Microsoft.EntityFrameworkCore;
using SistemaFacturas.Data;
using SistemaFacturas.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<FacturasDbContext>(options =>
    options.UseSqlite("Data Source=facturas.db"));

builder.Services.AddScoped<ConsultasSistema>();
builder.Services.AddScoped<GestionArchivadas>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FacturasDbContext>();
    db.Database.EnsureCreated();
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