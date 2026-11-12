using System.ComponentModel.DataAnnotations;

namespace einkaufslisterichtig.Models;

/// <summary>
/// Eine Zutat gibt es nur einmal (eindeutiger Name). Mehrere Gerichte können dieselbe Zutat verwenden (n:m über <see cref="GerichtZutat"/>).
/// </summary>
public class Zutat
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte gib eine Zutat ein.")]
    [StringLength(100, ErrorMessage = "Die Zutat darf höchstens 100 Zeichen lang sein.")]
    [RegularExpression(@"^(?=.*[\p{L}\d]).+$", ErrorMessage = "Die Zutat muss Buchstaben oder Zahlen enthalten.")]
    public string Name { get; set; } = string.Empty;

    public List<GerichtZutat> GerichtZutaten { get; set; } = [];
}
