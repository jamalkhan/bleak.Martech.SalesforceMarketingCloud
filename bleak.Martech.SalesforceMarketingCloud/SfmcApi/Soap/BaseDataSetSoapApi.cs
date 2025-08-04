using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using System.Net;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;

namespace bleak.Martech.SalesforceMarketingCloud.Api.Soap;

public abstract class BaseDataSetSoapApi<TAPIObject, TPoco> : BaseSoapApi
    where TAPIObject : APIObject
    where TPoco : IPoco
{
    protected string RequestID { get; set; } = string.Empty;
    protected long RunningTally { get; set; } = 0;
    protected IFileWriter FileWriter { get; set; }

    public BaseDataSetSoapApi(IAuthRepository authRepository, IFileWriter fileWriter, SfmcConnectionConfiguration config)
        : base(authRepository, config)
    {
        FileWriter = fileWriter;
    }
    
    public void LoadDataSet(string filePath)
    {
        string? status = "";
        do
        {
            try
            {
                
                if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Invoking SOAP Call. URL: {url}");
                RestResults<SoapEnvelope<TAPIObject>, string> results = 
                    ExecuteWithReauth
                    (
                        apiCall: () => _restManager.ExecuteRestMethod<SoapEnvelope<TAPIObject>, string>(
                            uri: new Uri(url),
                            verb: HttpVerbs.POST,
                            serializedPayload: BuildRequest(),
                            headers: BuildHeaders()
                        ),
                        errorCondition: HandleError,
                        reauthenticate: () => _authRepository.ResolveAuthentication()
                    );

                if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] results.Value = {results?.Results}");
                if (results?.Error != null) Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] results.Error = {results.Error}");

                status = results?.Results?.Body?.RetrieveResponse?.OverallStatus;

                // Process Results
                if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Overall Status: {results!.Results.Body.RetrieveResponse.OverallStatus}");
                int currentPageSize = 0;
                if (results!.Results.Body.RetrieveResponse.Results.Any())
                {
                    var wsdlObjects = results!.Results.Body.RetrieveResponse.Results;
                    List<TPoco> pocos = new List<TPoco>();
                    pocos.AddRange(wsdlObjects.Select(x => ConvertToPoco((TAPIObject)x)));
                    FileWriter.WriteToFile(filePath, pocos);
                    

                    currentPageSize = wsdlObjects.Count();
                    RunningTally += currentPageSize;

                    pocos.Clear();
                    pocos.TrimExcess();

                    
                    if (status == "MoreDataAvailable")
                    {
                        Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] More Data Available. Added: {currentPageSize} records; Total: {RunningTally}; Request ID: {results.Results.Body.RetrieveResponse.RequestID}");
                    }
                    else
                    {
                        Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Current Page: {currentPageSize} records. Total: {RunningTally} Request ID: {results.Results.Body.RetrieveResponse.RequestID}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}");
                status = $"Error - {ex.Message}";
            }
        }
        while (status == "MoreDataAvailable");
    }

    private bool HandleError(RestResults<SoapEnvelope<TAPIObject>, string> results)
    {
        if (results == null)
        {
            Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error: results is null.");
            return true;
        }
        if (results.Results == null)
        {
            Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error: results.Results is null.");
            return true;
        }
        if (results.Results.Body == null)
        {
            Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error: results.Results.Body is null.");
            return true;
        }
        if (results.Results.Body.RetrieveResponse == null)
        {
            Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error: results.Results.Body.RetrieveResponse is null.");
            return true;
        }

        if (results.Results.Body.RetrieveResponse.OverallStatus == "Error" || results?.UnhandledError?.Contains("401") == true)
        {
            Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] RetrieveResponse.OverallStatus: {results.Results.Body.RetrieveResponse.OverallStatus}.");
            return true;
        }
        else
        {
            return false;
        }
    }

    private RestResults<TResponse, string> ExecuteWithReauth<TResponse>
    (
        Func<RestResults<TResponse, string>> apiCall,
        Func<RestResults<TResponse, string>, bool> errorCondition,
        Action reauthenticate
    )
    {
        var results = apiCall();
        
        if (errorCondition(results))
        {
            Console.WriteLine($"Unauthenticated: {results.UnhandledError}");
            reauthenticate();
            Console.WriteLine($"Reauthenticated!");
            results = apiCall();
        }
        
        return results;
    }

    public abstract TPoco ConvertToPoco(TAPIObject wsdlObject);

    public abstract string BuildRequest();
}
