using einkaufslisterichtig.Models;
using einkaufslisterichtig.Services;

namespace einkaufslisterichtig.Tests;

/// <summary>Unit-Tests für die Validierungslogik der Service-Schicht. Es wird keine Datenbank benötigt.</summary>
public class EinkaufslistenServiceUnitTests
{
    private readonly EinkaufslistenService service = new(new KeineDatenbankFactory());

    [Theory]
    [InlineData("Milch", 1, "Liter", true)]
    [InlineData("", 1, "Liter", false)]            // Name ist Pflicht
    [InlineData("   ", 1, "Liter", false)]         // nur Leerzeichen
    [InlineData("Milch", 0, "Liter", false)]       // Menge zu klein
    [InlineData("Milch", -1, "Liter", false)]
    [InlineData("Milch", 10001, "Liter", false)]   // Menge zu groß
    [InlineData("Milch", 1, "", false)]            // Einheit ist Pflicht
    [InlineData("Milch", 1, "kg123", false)]       // nur Buchstaben und Punkte
    public void Artikel_wird_nach_den_Regeln_aus_VALIDIERUNG_geprueft(
        string name, decimal menge, string einheit, bool erwartetGueltig)
    {
        var artikel = new Artikel { Name = name, Menge = menge, Einheit = einheit };

        Assert.Equal(erwartetGueltig, service.IstGueltig(artikel, out _));
    }

    [Fact]
    public void Artikel_Name_mit_101_Zeichen_wird_abgelehnt()
    {
        var artikel = new Artikel { Name = new string('a', 101) };

        Assert.False(service.IstGueltig(artikel, out _));
    }

    [Fact]
    public void Stueck_verlangt_ganze_Menge_kg_nicht()
    {
        var stueck = new Artikel { Name = "Tomaten", Menge = 2.5m, Einheit = "Stück" };
        var kilo = new Artikel { Name = "Tomaten", Menge = 2.5m, Einheit = "kg" };

        Assert.False(service.IstGueltig(stueck, out _));
        Assert.True(service.IstGueltig(kilo, out _));
    }

    [Fact]
    public void Gericht_ohne_Zutat_ist_ungueltig()
    {
        var gericht = new Gericht { Name = "Pfannkuchen" };

        Assert.False(service.IstGueltig(gericht, out var fehler));
        Assert.Contains(fehler, f => f.ErrorMessage!.Contains("mindestens eine Zutat"));
    }

    [Fact]
    public void Gericht_mit_doppelter_Zutat_ist_ungueltig_auch_bei_anderer_Schreibweise()
    {
        var gericht = new Gericht
        {
            Name = "Pfannkuchen",
            GerichtZutaten =
            [
                new GerichtZutat { Zutat = new Zutat { Name = "Mehl" } },
                new GerichtZutat { Zutat = new Zutat { Name = "mehl" } }
            ]
        };

        Assert.False(service.IstGueltig(gericht, out var fehler));
        Assert.Contains(fehler, f => f.ErrorMessage!.Contains("nicht doppelt"));
    }

    [Fact]
    public void SindGueltig_sammelt_die_Fehler_aller_Modelle()
    {
        object[] modelle =
        [
            new Artikel { Name = "", Einheit = "Stück" },
            new Kategorie { Name = "Dessert 1" }       // Zahl in Kategorie nicht erlaubt
        ];

        Assert.False(service.SindGueltig(modelle, out var fehler));
        Assert.True(fehler.Count >= 2);
    }

    [Fact]
    public async Task Ungueltige_Eingabe_erreicht_die_Datenbank_nie()
    {
        // KeineDatenbankFactory wirft bei jedem Zugriff. Die Validierung muss also vorher abbrechen.
        SpeicherErgebnis ergebnis = await service.ArtikelHinzufuegenAsync("Milch", 0, "Liter", "");

        Assert.False(ergebnis.Erfolg);
        Assert.False(string.IsNullOrWhiteSpace(ergebnis.Fehler));
    }
}
