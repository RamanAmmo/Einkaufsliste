using System.ComponentModel.DataAnnotations;
using einkaufslisterichtig.Data;
using einkaufslisterichtig.Models;
using Microsoft.EntityFrameworkCore;

namespace einkaufslisterichtig.Services;

/// <summary>Ergebnis einer Schreiboperation. Fehler werden als Text für die Oberfläche zurückgegeben.</summary>
public sealed record SpeicherErgebnis(bool Erfolg, string? Fehler = null)
{
    public static SpeicherErgebnis Ok { get; } = new(true);
    public static SpeicherErgebnis Fehlgeschlagen(string fehler) => new(false, fehler);
}

/// <summary>
/// Einzige Schicht, die auf die Datenbank zugreift. Komponenten kennen den DbContext nicht.
/// Jede Operation erzeugt über die Factory einen kurzlebigen DbContext (geeignet für Blazor Server).
/// </summary>
public sealed class EinkaufslistenService(IDbContextFactory<AppDbContext> dbFactory)
{
    // ---------- Lesen ----------

    public async Task<List<Artikel>> GetArtikelAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.Artikel.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }

    /// <summary>Lädt Gerichte samt Kategorie und Zutaten in einer Abfrage (kein N+1-Problem).</summary>
    public async Task<List<Gericht>> GetGerichteAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.Gerichte
            .AsNoTracking()
            .Include(g => g.Kategorie)
            .Include(g => g.GerichtZutaten)
                .ThenInclude(gz => gz.Zutat)
            .AsSplitQuery()
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    // ---------- Artikel ----------

    /// <summary>
    /// Fügt einen Artikel hinzu. Entspricht der Name einem gespeicherten Gericht, werden alle Zutaten übernommen.
    /// Gleiche Artikel mit gleicher Einheit werden zusammengeführt (Mengen addiert).
    /// </summary>
    public async Task<SpeicherErgebnis> ArtikelHinzufuegenAsync(string name, decimal menge, string einheit, string notiz)
    {
        name = name.Trim();
        einheit = einheit.Trim();
        notiz = notiz.Trim();

        var pruefling = new Artikel { Name = name, Menge = menge, Einheit = einheit, Notiz = notiz };
        if (!IstGueltig(pruefling, out List<ValidationResult> eingabeFehler))
        {
            return SpeicherErgebnis.Fehlgeschlagen(Fehlertext(eingabeFehler));
        }

        await using AppDbContext db = await dbFactory.CreateDbContextAsync();

        Gericht? gericht = await db.Gerichte
            .Include(g => g.GerichtZutaten).ThenInclude(gz => gz.Zutat)
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.Name == name);

        string[] namen = gericht is null
            ? new[] { name }
            : gericht.GerichtZutaten.Select(gz => gz.Zutat!.Name).ToArray();

        foreach (string artikelName in namen)
        {
            await HinzufuegenOderAddierenAsync(db, artikelName, menge, einheit, notiz);
        }

        List<Artikel> geaendert = db.ChangeTracker.Entries<Artikel>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();
        if (!KannSpeichern(geaendert, out List<ValidationResult> fehler))
        {
            return SpeicherErgebnis.Fehlgeschlagen(Fehlertext(fehler));
        }

        return await SpeichernAsync(db);
    }

    public async Task ErledigtSetzenAsync(int artikelId, bool erledigt)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        await db.Artikel
            .Where(a => a.Id == artikelId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Erledigt, erledigt));
    }

    public async Task ArtikelLoeschenAsync(int artikelId)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        await db.Artikel.Where(a => a.Id == artikelId).ExecuteDeleteAsync();
    }

    private static async Task HinzufuegenOderAddierenAsync(
        AppDbContext db, string name, decimal menge, string einheit, string notiz)
    {
        // Zuerst bereits getrackte (noch nicht gespeicherte) Artikel prüfen, dann die Datenbank.
        Artikel? vorhanden = db.Artikel.Local.FirstOrDefault(a =>
                string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.Einheit, einheit, StringComparison.OrdinalIgnoreCase))
            ?? await db.Artikel.FirstOrDefaultAsync(a => a.Name == name && a.Einheit == einheit);

        if (vorhanden is not null)
        {
            vorhanden.Menge += menge;
            if (string.IsNullOrWhiteSpace(vorhanden.Notiz) && !string.IsNullOrWhiteSpace(notiz))
            {
                vorhanden.Notiz = notiz;
            }

            return;
        }

        db.Artikel.Add(new Artikel { Name = name, Menge = menge, Einheit = einheit, Notiz = notiz });
    }

    // ---------- Gerichte ----------

    public async Task<SpeicherErgebnis> GerichtAnlegenAsync(string name, string kategorieName, string ersteZutat)
    {
        name = name.Trim();
        kategorieName = kategorieName.Trim();
        ersteZutat = ersteZutat.Trim();

        var kategoriePruefling = new Kategorie { Name = kategorieName };
        var zutatPruefling = new Zutat { Name = ersteZutat };
        if (!SindGueltig([kategoriePruefling, zutatPruefling], out List<ValidationResult> eingabeFehler))
        {
            return SpeicherErgebnis.Fehlgeschlagen(Fehlertext(eingabeFehler));
        }

        await using AppDbContext db = await dbFactory.CreateDbContextAsync();

        if (await db.Gerichte.AnyAsync(g => g.Name == name))
        {
            return SpeicherErgebnis.Fehlgeschlagen("Dieses Gericht ist bereits gespeichert.");
        }

        Kategorie kategorie = await db.Kategorien.FirstOrDefaultAsync(k => k.Name == kategorieName)
            ?? kategoriePruefling;
        Zutat zutat = await ZutatHolenOderAnlegenAsync(db, zutatPruefling);

        var gericht = new Gericht
        {
            Name = name,
            Kategorie = kategorie,
            GerichtZutaten = [new GerichtZutat { Zutat = zutat }]
        };

        if (!IstGueltig(gericht, out List<ValidationResult> fehler))
        {
            return SpeicherErgebnis.Fehlgeschlagen(Fehlertext(fehler));
        }

        db.Gerichte.Add(gericht);
        return await SpeichernAsync(db);
    }

    public async Task<SpeicherErgebnis> ZutatHinzufuegenAsync(int gerichtId, string zutatName)
    {
        zutatName = zutatName.Trim();
        var zutatPruefling = new Zutat { Name = zutatName };
        if (!IstGueltig(zutatPruefling, out List<ValidationResult> eingabeFehler))
        {
            return SpeicherErgebnis.Fehlgeschlagen(Fehlertext(eingabeFehler));
        }

        await using AppDbContext db = await dbFactory.CreateDbContextAsync();

        Gericht? gericht = await db.Gerichte
            .Include(g => g.GerichtZutaten).ThenInclude(gz => gz.Zutat)
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.Id == gerichtId);
        if (gericht is null)
        {
            return SpeicherErgebnis.Fehlgeschlagen("Das Gericht wurde nicht gefunden.");
        }

        if (gericht.GerichtZutaten.Any(gz =>
                string.Equals(gz.Zutat!.Name, zutatName, StringComparison.OrdinalIgnoreCase)))
        {
            return SpeicherErgebnis.Fehlgeschlagen("Diese Zutat ist im Gericht bereits enthalten.");
        }

        Zutat zutat = await ZutatHolenOderAnlegenAsync(db, zutatPruefling);
        gericht.GerichtZutaten.Add(new GerichtZutat { Gericht = gericht, Zutat = zutat });

        if (!IstGueltig(gericht, out List<ValidationResult> fehler))
        {
            return SpeicherErgebnis.Fehlgeschlagen(Fehlertext(fehler));
        }

        return await SpeichernAsync(db);
    }

    public async Task<SpeicherErgebnis> ZutatEntfernenAsync(int gerichtId, int zutatId)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();

        int anzahl = await db.GerichtZutaten.CountAsync(gz => gz.GerichtId == gerichtId);
        if (anzahl <= 1)
        {
            return SpeicherErgebnis.Fehlgeschlagen("Ein Gericht muss mindestens eine Zutat enthalten.");
        }

        await db.GerichtZutaten
            .Where(gz => gz.GerichtId == gerichtId && gz.ZutatId == zutatId)
            .ExecuteDeleteAsync();
        return SpeicherErgebnis.Ok;
    }

    public async Task GerichtLoeschenAsync(int gerichtId)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();

        // Die Zuordnungen in GerichtZutat werden durch ON DELETE CASCADE in der Datenbank mitgelöscht.
        await db.Gerichte.Where(g => g.Id == gerichtId).ExecuteDeleteAsync();
    }

    private static async Task<Zutat> ZutatHolenOderAnlegenAsync(AppDbContext db, Zutat neu)
    {
        return await db.Zutaten.FirstOrDefaultAsync(z => z.Name == neu.Name) ?? neu;
    }

    private static async Task<SpeicherErgebnis> SpeichernAsync(AppDbContext db)
    {
        try
        {
            await db.SaveChangesAsync();
            return SpeicherErgebnis.Ok;
        }
        catch (DbUpdateException)
        {
            return SpeicherErgebnis.Fehlgeschlagen(
                "Die Daten konnten nicht gespeichert werden. Möglicherweise existiert der Eintrag bereits.");
        }
    }

    // ---------- Validierung (ohne Datenbankzugriff) ----------

    public bool KannSpeichern(IEnumerable<object> modelle, out List<ValidationResult> fehler)
    {
        return SindGueltig(modelle, out fehler);
    }

    public bool IstGueltig(object model, out List<ValidationResult> fehler)
    {
        fehler = [];
        return Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            fehler,
            validateAllProperties: true);
    }

    public bool SindGueltig(IEnumerable<object> modelle, out List<ValidationResult> fehler)
    {
        fehler = [];
        foreach (object model in modelle)
        {
            if (!IstGueltig(model, out List<ValidationResult> modellfehler))
            {
                fehler.AddRange(modellfehler);
            }
        }

        return fehler.Count == 0;
    }

    private static string Fehlertext(IEnumerable<ValidationResult> fehler)
    {
        return string.Join(" ", fehler
            .Select(f => f.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct());
    }
}
