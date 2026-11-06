using einkaufslisterichtig.Data;
using einkaufslisterichtig.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace einkaufslisterichtig.Tests;

/// <summary>
/// Echte SQLite-In-Memory-Datenbank pro Test. Die Verbindung bleibt offen, solange das Objekt lebt,
/// sonst würde die Datenbank sofort verschwinden. Jeder Test bekommt seine eigene, frische Datenbank.
/// Das Schema wird mit EnsureCreated aus dem Modell erzeugt (inklusive Seed-Daten, Constraints und Indizes).
/// </summary>
public sealed class TestDatenbank : IDbContextFactory<AppDbContext>, IDisposable
{
    private readonly SqliteConnection verbindung;
    private readonly DbContextOptions<AppDbContext> optionen;

    public TestDatenbank()
    {
        verbindung = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        verbindung.Open();
        optionen = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(verbindung).Options;

        using AppDbContext db = CreateDbContext();
        db.Database.EnsureCreated();
    }

    public AppDbContext CreateDbContext() => new(optionen);

    public EinkaufslistenService NeuerService() => new(this);

    public void Dispose() => verbindung.Dispose();
}

/// <summary>Factory, die nie benutzt werden darf. Damit beweisen Unit-Tests, dass sie ohne Datenbank laufen.</summary>
public sealed class KeineDatenbankFactory : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext() =>
        throw new InvalidOperationException("Unit-Tests dürfen keine Datenbank verwenden.");
}
