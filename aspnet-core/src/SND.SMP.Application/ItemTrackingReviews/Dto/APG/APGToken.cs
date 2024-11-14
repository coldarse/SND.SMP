public class APGTokenRequest
{
    public string userName { get; set; }
    public string passWord { get; set; }
}

public class APGTokenResponse
{
    public Token token { get; set; }
    public string message { get; set; }
}

public class Token
{
    public string token { get; set; }
    public string expiration { get; set; }
}