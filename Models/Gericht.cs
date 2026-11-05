
using System.ComponentModel.DataAnnotations;

namespace einkaufslisterichtig.Models;

public class Gericht : IValidatableObject
{
    [Required(ErrorMessage = "Bitte gib einen Namen für das Gericht ein.")]
    [StringLength(80, ErrorMessage = "Der Gerichtname darf höchstens 80 Zeichen lang sein.")]
    [RegularExpression(@"^(?=.*[\p{L}\d]).+$", ErrorMessage = "Der Gerichtname muss Buchstaben oder Zahlen enthalten.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte wähle eine Kategorie für das Gericht aus.")]
    [StringLength(30, ErrorMessage = "Die Kategorie darf höchstens 30 Zeichen lang sein.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Die Kategorie darf nur Buchstaben und Leerzeichen enthalten.")]
    public string Kategorie { get; set; } = string.Empty;

    [KeineDoppeltenZutaten]
    public List<string> Zutaten { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield break;
        }

        if (Zutaten.Count == 0)
        {
            yield return new ValidationResult(
                "Ein Gericht muss mindestens eine Zutat enthalten.",
                [nameof(Zutaten)]);
        }
    }
}
