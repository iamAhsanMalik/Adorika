namespace Adorika.ServiceDefaults.Scalar;

public class ScalarUiSettings
{
    public const string SectionName = nameof(ScalarUiSettings);
    public string? Title { get; set; }
    public string? EndpointPrefix { get; set; }
    public bool ExpandAllTagsByDefault { get; set; }
    public bool HideModelsSection { get; set; }
    public bool RedirectToDocumentation { get; set; }
    public bool EnablePersistentAuthentication { get; set; }
}