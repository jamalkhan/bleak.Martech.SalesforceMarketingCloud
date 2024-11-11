using System;
using System.Collections.Generic;
using System.Text;

namespace bleak.Martech.SalesforceMarketingCloud.Exceptions
{
    public abstract class BaseMarketingCloudException : Exception
    {
        internal BaseMarketingCloudException(string message) : base(message) { }
    }
}