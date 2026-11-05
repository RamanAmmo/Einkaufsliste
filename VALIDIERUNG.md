# Validierung

Alle Formulare verwenden `EditForm`, `DataAnnotationsValidator` und eine
`ValidationMessage`-Meldung pro Eingabefeld. Vor dem Speichern prüft
`EinkaufslistenService.KannSpeichern` die Modelle erneut. Bei einem Fehler wird
der LocalStorage nicht verändert.

| Feld | Regel | Fachliche Begründung | Testfall |
|---|---|---|---|
| Artikel- oder Gerichtname | Pflichtfeld, maximal 100 Zeichen | Jeder Einkaufslisteneintrag muss identifizierbar sein. | Leer, nur Leerzeichen und 101 Zeichen werden abgelehnt. |
| Menge | Wertebereich 0,01 bis 10.000 | Null oder negative Mengen sind für einen Einkauf nicht sinnvoll; 10.000 verhindert Fehlbedienung. | 0, -1 und 10.001 werden abgelehnt. |
| Einheit | Pflichtfeld, maximal 20 Zeichen, nur Buchstaben und Punkte | Eine Einheit wie „Stück“, „kg“ oder „Liter“ macht die Menge verständlich. | Leer, „kg123“ und 21 Zeichen werden abgelehnt. |
| Notiz | Optional, maximal 200 Zeichen | Ein kurzer Hinweis wie „Vollmilch“ ist nützlich, aber unbegrenzt großer Text nicht. | 201 Zeichen werden abgelehnt. |
| Gerichtname | Pflichtfeld, maximal 80 Zeichen, Buchstabe oder Zahl erforderlich | Ein Gericht braucht einen verständlichen Namen für Auswahl und Speicherung. | Leer, nur Sonderzeichen und 81 Zeichen werden abgelehnt. |
| Kategorie | Pflichtfeld, maximal 30 Zeichen, nur Buchstaben und Leerzeichen | Kategorien wie „Hauptgericht“ erleichtern die Übersicht über mehrere Gerichte. | Leer, Zahlen, Sonderzeichen und 31 Zeichen werden abgelehnt. |
| Erste Zutat | Pflichtfeld, maximal 100 Zeichen, Buchstabe oder Zahl erforderlich | Ein Gericht ohne Zutat kann später nicht sinnvoll übernommen werden. | Leer, nur Sonderzeichen und 101 Zeichen werden abgelehnt. |
| Weitere Zutat | Pflichtfeld, maximal 100 Zeichen, Buchstabe oder Zahl erforderlich | Auch nachträgliche Zutaten müssen als gültige Einträge gespeichert werden. | Leer, nur Sonderzeichen und 101 Zeichen werden abgelehnt. |
| Zutaten eines Gerichts | Eigenes `ValidationAttribute` gegen Duplikate | Dieselbe Zutat soll beim späteren Übernehmen nicht mehrfach auf der Einkaufsliste landen. | „Mehl“ und „mehl“ im selben Gericht werden abgelehnt. |
| Menge und Einheit | `IValidatableObject`: Bei „Stück“ muss die Menge ganzzahlig sein | 2,5 Stück ist fachlich unklar, während 2,5 kg sinnvoll sein kann. | 2,5 Stück wird abgelehnt; 2,5 kg wird akzeptiert. |
| Gespeicherte Modelle | Service prüft alle Modelle vor dem Speichern | Ungültige Daten dürfen auch bei einem direkten Service-Aufruf nicht gespeichert werden. | Ein ungültiger Artikel oder ein ungültiges Gericht führt zu keinem Speichervorgang. |
| Zutaten aus mehreren Gerichten | Gleiche Zutat und gleiche Einheit werden zusammengeführt und mengenmäßig addiert | Werden beispielsweise für zwei Gerichte Tomaten benötigt, soll die Einkaufsliste die gesamte benötigte Menge anzeigen statt einen Bedarf zu verlieren. Unterschiedliche Einheiten werden nicht ohne Umrechnung vermischt. | 2 Stück Tomaten plus 3 Stück Tomaten ergeben 5 Stück Tomaten; 2 kg und 3 Stück bleiben getrennte Einträge. |

Eine Datenbank und damit ein Datenbank-Unique-Index existieren in dieser
Projektstufe noch nicht. Die lokale Prüfung auf doppelte Gerichtnamen und
Zutaten verhindert bereits die fachlich relevanten Duplikate.
