using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Fileops;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public abstract class BaseDataSetSoapApi<TAPIObject, TPoco> : BaseSoapApi
        where TAPIObject : APIObject
        where TPoco : IPoco
    {
        protected string RequestID { get; set; } = string.Empty;
        protected long RunningTally { get; set; } = 0;
        protected IFileWriter FileWriter { get; set; }

        public BaseDataSetSoapApi(AuthRepository authRepository, IFileWriter fileWriter) : base(authRepository)
        {
            FileWriter = fileWriter;
        }
        
        public void LoadDataSet()
        {
            try
            {
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Invoking SOAP Call. URL: {url}");
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

                if (AppConfiguration.Instance.Debug) Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] results.Value = {results?.Results}");
                if (results?.Error != null) Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] results.Error = {results.Error}");

                // Process Results
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Overall Status: {results!.Results.Body.RetrieveResponse.OverallStatus}");
                int currentPageSize = 0;
                if (results!.Results.Body.RetrieveResponse.Results.Any())
                {
                    var wsdlObjects = results!.Results.Body.RetrieveResponse.Results;
                    List<TPoco> pocos = new List<TPoco>();
                    pocos.AddRange(wsdlObjects.Select(x => ConvertToPoco((TAPIObject)x)));
                    FileWriter.WriteToFile(pocos);
                    

                    currentPageSize = wsdlObjects.Count();
                    RunningTally += currentPageSize;

                    pocos.Clear();
                    pocos.TrimExcess();

                    if (results.Results.Body.RetrieveResponse.OverallStatus == "MoreDataAvailable")
                    {
                        Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] More Data Available. Current Page: {currentPageSize} records; Running Total: {RunningTally}; Request ID: {results.Results.Body.RetrieveResponse.RequestID}");
                        LoadDataSet();
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
                throw;
            }
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
}