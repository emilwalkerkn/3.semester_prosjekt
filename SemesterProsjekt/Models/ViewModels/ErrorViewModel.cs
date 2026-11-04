namespace SemesterProsjekt.Models;

/// <summary>
/// Data til feilsiden (Views/Shared/Error.cshtml).
/// </summary>
public class ErrorViewModel
{
    // ID-en til forespørselen som feilet, nyttig ved feilsøking i loggene.
    public string? RequestId { get; set; }

    // Feilsiden viser bare forespørsels-ID-en hvis den faktisk finnes.
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
