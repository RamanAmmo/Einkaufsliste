using einkaufslisterichtig.Data;
using einkaufslisterichtig.Models;
using Microsoft.EntityFrameworkCore;

namespace einkaufslisterichtig.Tests;

/// <summary>Integrationstests gegen eine echte SQLite-Datenbank (In-Memory, pro Test neu).</summary>
public class DatenbankIntegrationTests
{
    [Fact]
    public async Task Seed_Daten_sind_vorhanden_und_Milch_gehoert_zu_mehreren_Gerichten()
    {
        using var datenbank = new TestDatenbank();
        await using AppDbContext db = datenbank.CreateDbContext();

        Assert.True(await db.Gerichte.CountAsync() >= 3);
        Assert.True(await db.Artikel.AnyAsync());

        // n:m: Eine Zutat in mehreren Gerichten.
        Zutat milch = await db.Zutaten.Include(z => z.GerichtZutaten).SingleAsync(z => z.Name == "Milch");
        Assert.True(milch.GerichtZutaten.Count >= 3);
    }

    [Fact]
    public async Task Gerichte_werden_mit_Kategorie_und_Zutaten_geladen()
    {
        using var datenbank = new TestDatenbank();

        List<Gericht> gerichte = await datenbank.NeuerService().GetGerichteAsync();

        Gericht pfannkuchen = gerichte.Single(g => g.Name == "Pfannkuchen");
        Assert.Equal("Frühstück", pfannkuchen.Kategorie!.Name);
        Assert.Contains(pfannkuchen.GerichtZutaten, gz => gz.Zutat!.Name == "Mehl");
    }

    [Fact]
    public async Task Gleicher_Zutatenname_in_anderer_Schreibweise_verletzt_den_Unique_Index()
    {
        using var datenbank = new TestDatenbank();
        await using AppDbContext db = datenbank.CreateDbContext();

        db.Zutaten.Add(new Zutat { Name = "MEHL" });   // "Mehl" existiert durch die Seed-Daten

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Dieselbe_Zutat_darf_nicht_zweimal_im_selben_Gericht_stehen()
    {
        using var datenbank = new TestDatenbank();
        await using AppDbContext db = datenbank.CreateDbContext();

        db.GerichtZutaten.Add(new GerichtZutat { GerichtId = 1, ZutatId = 1 });   // gibt es bereits

        await Assert.ThrowsAnyAsync<Exception>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Gericht_loeschen_entfernt_Zuordnungen_aber_nicht_die_Zutaten()
    {
        using var datenbank = new TestDatenbank();
        int zutatenVorher;
        await using (AppDbContext db = datenbank.CreateDbContext())
        {
            zutatenVorher = await db.Zutaten.CountAsync();
        }

        await datenbank.NeuerService().GerichtLoeschenAsync(1);

        await using AppDbContext pruefung = datenbank.CreateDbContext();
        Assert.False(await pruefung.Gerichte.AnyAsync(g => g.Id == 1));
        Assert.False(await pruefung.GerichtZutaten.AnyAsync(gz => gz.GerichtId == 1));   // Cascade
        Assert.Equal(zutatenVorher, await pruefung.Zutaten.CountAsync());                 // Zutaten bleiben
    }

    [Fact]
    public async Task Kategorie_mit_Gerichten_kann_nicht_geloescht_werden()
    {
        using var datenbank = new TestDatenbank();
        await using AppDbContext db = datenbank.CreateDbContext();

        Kategorie fruehstueck = await db.Kategorien.SingleAsync(k => k.Name == "Frühstück");
        db.Kategorien.Remove(fruehstueck);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());   // Restrict
    }

    [Fact]
    public async Task Verwendete_Zutat_kann_nicht_geloescht_werden()
    {
        using var datenbank = new TestDatenbank();
        await using AppDbContext db = datenbank.CreateDbContext();

        Zutat mehl = await db.Zutaten.SingleAsync(z => z.Name == "Mehl");
        db.Zutaten.Remove(mehl);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());   // Restrict
    }

