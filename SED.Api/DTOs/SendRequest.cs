namespace SED.Api.DTOs;

public class SendRequest
{
    public string RouteType { get; set; } = "Sequential";
    public List<ApproverDto> Approvers { get; set; } = new();
}