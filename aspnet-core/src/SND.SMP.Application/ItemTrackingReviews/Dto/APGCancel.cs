using System.Collections.Generic;

public class ItemPackageRequest
{
    public string tracking { get; set; }
}

public class PackagesRequestCollection
{
    public List<ItemPackageRequest> packages { get; set; }
}

public class ItemPackageResponse
{
    public string tracking { get; set; }
    public string registeredCode { get; set; }
    public string status { get; set; }
}

public class PackagesResponseCollection
{
    public List<ItemPackageResponse> packages { get; set; }
}
