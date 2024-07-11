using System.Collections.Generic;

public class PreAlertSuccessWithErrorsEmail
{
    public string customerCode { get; set; }
    public string dispatchNo { get; set; }
    public decimal totalWeight { get; set; }
    public int totalBags { get; set; }
    public string avgItemValue { get; set; }
    public List<DispatchValidateDto> validations { get; set; }
}

