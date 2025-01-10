using System;

public class SPLTokenRequest
{
    public string userName { get; set; }
    public string password { get; set; }
}

public class Token
{
    public string Access_Token { get; set; }
    public long Expires_In { get; set; }
    public string Token_Type { get; set; }
    public string Refresh_Token { get; set; }
}

public class SPLTokenResponse
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTime LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public string UserTypeCode { get; set; }
    public Token Token { get; set; }
    public string ExternalCode { get; set; }
    public string BranchCode { get; set; }
    public string CustomerCode { get; set; }
    public string TimeZone { get; set; }
}