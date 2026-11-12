namespace einkaufslisterichtig.Models;

/// <summary>
/// Verbindungstabelle der n:m-Beziehung zwischen <see cref="Gericht"/> und <see cref="Zutat"/>.
/// Der zusammengesetzte Primärschlüssel (GerichtId, ZutatId) verhindert, dass dieselbe Zutat
/// zweimal im selben Gericht steht.
/// </summary>
public class GerichtZutat
{
    public int GerichtId { get; set; }
    public Gericht? Gericht { get; set; }

    public int ZutatId { get; set; }
    public Zutat? Zutat { get; set; }
}
