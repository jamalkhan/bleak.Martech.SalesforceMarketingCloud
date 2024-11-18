using bleak.Api.Rest;
using bleak.Api.Rest.Common;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp
{
    public static class Program
    {

    
        private static void Main(string[] args)
        {
            // See https://aka.ms/new-console-template for more information
            Console.WriteLine("Hello, World!");


            var serializer = new JsonSerializer();
            CoreRestManager rm = new CoreRestManager(serializer: serializer, deserializer: serializer);

            string tokenUri = "https://" + AppConfiguration.Instance.Subdomain + ".auth.marketingcloudapis.com/v2/token";
            Console.WriteLine($"Trying to authenticate to {tokenUri}");
            
            var authResults = rm.ExecuteRestMethod<AuthToken, string>(
                uri: new Uri(tokenUri), 
                verb: Api.Rest.Common.HttpVerbs.POST, 
                payload: new
                            { 
                                grant_type = "client_credentials", 
                                client_id = AppConfiguration.Instance.ClientId, 
                                client_secret = AppConfiguration.Instance.ClientSecret, 
                                account_id = AppConfiguration.Instance.MemberId 
                            },
                headers: new List<Header>() { new Header() { Name= "Content-Type", Value="application/json" } }
                );

            //File.WriteAllBytes("temp.token", authResults.Results.)
            Console.WriteLine($"authResults.Value = { authResults.Results}");
            Console.WriteLine($"authResults.Error = { authResults.Error}");


            var data = new { Name = "Example", Value = 123 };
            string json = JsonSerializer.Serialize(data);

            // Combine AppContext.BaseDirectory with the desired file name
            string filePath = Path.Combine(AppContext.BaseDirectory, "output.json");

            // Write JSON content to the file
            File.WriteAllText(filePath, json);

        }
        // context.set("dne_tokenRefreshTime", Date.now());
    
        // const authRequest = {
        //     url: 'https://' + context.get("et_subdomain") + '.auth.marketingcloudapis.com/v2/token',
        //     method: 'POST',
        //     header: 'Content-Type:application/json',
        //     body: JSON.stringify({
        //         "grant_type": "client_credentials",
        //         "client_id": context.get("et_clientId"),
        //         "client_secret": context.get("et_clientSecret"),
        //         "account_id": context.get("et_mid")
        //     })
        // };
    
        // pm.sendRequest(authRequest, (err, res) => {
        //     console.log(err ? err : res.json());
        //     if (err === null) {
        //         const responseJson = res.json();
        //         context.set('dne_etAccessToken', responseJson.access_token);
        //         console.log("Success: token acquired");
        //     } else {
        //         console.error("Failed: token acquired");
        //     }
        // });


/* Begin Usage Tracking */
// pm.sendRequest('www.google-analytics.com/collect?v=1&tid=UA-114173005-2&cid=1&t=pageview&dh=mcexperts.ninja&dp=/projects/postman/collection/run&dt=PostmanEnhancedCollectionRun');
/* End Usage Tracking */

/* Begin Token Refresh Logic */
// const context = pm.environment.name ? pm.environment : pm.collectionVariables;

// const tokenRefreshTime = context.get("dne_tokenRefreshTime") || 0;

/* skip token refresh if no token exists */
// if (!tokenRefreshTime) { 
//     console.log('no token available at the SFMC API Pre-request Script level');
// 
//     console.log("getting new token...");
//         context.set("dne_tokenRefreshTime", Date.now());
//     
//         const authRequest = {
//             url: 'https://' + context.get("et_subdomain") + '.auth.marketingcloudapis.com/v2/token',
//             method: 'POST',
//             header: 'Content-Type:application/json',
//             body: JSON.stringify({
//                 "grant_type": "client_credentials",
//                 "client_id": context.get("et_clientId"),
//                 "client_secret": context.get("et_clientSecret"),
//                 "account_id": context.get("et_mid")
//             })
//         };
    
//         pm.sendRequest(authRequest, (err, res) => {
//             console.log(err ? err : res.json());
//             if (err === null) {
//                 const responseJson = res.json();
//                 context.set('dne_etAccessToken', responseJson.access_token);
//                 console.log("Success: token acquired");
//             } else {
//                 console.error("Failed: token acquired");
//             }
//         });
// }
// else {
//     console.log("token refresh time: " + tokenRefreshTime);

//     const days = 86400000   // day
//     const hours = 3600000   // hour
//     const minutes = 60000   // minute

//     const tokenAge = Math.round((((Date.now() - tokenRefreshTime) % days) % hours) / minutes);
    
//     if (tokenAge < 18) {
//         console.log("token valid");
//     } else {
//         console.log("refreshing token...");
//         context.set("dne_tokenRefreshTime", Date.now());
    
//         const authRequest = {
//             url: 'https://' + context.get("et_subdomain") + '.auth.marketingcloudapis.com/v2/token',
//             method: 'POST',
//             header: 'Content-Type:application/json',
//             body: JSON.stringify({
//                 "grant_type": "client_credentials",
//                 "client_id": context.get("et_clientId"),
//                 "client_secret": context.get("et_clientSecret"),
//                 "account_id": context.get("et_mid")
//             })
//         };
    
//         pm.sendRequest(authRequest, (err, res) => {
//             console.log(err ? err : res.json());
//             if (err === null) {
//                 const responseJson = res.json();
//                 context.set('dne_etAccessToken', responseJson.access_token);
//                 console.log("Success: token refresh");
//             } else {
//                 console.error("Failed: token refresh");
//             }
//         });
//     }
// }
/* End Token Refresh Logic */
    }
}