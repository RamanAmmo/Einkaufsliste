using System.ComponentModel.DataAnnotations;

namespace einkaufslisterichtig.Models;

public sealed class GerichtEingabe
{
    [Required(ErrorMessage = "Bitte gib einen Namen für das Gericht ein.")]
    [StringLength(80, ErrorMessage = "Der Gerichtname darf höchstens 80 Zeichen lang sein.")]
    [RegularExpression(@"^(?=.*[\p{L}\d]).+$", ErrorMessage = "Der Gerichtname muss Buchstaben oder Zahlen enthalten.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte wähle eine Kategorie für das Gericht aus.")]
    [StringLength(30, ErrorMessage = "Die Kategorie darf höchstens 30 Zeichen lang sein.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Die Kategorie darf nur Buchstaben und Leerzeichen enthalten.")]
    public string Kategorie { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib mindestens eine Zutat ein.")]
    [StringLength(100, ErrorMessage = "Die Zutat darf höchstens 100 Zeichen lang sein.")]
    [RegularExpression(@"^(?=.*[\p{L}\d]).+$", ErrorMessage = "Die Zutat muss Buchstaben oder Zahlen enthalten.")]
    public string Zutat { get; set; } = string.Empty;
}
