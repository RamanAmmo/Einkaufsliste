using System.ComponentModel.DataAnnotations;

namespace einkaufslisterichtig.Models;

public sealed class ZutatEingabe
{
    [Required(ErrorMessage = "Bitte gib eine Zutat ein.")]
    [StringLength(100, ErrorMessage = "Die Zutat darf höchstens 100 Zeichen lang sein.")]
    [RegularExpression(@"^(?=.*[\p{L}\d]).+$", ErrorMessage = "Die Zutat muss Buchstaben oder Zahlen enthalten.")]
    public string Name { get; set; } = string.Empty;
}
