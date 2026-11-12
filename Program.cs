using einkaufslisterichtig.Components;
using einkaufslisterichtig.Data;
using einkaufslisterichtig.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Blazor Server: Eine Verbindung (Circuit) lebt lange und Komponenten können gleichzeitig Aufgaben starten.
// Ein einzelner, langlebiger DbContext wäre nicht thread-sicher. Deshalb wird eine IDbContextFactory
// registriert und der Service erzeugt pro Operation einen kurzlebigen DbContext.
string connectionString = builder.Configuration.GetConnectionString("Einkaufsliste")
    ?? "Data Source=einkaufsliste.db;Foreign Keys=True";
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<EinkaufslistenService>();

var app = builder.Build();

// Schema aus den Migrationen erzeugen bzw. aktualisieren (legt auch die Seed-Daten an).
using (IServiceScope scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using AppDbContext db = factory.CreateDbContext();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
