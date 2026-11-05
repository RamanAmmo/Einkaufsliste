using System.ComponentModel.DataAnnotations;

namespace einkaufslisterichtig.Models;

[AttributeUsage(AttributeTargets.Property)]
public sealed class KeineDoppeltenZutatenAttribute : ValidationAttribute
{
    public KeineDoppeltenZutatenAttribute()
        : base("Ein Gericht darf dieselbe Zutat nicht doppelt enthalten.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IEnumerable<string> zutaten)
        {
            return ValidationResult.Success;
        }

        string[] bereinigteZutaten = zutaten
            .Where(zutat => !string.IsNullOrWhiteSpace(zutat))
            .Select(zutat => zutat.Trim())
            .ToArray();

        return bereinigteZutaten.Length == bereinigteZutaten.Distinct(StringComparer.OrdinalIgnoreCase).Count()
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage, [validationContext.MemberName!]);
    }
}
