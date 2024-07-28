using System;

public class StageResult
{
    public DateTime DateStage { get; set; }
    public string Description { get; set; }

    public StageResult(DateTime dateStage, string description)
    {
        DateStage = dateStage;
        Description = description;
    }
}
