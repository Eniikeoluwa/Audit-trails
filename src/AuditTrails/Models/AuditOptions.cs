namespace AuditTrails.Models;

public class AuditOptions
{
    public bool IncludeStackTrace { get; set; } = false;
    public int MaxDescriptionLength { get; set; } = 2000;
}

