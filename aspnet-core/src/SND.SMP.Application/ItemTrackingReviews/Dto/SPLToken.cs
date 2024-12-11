public class SPLTokenRequest
{
    public string userName { get; set; }
    public string password { get; set; }
}

public class SPLTokenResponse
{
    [JsonProperty("access_token")]
    public string token { get; set; }

    [JsonProperty(".expires")]
    public string expires { get; set; }
}