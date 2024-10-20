using Newtonsoft.Json;

public class SATokenRequest
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string grant_type { get; set; }
}

public class SATokenResponse
{
    [JsonProperty("access_token")]
    public string token { get; set; }

    [JsonProperty(".expires")]
    public string expires { get; set; }
}