    [Fact]
    public async Task Check_Constraint_lehnt_ungueltige_Menge_ab_auch_ohne_Validierung()
    {
        using var datenbank = new TestDatenbank();
        await using AppDbContext db = datenbank.CreateDbContext();

        db.Artikel.Add(new Artikel { Name = "Salz", Menge = 0, Einheit = "kg" });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Artikel_mit_gleicher_Einheit_werden_zusammengefuehrt()
    {
        using var datenbank = new TestDatenbank();
        var service = datenbank.NeuerService();

        await service.ArtikelHinzufuegenAsync("Tomaten", 2, "Stück", "");
        await service.ArtikelHinzufuegenAsync("tomaten", 3, "Stück", "");   // andere Schreibweise
        await service.ArtikelHinzufuegenAsync("Tomaten", 2, "kg", "");      // andere Einheit

        List<Artikel> tomaten = (await service.GetArtikelAsync())
            .Where(a => a.Name == "Tomaten").ToList();
        Assert.Equal(2, tomaten.Count);
        Assert.Equal(5, tomaten.Single(a => a.Einheit == "Stück").Menge);
        Assert.Equal(2, tomaten.Single(a => a.Einheit == "kg").Menge);
    }

    [Fact]
    public async Task Gericht_als_Artikel_uebernimmt_alle_Zutaten()
    {
        using var datenbank = new TestDatenbank();
        var service = datenbank.NeuerService();

        var ergebnis = await service.ArtikelHinzufuegenAsync("Pfannkuchen", 1, "Portion", "");

        Assert.True(ergebnis.Erfolg, ergebnis.Fehler);
        List<Artikel> artikel = await service.GetArtikelAsync();
        foreach (string zutat in new[] { "Mehl", "Milch", "Eier", "Butter" })
        {
            Assert.Contains(artikel, a => a.Name == zutat && a.Einheit == "Portion");
        }
    }

    [Fact]
    public async Task Gericht_anlegen_Zutat_hinzufuegen_und_Duplikat_ablehnen()
    {
        using var datenbank = new TestDatenbank();
        var service = datenbank.NeuerService();

        var angelegt = await service.GerichtAnlegenAsync("Palatschinken", "Dessert", "Mehl");
        Assert.True(angelegt.Erfolg, angelegt.Fehler);

        Gericht gericht = (await service.GetGerichteAsync()).Single(g => g.Name == "Palatschinken");
        Assert.Equal("Dessert", gericht.Kategorie!.Name);            // vorhandene Kategorie wiederverwendet
        Assert.Single(gericht.GerichtZutaten);

        Assert.True((await service.ZutatHinzufuegenAsync(gericht.Id, "Marmelade")).Erfolg);
        Assert.False((await service.ZutatHinzufuegenAsync(gericht.Id, "marmelade")).Erfolg);   // Duplikat
        Assert.False((await service.GerichtAnlegenAsync("palatschinken", "Dessert", "Mehl")).Erfolg);

        await using AppDbContext db = datenbank.CreateDbContext();
        Assert.Equal(1, await db.Zutaten.CountAsync(z => z.Name == "Mehl"));   // Zutat nur einmal gespeichert
    }

    [Fact]
    public async Task Letzte_Zutat_eines_Gerichts_kann_nicht_entfernt_werden()
    {
        using var datenbank = new TestDatenbank();
        var service = datenbank.NeuerService();
        await service.GerichtAnlegenAsync("Toast", "Frühstück", "Brot");
        Gericht toast = (await service.GetGerichteAsync()).Single(g => g.Name == "Toast");

        var ergebnis = await service.ZutatEntfernenAsync(toast.Id, toast.GerichtZutaten.Single().ZutatId);

        Assert.False(ergebnis.Erfolg);
    }

    [Fact]
    public async Task Erledigt_aendern_und_Artikel_loeschen_wirken_in_der_Datenbank()
    {
        using var datenbank = new TestDatenbank();
        var service = datenbank.NeuerService();

        await service.ErledigtSetzenAsync(1, true);
        Assert.True((await service.GetArtikelAsync()).Single(a => a.Id == 1).Erledigt);

        await service.ArtikelLoeschenAsync(1);
        Assert.DoesNotContain(await service.GetArtikelAsync(), a => a.Id == 1);
    }

    [Fact]
    public async Task Tests_sind_unabhaengig_jede_Datenbank_startet_frisch()
    {
        using (var erste = new TestDatenbank())
        {
            await erste.NeuerService().ArtikelLoeschenAsync(1);
        }

        using var zweite = new TestDatenbank();
        Assert.Contains(await zweite.NeuerService().GetArtikelAsync(), a => a.Id == 1);
    }
}
