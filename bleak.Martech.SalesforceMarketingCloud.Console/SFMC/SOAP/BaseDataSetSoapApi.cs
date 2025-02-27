using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public abstract class BaseDataSetSoapApi<TAPIObject, TPoco> : BaseSoapApi
        where TAPIObject : APIObject
        where TPoco : IPoco
    {
        protected string RequestID { get; set; } = string.Empty;

        public List<Wsdl.APIObject> WsdlObjects { get; set; } = new List<Wsdl.APIObject>();
        public BaseDataSetSoapApi(AuthRepository authRepository) : base(authRepository)
        {
        }
        
        public List<TPoco> LoadDataSet()
        {
            LoadPage();
            
            if (WsdlObjects.Any())
            {
                List<TPoco> pocos = new List<TPoco>();
                pocos.AddRange(WsdlObjects.Select(x => ConvertToPoco((TAPIObject)x)));
                return pocos;
            }

            throw new Exception("Error Loading Folders");
        }

        private void LoadPage()
        {
            try
            {
                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Invoking SOAP Call. URL: {url}"); }

                var request = BuildRequest();
                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"[{this.GetType().Name}] Request: {request}"); }
                var results = _restManager.ExecuteRestMethod<SoapEnvelope<TAPIObject>, string>(
                    uri: new Uri(url),
                    verb: HttpVerbs.POST,
                    serializedPayload: request,
                    headers: BuildHeaders()
                );

                if (AppConfiguration.Instance.Debug) Console.WriteLine($"[{this.GetType().Name}] results.Value = {results?.Results}");
                if (results?.Error != null) Console.WriteLine($"[{this.GetType().Name}] results.Error = {results.Error}");

                // Process Results
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"Overall Status: {results!.Results.Body.RetrieveResponse.OverallStatus}");
                int currentPageSize = 0;
                foreach (var result in results!.Results.Body.RetrieveResponse.Results)
                {
                    WsdlObjects.Add(result);
                    currentPageSize++;
                }
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"[{this.GetType().Name}] Current Page had {currentPageSize} records. There are now {WsdlObjects.Count()} Total Objects Received.");

                if (results.Results.Body.RetrieveResponse.OverallStatus == "MoreDataAvailable")
                {
                    Console.WriteLine($"[{this.GetType().Name}] More Data Is Available. Request ID: {results.Results.Body.RetrieveResponse.RequestID}");
                    LoadPage();
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}");
                throw;
            }
        }

        public abstract TPoco ConvertToPoco(TAPIObject wsdlObject);

        public abstract string BuildRequest();
    }
}