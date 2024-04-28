using Microsoft.AspNetCore.Http;

public class UploadPostCheck
{
    public IFormFile? file { get; set; }
    public string dispatchNo { get; set; }

}