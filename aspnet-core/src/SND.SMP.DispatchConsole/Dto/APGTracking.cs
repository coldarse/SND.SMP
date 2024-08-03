using System.Collections.Generic;

public class APGTracking
{
    public string response { get; set; }
    public PackageItem package { get; set; }
    public List<Status> status { get; set; }
}

public class PackageItem
{
    public string tracking { get; set; }
    public string registeredCode { get; set; }
    public string stamp { get; set; }
}

public class Status
{
    public string code { get; set; }
    public string statusName { get; set; }
    public string description { get; set; }
    public string statusDate { get; set; }
    public string location { get; set; }
}

