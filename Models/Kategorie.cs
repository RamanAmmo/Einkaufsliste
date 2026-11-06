using System.ComponentModel.DataAnnotations;

namespace einkaufslisterichtig.Models;

/// <summary>Kategorie eines Gerichts, z. B. "Hauptgericht". Ein Gericht gehört genau einer Kategorie an (1:n).</summary>
public class Kategorie
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte wähle eine Kategorie für das Gericht aus.")]
    [StringLength(30, ErrorMessage = "Die Kategorie darf höchstens 30 Zeichen lang sein.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Die Kategorie darf nur Buchstaben und Leerzeichen enthalten.")]
    public string Name { get; set; } = string.Empty;

    public List<Gericht> Gerichte { get; set; } = [];
}
