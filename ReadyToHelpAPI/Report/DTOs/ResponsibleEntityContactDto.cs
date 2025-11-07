namespace readytohelpapi.Report.DTOs;

/// <summary>
/// Data transfer object for responsible entity contact details.
/// </summary>
public class ResponsibleEntityContactDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int ContactPhone { get; set; }
}
