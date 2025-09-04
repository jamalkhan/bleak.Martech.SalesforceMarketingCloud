namespace bleak.Martech.SalesforceMarketingCloud.Models.Sfmc.Dtos.Soap;

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName="RetrieveResponseMsg", WrapperNamespace="http://exacttarget.com/wsdl/partnerAPI", IsWrapped=true)]
public partial class RetrieveResponse<T>
    where T :  class, new()
{

    [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://exacttarget.com/wsdl/partnerAPI", Order = 0)]
    public string OverallStatus { get; set; } = string.Empty;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://exacttarget.com/wsdl/partnerAPI", Order=1)]
    public string RequestID { get; set; } = string.Empty;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://exacttarget.com/wsdl/partnerAPI", Order=2)]
    [System.Xml.Serialization.XmlElementAttribute("Results")]
    public T[] Results{ get; set; } = Array.Empty<T>();
    
    public RetrieveResponse()
    {
    }
    
    public RetrieveResponse(string OverallStatus, string RequestID, T[] Results)
    {
        this.OverallStatus = OverallStatus;
        this.RequestID = RequestID;
        this.Results = Results;
    }
}
