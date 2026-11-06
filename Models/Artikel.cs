using System.ComponentModel.DataAnnotations;

namespace einkaufslisterichtig.Models;

public class Artikel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte gib einen Artikel- oder Gerichtnamen ein.")]
    [StringLength(100, ErrorMessage = "Der Name darf höchstens 100 Zeichen lang sein.")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 10000, ErrorMessage = "Die Menge muss zwischen 0,01 und 10.000 liegen.")]
    public decimal Menge { get; set; } = 1;

    [Required(ErrorMessage = "Bitte gib eine Einheit an.")]
    [StringLength(20, ErrorMessage = "Die Einheit darf höchstens 20 Zeichen lang sein.")]
    [RegularExpression(@"^[\p{L}\.]+$", ErrorMessage = "Die Einheit darf nur Buchstaben und Punkte enthalten.")]
    public string Einheit { get; set; } = "Stück";

    [StringLength(200, ErrorMessage = "Die Notiz darf höchstens 200 Zeichen lang sein.")]
    public string Notiz { get; set; } = string.Empty;

    public bool Erledigt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.Equals(Einheit?.Trim(), "Stück", StringComparison.OrdinalIgnoreCase)
            && Menge != decimal.Truncate(Menge))
        {
            yield return new ValidationResult(
                "Bei der Einheit „Stück“ muss die Menge eine ganze Zahl sein.",
                [nameof(Menge), nameof(Einheit)]);
        }
    }
}
