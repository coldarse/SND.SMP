using SND.SMP.Chibis.Dto;


public class PreCheckRetryDto
{
    public ChibiUploadDto? UploadFile { get; set; }
    public PreCheckDetails Details { get; set; }
    public string path { get; set; }
    public string dispatchNo { get; set; }

}

public class PreCheckDto
{
    public ChibiUploadDto UploadFile { get; set; }
    public PreCheckDetails Details { get; set; }
}

public class PreCheckDetails
{
    public string DispatchNo { get; set; }
    public string AccNo { get; set; }
    public string PostalCode { get; set; }
    public string ServiceCode { get; set; }
    public string ProductCode { get; set; }
    public string DateDispatch { get; set; }
    private string _rateOptionId;
    public string RateOptionId
    {
        get { return _rateOptionId ?? ""; }
        set { _rateOptionId = value; }
    }
}