using System.ComponentModel.DataAnnotations;

public sealed class GuidewireOptions
{
    public const string SectionName = "Guidewire";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;
}
