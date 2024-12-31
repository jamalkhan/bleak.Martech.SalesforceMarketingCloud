using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication.SfmcPocos;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder.SfmcPocos;
using System.Diagnostics;
using System.ServiceModel;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp
{
    /*public partial class AuthRepository
    {
        private static object _syncLock = new object();
        private static AuthRepository? _instance = null;
        public static AuthRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock(_syncLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AuthRepository();
                        }
                    }
                }
                return _instance!;
            }
        }
    }*/
}