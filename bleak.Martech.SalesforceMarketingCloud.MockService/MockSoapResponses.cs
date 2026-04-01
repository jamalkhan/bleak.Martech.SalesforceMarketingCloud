using System.Globalization;
using System.Xml.Linq;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;

namespace bleak.Martech.SalesforceMarketingCloud.MockService;

public static class MockSoapResponses
{
    private static readonly XNamespace SoapNs = "http://www.w3.org/2003/05/soap-envelope";
    private static readonly XNamespace PartnerNs = "http://exacttarget.com/wsdl/partnerAPI";
    private static readonly XNamespace XsiNs = "http://www.w3.org/2001/XMLSchema-instance";

    public static string Handle(string requestXml, MockSfmcStore store)
    {
        var document = XDocument.Parse(requestXml);
        var action = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "Action")?.Value ?? string.Empty;

        return action switch
        {
            "Describe" => BuildDescribeResponse(document),
            "Retrieve" => BuildRetrieveResponse(document, store),
            "Create" => BuildCreateResponse(document, store),
            _ => BuildFaultResponse($"Unsupported SOAP action '{action}'."),
        };
    }

    private static string BuildDescribeResponse(XDocument document)
    {
        var objectType = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "ObjectType")?.Value ?? "UnknownObject";

        var response = new XDocument(
            new XElement(SoapNs + "Envelope",
                new XAttribute(XNamespace.Xmlns + "s", SoapNs),
                new XAttribute(XNamespace.Xmlns + "xsi", XsiNs),
                new XElement(SoapNs + "Body",
                    new XElement(PartnerNs + "DefinitionResponseMsg",
                        new XElement(PartnerNs + "ObjectDefinition",
                            new XElement(PartnerNs + "ObjectType", objectType),
                            new XElement(PartnerNs + "Name", objectType),
                            new XElement(PartnerNs + "Properties",
                                new XElement(PartnerNs + "Name", "CustomerKey")),
                            new XElement(PartnerNs + "Properties",
                                new XElement(PartnerNs + "Name", "Name")),
                            new XElement(PartnerNs + "Properties",
                                new XElement(PartnerNs + "Name", "Description")))))));

        return response.ToString(SaveOptions.DisableFormatting);
    }

    private static string BuildRetrieveResponse(XDocument document, MockSfmcStore store)
    {
        var retrieveRequest = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "RetrieveRequest");
        var objectType = retrieveRequest?.Elements().FirstOrDefault(x => x.Name.LocalName == "ObjectType")?.Value ?? string.Empty;

        var resultElements = objectType switch
        {
            "DataFolder" => BuildDataFolderResults(retrieveRequest, store),
            "DataExtension" => BuildDataExtensionResults(retrieveRequest, store),
            "QueryDefinition" => BuildQueryDefinitionResults(store),
            "OpenEvent" => BuildTrackingResults(store, objectType, retrieveRequest),
            "ClickEvent" => BuildTrackingResults(store, objectType, retrieveRequest),
            "SentEvent" => BuildTrackingResults(store, objectType, retrieveRequest),
            _ => [],
        };

        return BuildEnvelope("RetrieveResponseMsg",
            new XElement(PartnerNs + "OverallStatus", "OK"),
            new XElement(PartnerNs + "RequestID", string.Empty),
            resultElements);
    }

    private static IEnumerable<XElement> BuildDataFolderResults(XElement? retrieveRequest, MockSfmcStore store)
    {
        var contentType = retrieveRequest?
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "Property" && x.Value == "ContentType")
            ?.Parent?
            .Elements()
            .FirstOrDefault(x => x.Name.LocalName == "Value")
            ?.Value
            ?? "dataextension";

        return store.GetDataExtensionFolders(contentType).Select(folder =>
            new XElement(PartnerNs + "Results",
                new XAttribute(XsiNs + "type", "DataFolder"),
                new XElement(PartnerNs + "ID", folder.Id),
                new XElement(PartnerNs + "ObjectID", $"folder-{folder.Id}"),
                new XElement(PartnerNs + "ParentFolder",
                    new XElement(PartnerNs + "ID", folder.ParentId),
                    new XElement(PartnerNs + "Name", store.GetDataExtensionFolders(folder.ContentType).FirstOrDefault(x => x.Id == folder.ParentId)?.Name ?? string.Empty)),
                new XElement(PartnerNs + "Name", folder.Name),
                new XElement(PartnerNs + "Description", folder.Description),
                new XElement(PartnerNs + "ContentType", folder.ContentType),
                new XElement(PartnerNs + "IsActive", true),
                new XElement(PartnerNs + "IsEditable", true)));
    }

    private static IEnumerable<XElement> BuildDataExtensionResults(XElement? retrieveRequest, MockSfmcStore store)
    {
        var filterProperty = retrieveRequest?.Descendants().FirstOrDefault(x => x.Name.LocalName == "Property")?.Value;
        var filterOperator = retrieveRequest?.Descendants().FirstOrDefault(x => x.Name.LocalName == "SimpleOperator")?.Value;
        var filterValue = retrieveRequest?.Descendants().FirstOrDefault(x => x.Name.LocalName == "Value")?.Value;

        return store.GetDataExtensions(filterProperty, filterOperator, filterValue).Select(definition =>
            new XElement(PartnerNs + "Results",
                new XAttribute(XsiNs + "type", "DataExtension"),
                new XElement(PartnerNs + "ObjectID", definition.ObjectId),
                new XElement(PartnerNs + "CustomerKey", definition.CustomerKey),
                new XElement(PartnerNs + "Name", definition.Name),
                new XElement(PartnerNs + "Description", definition.Description),
                new XElement(PartnerNs + "CategoryID", definition.CategoryId),
                new XElement(PartnerNs + "IsSendable", false),
                new XElement(PartnerNs + "IsTestable", false)));
    }

    private static IEnumerable<XElement> BuildQueryDefinitionResults(MockSfmcStore store)
    {
        return store.GetQueryDefinitions().Select(query =>
            new XElement(PartnerNs + "Results",
                new XAttribute(XsiNs + "type", "QueryDefinition"),
                new XElement(PartnerNs + "CustomerKey", query.CustomerKey),
                new XElement(PartnerNs + "Name", query.Name),
                new XElement(PartnerNs + "Description", query.Description),
                new XElement(PartnerNs + "DataExtensionTarget",
                    new XElement(PartnerNs + "Name", query.DataExtensionTargetName)),
                new XElement(PartnerNs + "FileSpec", query.FileSpec),
                new XElement(PartnerNs + "FileType", query.FileType),
                new XElement(PartnerNs + "QueryText", query.QueryText)));
    }

    private static IEnumerable<XElement> BuildTrackingResults(MockSfmcStore store, string objectType, XElement? retrieveRequest)
    {
        var filterValues = retrieveRequest?
            .Descendants()
            .Where(x => x.Name.LocalName == "Value")
            .Select(x => x.Value)
            .ToList() ?? [];

        DateTime? startDateUtc = ParseDate(filterValues.ElementAtOrDefault(0));
        DateTime? endDateUtc = ParseDate(filterValues.ElementAtOrDefault(1));

        return store.GetTrackingEvents(objectType, startDateUtc, endDateUtc).Select(trackingEvent =>
        {
            var result = new XElement(PartnerNs + "Results",
                new XAttribute(XsiNs + "type", objectType),
                new XElement(PartnerNs + "SendID", trackingEvent.SendId),
                new XElement(PartnerNs + "SubscriberKey", trackingEvent.SubscriberKey),
                new XElement(PartnerNs + "EventDate", trackingEvent.EventDateUtc.ToString("o", CultureInfo.InvariantCulture)),
                new XElement(PartnerNs + "EventType", trackingEvent.EventType));

            if (objectType == "ClickEvent")
            {
                result.Add(new XElement(PartnerNs + "URLID", 1));
                result.Add(new XElement(PartnerNs + "URL", "https://example.com/mock"));
                result.Add(new XElement(PartnerNs + "URLIDLong", 1));
            }

            return result;
        });
    }

    private static string BuildCreateResponse(XDocument document, MockSfmcStore store)
    {
        var createObjects = document.Descendants().Where(x => x.Name.LocalName == "Objects").ToList();
        if (createObjects.Count == 0)
        {
            return BuildFaultResponse("CreateRequest did not contain any Objects.");
        }

        var objectType = createObjects[0].Attributes().FirstOrDefault(x => x.Name.LocalName == "type")?.Value ?? string.Empty;

        if (string.Equals(objectType, "DataExtension", StringComparison.OrdinalIgnoreCase))
        {
            var definition = new DataExtensionImportDefinition
            {
                CustomerKey = createObjects[0].Elements().FirstOrDefault(x => x.Name.LocalName == "CustomerKey")?.Value ?? string.Empty,
                Name = createObjects[0].Elements().FirstOrDefault(x => x.Name.LocalName == "Name")?.Value ?? string.Empty,
                Description = createObjects[0].Elements().FirstOrDefault(x => x.Name.LocalName == "Description")?.Value ?? string.Empty,
                CategoryId = int.TryParse(createObjects[0].Elements().FirstOrDefault(x => x.Name.LocalName == "CategoryID")?.Value, out var categoryId) ? categoryId : 0,
                Columns = createObjects[0]
                    .Descendants()
                    .Where(x => x.Name.LocalName == "Field")
                    .Select(field => new DataExtensionImportColumn
                    {
                        Name = field.Elements().FirstOrDefault(x => x.Name.LocalName == "Name")?.Value ?? string.Empty,
                        DataType = field.Elements().FirstOrDefault(x => x.Name.LocalName == "FieldType")?.Value ?? "Text",
                        IsNullable = !bool.TryParse(field.Elements().FirstOrDefault(x => x.Name.LocalName == "IsRequired")?.Value, out var isRequired) || !isRequired,
                        MaxLength = int.TryParse(field.Elements().FirstOrDefault(x => x.Name.LocalName == "MaxLength")?.Value, out var maxLength) ? maxLength : null,
                    })
                    .ToList(),
            };

            store.CreateDataExtension(definition);
        }
        else if (string.Equals(objectType, "DataExtensionObject", StringComparison.OrdinalIgnoreCase))
        {
            var customerKey = createObjects[0].Elements().FirstOrDefault(x => x.Name.LocalName == "CustomerKey")?.Value ?? string.Empty;
            var rows = createObjects.Select(obj =>
                obj.Descendants()
                    .Where(x => x.Name.LocalName == "Property")
                    .ToDictionary(
                        prop => prop.Elements().FirstOrDefault(x => x.Name.LocalName == "Name")?.Value ?? string.Empty,
                        prop => prop.Elements().FirstOrDefault(x => x.Name.LocalName == "Value")?.Value ?? string.Empty,
                        StringComparer.OrdinalIgnoreCase))
                .ToList();

            store.AddRows(customerKey, rows);
        }
        else
        {
            return BuildFaultResponse($"Unsupported create object type '{objectType}'.");
        }

        return BuildEnvelope("CreateResponse",
            new XElement(PartnerNs + "Results",
                new XElement(PartnerNs + "StatusCode", "OK"),
                new XElement(PartnerNs + "StatusMessage", "Mock create succeeded")),
            new XElement(PartnerNs + "RequestID", Guid.NewGuid().ToString("N")),
            new XElement(PartnerNs + "OverallStatus", "OK"));
    }

    private static string BuildFaultResponse(string message)
    {
        return new XDocument(
            new XElement(SoapNs + "Envelope",
                new XAttribute(XNamespace.Xmlns + "s", SoapNs),
                new XElement(SoapNs + "Body",
                    new XElement(SoapNs + "Fault",
                        new XElement(SoapNs + "Reason",
                            new XElement(SoapNs + "Text", message)))))).ToString(SaveOptions.DisableFormatting);
    }

    private static string BuildEnvelope(string bodyElementName, params object[] children)
    {
        return new XDocument(
            new XElement(SoapNs + "Envelope",
                new XAttribute(XNamespace.Xmlns + "s", SoapNs),
                new XAttribute(XNamespace.Xmlns + "xsi", XsiNs),
                new XElement(SoapNs + "Body",
                    new XElement(PartnerNs + bodyElementName, children)))).ToString(SaveOptions.DisableFormatting);
    }

    private static DateTime? ParseDate(string? rawValue)
    {
        if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            return parsed.ToUniversalTime();
        }

        return null;
    }
}
