using einkaufslisterichtig.Models;
using Microsoft.EntityFrameworkCore;

namespace einkaufslisterichtig.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Artikel> Artikel => Set<Artikel>();
    public DbSet<Gericht> Gerichte => Set<Gericht>();
    public DbSet<Kategorie> Kategorien => Set<Kategorie>();
    public DbSet<Zutat> Zutaten => Set<Zutat>();
    public DbSet<GerichtZutat> GerichtZutaten => Set<GerichtZutat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // NOCASE: "Mehl" und "mehl" gelten als gleich. Das entspricht der bisherigen Validierung
        // (StringComparison.OrdinalIgnoreCase) und macht Eindeutigkeit und Suche unabhängig von der Schreibweise.
        modelBuilder.Entity<Kategorie>(e =>
        {
            e.Property(k => k.Name).IsRequired().HasMaxLength(30).UseCollation("NOCASE");
            e.HasIndex(k => k.Name).IsUnique();
        });

        modelBuilder.Entity<Zutat>(e =>
        {
            e.Property(z => z.Name).IsRequired().HasMaxLength(100).UseCollation("NOCASE");
            e.HasIndex(z => z.Name).IsUnique();
        });

        modelBuilder.Entity<Gericht>(e =>
        {
            e.Property(g => g.Name).IsRequired().HasMaxLength(80).UseCollation("NOCASE");
            e.HasIndex(g => g.Name).IsUnique();

            // Eine Kategorie mit Gerichten darf nicht gelöscht werden (Restrict).
            e.HasOne(g => g.Kategorie)
                .WithMany(k => k.Gerichte)
                .HasForeignKey(g => g.KategorieId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GerichtZutat>(e =>
        {
            // Zusammengesetzter Schlüssel: dieselbe Zutat kann in einem Gericht nur einmal vorkommen.
            e.HasKey(gz => new { gz.GerichtId, gz.ZutatId });

            // Gericht löschen -> seine Zuordnungen verschwinden mit.
            e.HasOne(gz => gz.Gericht)
                .WithMany(g => g.GerichtZutaten)
                .HasForeignKey(gz => gz.GerichtId)
                .OnDelete(DeleteBehavior.Cascade);

            // Eine Zutat, die in einem Gericht verwendet wird, darf nicht gelöscht werden.
            e.HasOne(gz => gz.Zutat)
                .WithMany(z => z.GerichtZutaten)
                .HasForeignKey(gz => gz.ZutatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Artikel>(e =>
        {
            e.Property(a => a.Name).IsRequired().HasMaxLength(100).UseCollation("NOCASE");
            e.Property(a => a.Einheit).IsRequired().HasMaxLength(20).UseCollation("NOCASE");
            e.Property(a => a.Notiz).IsRequired().HasMaxLength(200).HasDefaultValue(string.Empty);
            e.Property(a => a.Menge).HasPrecision(18, 2);

            // Gleicher Artikel mit gleicher Einheit steht nur einmal auf der Liste (Mengen werden addiert).
            e.HasIndex(a => new { a.Name, a.Einheit }).IsUnique();

            // SQLite speichert decimal als Text, deshalb wird für den Vergleich nach REAL gecastet.
            e.ToTable(t => t.HasCheckConstraint(
                "CK_Artikel_Menge",
                "CAST(\"Menge\" AS REAL) >= 0.01 AND CAST(\"Menge\" AS REAL) <= 10000"));
        });

        Seed(modelBuilder);
    }

    private static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kategorie>().HasData(
            new Kategorie { Id = 1, Name = "Hauptgericht" },
            new Kategorie { Id = 2, Name = "Dessert" },
            new Kategorie { Id = 3, Name = "Frühstück" });

        modelBuilder.Entity<Zutat>().HasData(
            new Zutat { Id = 1, Name = "Mehl" },
            new Zutat { Id = 2, Name = "Milch" },
            new Zutat { Id = 3, Name = "Eier" },
            new Zutat { Id = 4, Name = "Butter" },
            new Zutat { Id = 5, Name = "Spaghetti" },
            new Zutat { Id = 6, Name = "Hackfleisch" },
            new Zutat { Id = 7, Name = "Tomaten" },
            new Zutat { Id = 8, Name = "Zwiebeln" },
            new Zutat { Id = 9, Name = "Zucker" },
            new Zutat { Id = 10, Name = "Kakao" },
            new Zutat { Id = 11, Name = "Haferflocken" },
            new Zutat { Id = 12, Name = "Bananen" });

        modelBuilder.Entity<Gericht>().HasData(
            new Gericht { Id = 1, Name = "Pfannkuchen", KategorieId = 3 },
            new Gericht { Id = 2, Name = "Spaghetti Bolognese", KategorieId = 1 },
            new Gericht { Id = 3, Name = "Porridge", KategorieId = 3 },
            new Gericht { Id = 4, Name = "Schokopudding", KategorieId = 2 });

        // Milch (Zutat 2) kommt in drei Gerichten vor und zeigt damit die n:m-Beziehung.
        modelBuilder.Entity<GerichtZutat>().HasData(
            new GerichtZutat { GerichtId = 1, ZutatId = 1 },
            new GerichtZutat { GerichtId = 1, ZutatId = 2 },
            new GerichtZutat { GerichtId = 1, ZutatId = 3 },
            new GerichtZutat { GerichtId = 1, ZutatId = 4 },
            new GerichtZutat { GerichtId = 2, ZutatId = 5 },
            new GerichtZutat { GerichtId = 2, ZutatId = 6 },
            new GerichtZutat { GerichtId = 2, ZutatId = 7 },
            new GerichtZutat { GerichtId = 2, ZutatId = 8 },
            new GerichtZutat { GerichtId = 3, ZutatId = 11 },
            new GerichtZutat { GerichtId = 3, ZutatId = 2 },
            new GerichtZutat { GerichtId = 3, ZutatId = 12 },
            new GerichtZutat { GerichtId = 4, ZutatId = 2 },
            new GerichtZutat { GerichtId = 4, ZutatId = 9 },
            new GerichtZutat { GerichtId = 4, ZutatId = 10 });

        modelBuilder.Entity<Artikel>().HasData(
            new Artikel { Id = 1, Name = "Milch", Menge = 2, Einheit = "Liter", Notiz = "Vollmilch" },
            new Artikel { Id = 2, Name = "Brot", Menge = 1, Einheit = "Stück", Notiz = string.Empty },
            new Artikel { Id = 3, Name = "Äpfel", Menge = 6, Einheit = "Stück", Notiz = "Jonagold" });
    }
}
