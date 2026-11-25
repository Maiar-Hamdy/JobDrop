namespace JobDrop.Domain.Services;

/// <summary>
/// Pure projection — never returns PII. Generates #JD-XXXX from a stable GUID hash.
/// </summary>
public static class BlindDossierProjector
{
    public static string CandidateCode(Guid candidateProfileId)
    {
        // Stable 4-char base36 from GUID bytes
        var bytes = candidateProfileId.ToByteArray();
        var val = BitConverter.ToUInt16(bytes, 0) % 10000;
        return $"#JD-{val:D4}";
    }

    public static string MaskName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "Anonymous";
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][..1] + ".";
        return $"{parts[0]} {parts[^1][..1]}.";
    }
}