using System;
using System.Collections.Generic;

public class EPTracking
{
    public string TrackingNumber { get; set; }
    public string TrackingReferenceNo { get; set; }
    public T_Sender Sender { get; set; }
    public T_Receiver Receiver { get; set; }
    public T_LastStatus LastStatus { get; set; }
    public List<T_Event> Events { get; set; }
    public T_Weight Weight { get; set; }
}

public class T_Sender
{
    public string Name { get; set; }
    public string ContactNumber { get; set; }
}

public class T_Receiver
{
    public string Name { get; set; }
    public string ContactNumber { get; set; }
}

public class T_LastStatus
{
    public string Code { get; set; }
    public string DescriptionAr { get; set; }
    public string DescriptionEn { get; set; }
}

public class T_Event
{
    public string TimeStamp { get; set; }
    public T_Status Status { get; set; }
    public string LocationAr { get; set; }
    public string LocationEn { get; set; }
    public string Url { get; set; }
}

public class T_Status
{
    public string Code { get; set; }
    public string DescriptionAr { get; set; }
    public string DescriptionEn { get; set; }
}

public class T_Weight
{
    public int Value { get; set; }
    public string Unit { get; set; }
}