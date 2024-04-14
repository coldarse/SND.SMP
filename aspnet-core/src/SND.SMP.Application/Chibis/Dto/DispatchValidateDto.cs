using System.Collections.Generic;

public class DispatchValidateDto
{
    public string Category { get; set; }
    public List<string> ItemIds { get; set; } = [];
    public string Message { get; set; }
}