using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace bleak.Martech.SalesforceMarketingCloud.Api.Soap;

public abstract class BaseDataSetSoapApi<TApiImplementation, TAPIObject, TPoco> : BaseSoapApi<TApiImplementation>
    where TAPIObject : APIObject
    where TPoco : IPoco
{
    protected string RequestID { get; set; } = string.Empty;
    protected long RunningTally { get; set; } = 0;
    protected IFileWriter FileWriter { get; set; }

    public BaseDataSetSoapApi
    (
        IRestClientAsync restClientAsync,
        IAuthRepository authRepository,
        IFileWriter fileWriter,
        SfmcConnectionConfiguration config,
        ILogger<TApiImplementation> logger
    )
        : base
        (
            restClientAsync: restClientAsync,
            authRepository: authRepository,
            sfmcConnectionConfiguration: config,
            logger: logger
        )
    {
        FileWriter = fileWriter;
    }
    
   public async Task LoadDataSetAsync(string filePath)
    {
        string? status = "";

        do
        {
            try
            {
                _logger.LogInformation(
                    $"[{GetType().Name} {DateTime.Now:yyyy-MM-dd HH:mm:ss}] Invoking SOAP Call. URL: {url}");

                RestResults<SoapEnvelope<TAPIObject>, string> results =
                    await ExecuteWithReauthAsync(
                        apiCallAsync: () => _restClientAsync.ExecuteRestMethodAsync<SoapEnvelope<TAPIObject>, string>(
                            uri: new Uri(url),
                            verb: HttpVerbs.POST,
                            serializedPayload: BuildRequest(),
                            headers: BuildHeaders()
                        ),
                        errorConditionAsync: HandleError,
                        reauthenticateAsync: _authRepository.ResolveAuthenticationAsync
                    ).ConfigureAwait(false);

                _logger.LogInformation(
                    $"[{GetType().Name} {DateTime.Now:yyyy-MM-dd HH:mm:ss}] results.Value = {results?.Results}");

                if (results?.Error != null)
                {
                    _logger.LogError(
                        $"[{GetType().Name} {DateTime.Now:yyyy-MM-dd HH:mm:ss}] results.Error = {results.Error}");
                }

                status = results?.Results?.Body?.RetrieveResponse?.OverallStatus;

                // Process Results
                _logger.LogInformation(
                    $"[{GetType().Name} {DateTime.Now:yyyy-MM-dd HH:mm:ss}] Overall Status: {results!.Results.Body.RetrieveResponse.OverallStatus}");

                int currentPageSize = 0;

                if (results!.Results.Body.RetrieveResponse.Results.Any())
                {
                    var wsdlObjects = results!.Results.Body.RetrieveResponse.Results;

                    // Convert to POCOs
                    List<TPoco> pocos = wsdlObjects
                        .Select(x => ConvertToPoco((TAPIObject)x))
                        .ToList();

                    await Task.Run(() => FileWriter.WriteToFile(filePath, pocos))
                    .ConfigureAwait(false);

                    currentPageSize = wsdlObjects.Count();
                    RunningTally += currentPageSize;

                    pocos.Clear();
                    pocos.TrimExcess();

                    if (status == "MoreDataAvailable")
                    {
                        _logger.LogInformation(
                            $"[{GetType().Name} {DateTime.Now:yyyy-MM-dd HH:mm:ss}] More Data Available. Added: {currentPageSize} records; Total: {RunningTally}; Request ID: {results.Results.Body.RetrieveResponse.RequestID}");
                    }
                    else
                    {
                        _logger.LogInformation(
                            $"[{GetType().Name} {DateTime.Now:yyyy-MM-dd HH:mm:ss}] Current Page: {currentPageSize} records. Total: {RunningTally} Request ID: {results.Results.Body.RetrieveResponse.RequestID}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error {ex.Message}");
                status = $"Error - {ex.Message}";
            }

        } while (status == "MoreDataAvailable");
    }


    private bool HandleError(RestResults<SoapEnvelope<TAPIObject>, string> results)
    {
        if (results == null)
        {
            _logger.LogError($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error: results is null.");
            return true;
        }
        if (results.Results == null)
        {
            _logger.LogError($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error: results.Results is null.");
            return true;
        }
        if (results.Results.Body == null)
        {
            _logger.LogError($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error: results.Results.Body is null.");
            return true;
        }
        if (results.Results.Body.RetrieveResponse == null)
        {
            _logger.LogError($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error: results.Results.Body.RetrieveResponse is null.");
            return true;
        }

        if (results.Results.Body.RetrieveResponse.OverallStatus == "Error" || results?.UnhandledError?.Contains("401") == true)
        {
            _logger.LogInformation($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] RetrieveResponse.OverallStatus: {results.Results.Body.RetrieveResponse.OverallStatus}.");
            return true;
        }
        else
        {
            return false;
        }
    }

    private async Task<RestResults<TResponse, string>> ExecuteWithReauthAsync<TResponse>
    (
        Func<Task<RestResults<TResponse, string>>> apiCallAsync,
        Func<RestResults<TResponse, string>, bool> errorConditionAsync,
        Func<Task> reauthenticateAsync
    )
    {
        var results = await apiCallAsync().ConfigureAwait(false);

        if (errorConditionAsync(results))
        {
            _logger.LogInformation($"Unauthenticated: {results.UnhandledError}");
            await reauthenticateAsync().ConfigureAwait(false);
            _logger.LogInformation($"Reauthenticated!");
            results = await apiCallAsync().ConfigureAwait(false);
        }

        return results;
    }
    public abstract TPoco ConvertToPoco(TAPIObject wsdlObject);

    public abstract string BuildRequest();
}
