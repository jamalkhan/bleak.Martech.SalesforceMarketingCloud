using bleak.Martech.SalesforceMarketingCloud.Wsdl;
//using SalesforceMarketingCloudSoapApi;
using System.Collections.Generic;
using System.Data;
//using Folder = JamBot.Esp.Folder;
//using Message = JamBot.Esp.Message;
using System.Linq;
using System;


namespace bleak.Martech.SalesforceMarketingCloud.Helpers
{
    public static class SfmcEndpointsHelpers
    {
        public static IEnumerable<string> GetEndpoints()
        {
            return new List<string>
            {
                "https://webservice.exacttarget.com/Service.asmx",
                "https://webservice.s4.exacttarget.com/Service.asmx",
                "https://webservice.s6.exacttarget.com/Service.asmx",
                "https://webservice.s7.exacttarget.com/Service.asmx",
                "https://webservice.s10.exacttarget.com/Service.asmx",
                "https://webservice.test.exacttarget.com/Service.asmx",
            };
        }
    }


    public static class ExtensionMethods
    {
        
        public static bool IsInteger(this DataColumn column)
        {
            if (column == null)
                return false;

            // Make this const
            var numericTypes = new[]
            {
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(sbyte),
                typeof(ushort),
                typeof(uint),
                typeof(ulong)};

            return numericTypes.Contains(column.DataType);
        }

        public static Type ConvertToDotNetType(this PropertyType type)
        {
            switch (type)
            {
                case PropertyType.@double:
                    return typeof(double);
                case PropertyType.@string:
                    return typeof(string);
                case PropertyType.boolean:
                    return typeof(bool);
                case PropertyType.datetime:
                    return typeof(DateTime);
            }
            return typeof(object);
        }
    }
}