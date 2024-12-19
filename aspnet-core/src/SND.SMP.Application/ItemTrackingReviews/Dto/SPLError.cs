public class SPLError
{
    public string Type { get; set; }
    public string Detail { get; set; }
    public string Title { get; set; }
    public string Instance { get; set; } // Nullable field
    public int Status { get; set; }
    public string Code { get; set; }
    public string TargetSite { get; set; } // Nullable field
    public string Message { get; set; }
    public object Data { get; set; } // Use object for a flexible type
    public string InnerException { get; set; } // Nullable field
    public string HelpLink { get; set; } // Nullable field
    public string Source { get; set; } // Nullable field
    public int HResult { get; set; }
    public string StackTrace { get; set; } // Nullable field
}