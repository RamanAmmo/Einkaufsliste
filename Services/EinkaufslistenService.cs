using System.ComponentModel.DataAnnotations;
using einkaufslisterichtig.Models;

namespace einkaufslisterichtig.Services;

public sealed class EinkaufslistenService
{
    public bool KannSpeichern(IEnumerable<object> modelle, out List<ValidationResult> fehler)
    {
        return SindGueltig(modelle, out fehler);
    }

    public bool IstGueltig(object model, out List<ValidationResult> fehler)
    {
        fehler = [];
        return Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            fehler,
            validateAllProperties: true);
    }

    public bool SindGueltig(IEnumerable<object> modelle, out List<ValidationResult> fehler)
    {
        fehler = [];
        foreach (object model in modelle)
        {
            if (!IstGueltig(model, out List<ValidationResult> modellfehler))
            {
                fehler.AddRange(modellfehler);
            }
        }

        return fehler.Count == 0;
    }
}
