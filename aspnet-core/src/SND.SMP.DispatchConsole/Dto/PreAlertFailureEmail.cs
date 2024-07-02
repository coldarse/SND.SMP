using System.Collections.Generic;
using SND.SMP.DispatchConsole.Dto;

public class PreAlertFailureEmail
{
    public string customerCode { get; set; }
    public string dispatchNo { get; set; }
    public List<DispatchValidateDto> validations { get; set; }
}
