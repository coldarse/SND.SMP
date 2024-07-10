using System.Collections.Generic;

public class Stage4Update
{
    public string DispatchNo { get; set; }
    public List<CountryWithAirport> CountryWithAirports { get; set; }
}

public class CountryWithAirport
{
    public string Country { get; set; }
    public string Airport { get; set; }
}