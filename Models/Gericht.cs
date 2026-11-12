using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace einkaufslisterichtig.Models;

public class Gericht : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte gib einen Namen für das Gericht ein.")]
    [StringLength(80, ErrorMessage = "Der Gerichtname darf höchstens 80 Zeichen lang sein.")]
    [RegularExpression(@"^(?=.*[\p{L}\d]).+$", ErrorMessage = "Der Gerichtname muss Buchstaben oder Zahlen enthalten.")]
    public string Name { get; set; } = string.Empty;

    public int KategorieId { get; set; }
    public Kategorie? Kategorie { get; set; }

    public List<GerichtZutat> GerichtZutaten { get; set; } = [];

    /// <summary>Namen der Zutaten (nicht in der Datenbank gespeichert, nur für die Validierung).</summary>
    [NotMapped]
    [KeineDoppeltenZutaten]
    public IReadOnlyList<string> ZutatNamen =>
        GerichtZutaten.Select(gz => gz.Zutat?.Name ?? string.Empty).ToList();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield break;
        }

        if (GerichtZutaten.Count == 0)
        {
            yield return new ValidationResult(
                "Ein Gericht muss mindestens eine Zutat enthalten.",
                [nameof(GerichtZutaten)]);
        }
    }
}
