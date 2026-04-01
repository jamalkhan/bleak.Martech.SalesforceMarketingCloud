using System.Text.RegularExpressions;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;

namespace bleak.Martech.SalesforceMarketingCloud.MockService;

public sealed class MockSfmcStore
{
    private readonly object _sync = new();
    private readonly List<SfmcFolder> _assetFolders;
    private readonly List<MockDataExtensionFolder> _dataExtensionFolders;
    private readonly List<SfmcAsset> _assets;
    private readonly List<MockQueryDefinition> _queryDefinitions;
    private readonly List<MockTrackingEvent> _openEvents;
    private readonly List<MockTrackingEvent> _clickEvents;
    private readonly List<MockTrackingEvent> _sentEvents;
    private readonly Dictionary<string, MockDataExtensionDefinition> _dataExtensions;
    private int _nextDataExtensionObjectId = 5000;

    public MockSfmcStore()
    {
        _assetFolders =
        [
            new SfmcFolder { id = 100, parentId = 0, name = "Content Builder", description = "Root content", categoryType = "asset", enterpriseId = 1, memberId = 1 },
            new SfmcFolder { id = 110, parentId = 100, name = "Emails", description = "Email assets", categoryType = "asset", enterpriseId = 1, memberId = 1 },
            new SfmcFolder { id = 120, parentId = 100, name = "Shared Blocks", description = "Reusable blocks", categoryType = "asset", enterpriseId = 1, memberId = 1 },
            new SfmcFolder { id = 130, parentId = 100, name = "Images", description = "Image assets", categoryType = "asset", enterpriseId = 1, memberId = 1 },
        ];

        _dataExtensionFolders =
        [
            new MockDataExtensionFolder(200, 0, "Data Extensions", "dataextension", "Root DE folder"),
            new MockDataExtensionFolder(210, 200, "Subscribers", "dataextension", "Subscriber data"),
            new MockDataExtensionFolder(220, 200, "Imports", "dataextension", "Imported files"),
            new MockDataExtensionFolder(300, 0, "Shared Data Extensions", "shared_data", "Shared root"),
            new MockDataExtensionFolder(310, 300, "Reference", "shared_data", "Reference data"),
        ];

        _assets =
        [
            BuildHtmlAsset(
                id: 1001,
                customerKey: "welcome-email",
                folderId: 110,
                name: "Welcome Email",
                assetTypeName: "htmlemail",
                content: "<html><body><h1>Welcome</h1><p>%%=ContentBlockByKey(\"hero-block\")=%%</p></body></html>"),
            BuildHtmlAsset(
                id: 1002,
                customerKey: "hero-block",
                folderId: 120,
                name: "Hero Block",
                assetTypeName: "htmlblock",
                content: "<section><p>This is a mock content block.</p></section>"),
            BuildImageAsset(
                id: 1003,
                customerKey: "spring-banner",
                folderId: 130,
                name: "Spring Banner",
                fileName: "spring-banner.png",
                extension: "png",
                publishedPath: "/mock-content/spring-banner.png"),
        ];

        _dataExtensions = new Dictionary<string, MockDataExtensionDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["Subscribers_Master"] = new MockDataExtensionDefinition
            {
                Name = "Subscribers Master",
                CustomerKey = "Subscribers_Master",
                Description = "Primary subscriber list",
                CategoryId = 210,
                ObjectId = "de-subscribers-master",
                Columns =
                [
                    new DataExtensionImportColumn { Name = "SubscriberKey", DataType = "string", IsNullable = false, MaxLength = 100 },
                    new DataExtensionImportColumn { Name = "EmailAddress", DataType = "string", IsNullable = false, MaxLength = 255 },
                    new DataExtensionImportColumn { Name = "Status", DataType = "string", IsNullable = true, MaxLength = 50 },
                ],
                Rows =
                [
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["SubscriberKey"] = "jamal@example.com",
                        ["EmailAddress"] = "jamal@example.com",
                        ["Status"] = "Active",
                    },
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["SubscriberKey"] = "alex@example.com",
                        ["EmailAddress"] = "alex@example.com",
                        ["Status"] = "Active",
                    },
                ],
            },
            ["Product_Catalog"] = new MockDataExtensionDefinition
            {
                Name = "Product Catalog",
                CustomerKey = "Product_Catalog",
                Description = "Catalog export",
                CategoryId = 310,
                ObjectId = "de-product-catalog",
                Columns =
                [
                    new DataExtensionImportColumn { Name = "Sku", DataType = "string", IsNullable = false, MaxLength = 50 },
                    new DataExtensionImportColumn { Name = "Name", DataType = "string", IsNullable = false, MaxLength = 255 },
                    new DataExtensionImportColumn { Name = "Price", DataType = "float", IsNullable = true },
                ],
                Rows =
                [
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["Sku"] = "ABC-100",
                        ["Name"] = "Starter Kit",
                        ["Price"] = "29.99",
                    },
                ],
            },
        };

        _queryDefinitions =
        [
            new MockQueryDefinition(
                Name: "Refresh Product Catalog",
                Description: "Loads the latest product catalog",
                CustomerKey: "query-refresh-product-catalog",
                DataExtensionTargetName: "Product Catalog",
                FileSpec: string.Empty,
                FileType: "SQL",
                QueryText: "select Sku, Name, Price from ProductSource"),
            new MockQueryDefinition(
                Name: "Active Subscribers",
                Description: "Filters active subscribers",
                CustomerKey: "query-active-subscribers",
                DataExtensionTargetName: "Subscribers Master",
                FileSpec: string.Empty,
                FileType: "SQL",
                QueryText: "select SubscriberKey, EmailAddress from Subscribers where Status = 'Active'"),
        ];

        _openEvents = BuildTrackingEvents("Open");
        _clickEvents = BuildTrackingEvents("Click");
        _sentEvents = BuildTrackingEvents("Sent");
    }

    public SfmcAuthToken Authenticate(string scheme, HostString host)
    {
        var baseUrl = $"{scheme}://{host}";
        return new SfmcAuthToken
        {
            access_token = "mock-sfmc-token",
            token_type = "Bearer",
            expires_in = 3600,
            scope = "mock",
            soap_instance_url = $"{baseUrl}/Service.asmx",
            rest_instance_url = baseUrl,
        };
    }

    public IReadOnlyList<SfmcFolder> GetAssetFolders()
    {
        return _assetFolders;
    }

    public IReadOnlyList<MockDataExtensionFolder> GetDataExtensionFolders(string contentType)
    {
        return _dataExtensionFolders
            .Where(x => string.Equals(x.ContentType, contentType, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Id)
            .ToList();
    }

    public IReadOnlyList<SfmcAsset> GetAssets(string? filter)
    {
        IEnumerable<SfmcAsset> query = _assets;

        if (string.IsNullOrWhiteSpace(filter))
        {
            return query.OrderBy(x => x.name).ToList();
        }

        var categoryMatch = Regex.Match(filter, @"category\.id eq (\d+)", RegexOptions.IgnoreCase);
        if (categoryMatch.Success)
        {
            var categoryId = int.Parse(categoryMatch.Groups[1].Value);
            query = query.Where(x => x.category.id == categoryId);
        }

        var idMatch = Regex.Match(filter, @"id eq (\d+)", RegexOptions.IgnoreCase);
        if (idMatch.Success)
        {
            var id = int.Parse(idMatch.Groups[1].Value);
            query = query.Where(x => x.id == id);
        }

        var customerKeyEquals = Regex.Match(filter, @"customerKey eq '([^']+)'", RegexOptions.IgnoreCase);
        if (customerKeyEquals.Success)
        {
            var key = customerKeyEquals.Groups[1].Value;
            query = query.Where(x => string.Equals(x.customerKey, key, StringComparison.OrdinalIgnoreCase));
        }

        var nameEquals = Regex.Match(filter, @"name eq '([^']+)'", RegexOptions.IgnoreCase);
        if (nameEquals.Success)
        {
            var name = nameEquals.Groups[1].Value;
            query = query.Where(x => string.Equals(x.name, name, StringComparison.OrdinalIgnoreCase));
        }

        var likeMatch = Regex.Match(filter, @"(name|customerKey) like '%(.+)%'", RegexOptions.IgnoreCase);
        if (likeMatch.Success)
        {
            var property = likeMatch.Groups[1].Value;
            var searchTerm = likeMatch.Groups[2].Value;

            query = property.Equals("name", StringComparison.OrdinalIgnoreCase)
                ? query.Where(x => x.name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                : query.Where(x => x.customerKey.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderBy(x => x.name).ToList();
    }

    public IReadOnlyList<SfmcAsset> BuildAssetsForResponse(string scheme, HostString host, IEnumerable<SfmcAsset> assets)
    {
        var baseUrl = $"{scheme}://{host}";
        return assets.Select(asset => CloneAsset(asset, baseUrl)).ToList();
    }

    public MockDataExtensionDefinition? GetDataExtension(string customerKey)
    {
        lock (_sync)
        {
            return _dataExtensions.TryGetValue(customerKey, out var definition)
                ? definition.Clone()
                : null;
        }
    }

    public IReadOnlyList<MockDataExtensionDefinition> GetDataExtensions(string? filterProperty = null, string? filterOperator = null, string? filterValue = null)
    {
        lock (_sync)
        {
            IEnumerable<MockDataExtensionDefinition> query = _dataExtensions.Values.Select(x => x.Clone());

            if (!string.IsNullOrWhiteSpace(filterProperty))
            {
                if (string.Equals(filterProperty, "CategoryID", StringComparison.OrdinalIgnoreCase)
                    && long.TryParse(filterValue, out var categoryId))
                {
                    query = query.Where(x => x.CategoryId == categoryId);
                }
                else if (string.Equals(filterProperty, "Name", StringComparison.OrdinalIgnoreCase))
                {
                    var normalizedOperator = filterOperator?.ToLowerInvariant();
                    query = normalizedOperator switch
                    {
                        "like" => query.Where(x => x.Name.Contains(filterValue ?? string.Empty, StringComparison.OrdinalIgnoreCase)),
                        "beginswith" => query.Where(x => x.Name.StartsWith(filterValue ?? string.Empty, StringComparison.OrdinalIgnoreCase)),
                        "endswith" => query.Where(x => x.Name.EndsWith(filterValue ?? string.Empty, StringComparison.OrdinalIgnoreCase)),
                        _ => query,
                    };
                }
            }

            return query.OrderBy(x => x.Name).ToList();
        }
    }

    public void CreateDataExtension(DataExtensionImportDefinition definition)
    {
        lock (_sync)
        {
            _dataExtensions[definition.CustomerKey] = new MockDataExtensionDefinition
            {
                Name = definition.Name,
                CustomerKey = definition.CustomerKey,
                Description = definition.Description,
                CategoryId = definition.CategoryId,
                ObjectId = $"de-{Interlocked.Increment(ref _nextDataExtensionObjectId)}",
                Columns = definition.Columns
                    .Select(column => new DataExtensionImportColumn
                    {
                        Name = column.Name,
                        DataType = column.DataType,
                        IsNullable = column.IsNullable,
                        MaxLength = column.MaxLength,
                    })
                    .ToList(),
                Rows = [],
            };
        }
    }

    public void AddRows(string customerKey, IReadOnlyList<Dictionary<string, string>> rows)
    {
        lock (_sync)
        {
            if (!_dataExtensions.TryGetValue(customerKey, out var definition))
            {
                throw new InvalidOperationException($"Unknown mock data extension '{customerKey}'.");
            }

            foreach (var row in rows)
            {
                definition.Rows.Add(new Dictionary<string, string>(row, StringComparer.OrdinalIgnoreCase));
            }
        }
    }

    public IReadOnlyList<MockQueryDefinition> GetQueryDefinitions()
    {
        return _queryDefinitions;
    }

    public IReadOnlyList<MockTrackingEvent> GetTrackingEvents(string objectType, DateTime? startDateUtc, DateTime? endDateUtc)
    {
        IEnumerable<MockTrackingEvent> source = objectType switch
        {
            "OpenEvent" => _openEvents,
            "ClickEvent" => _clickEvents,
            "SentEvent" => _sentEvents,
            _ => [],
        };

        if (startDateUtc.HasValue)
        {
            source = source.Where(x => x.EventDateUtc >= startDateUtc.Value);
        }

        if (endDateUtc.HasValue)
        {
            source = source.Where(x => x.EventDateUtc < endDateUtc.Value);
        }

        return source.OrderBy(x => x.EventDateUtc).ToList();
    }

    public byte[] GetMockContent(string fileName)
    {
        return fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            ? Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+yNlwAAAAASUVORK5CYII=")
            : System.Text.Encoding.UTF8.GetBytes("Mock SFMC content");
    }

    private static List<MockTrackingEvent> BuildTrackingEvents(string eventType)
    {
        var start = DateTime.UtcNow.Date.AddDays(-2);
        return
        [
            new MockTrackingEvent("subscriber-001", start.AddHours(8), 101, eventType),
            new MockTrackingEvent("subscriber-002", start.AddHours(11), 102, eventType),
            new MockTrackingEvent("subscriber-003", start.AddDays(1).AddHours(9), 103, eventType),
        ];
    }

    private static SfmcAsset BuildHtmlAsset(int id, string customerKey, int folderId, string name, string assetTypeName, string content)
    {
        return new SfmcAsset
        {
            id = id,
            customerKey = customerKey,
            objectID = $"asset-{id}",
            assetType = new SfmcAssetType { id = id, name = assetTypeName, displayName = name },
            name = name,
            description = $"Mock asset for {name}",
            createdDate = DateTime.UtcNow.AddDays(-14),
            createdBy = new SfmcUser { id = 1, email = "mock@example.com", name = "Mock User", userId = "mock-user" },
            modifiedDate = DateTime.UtcNow.AddDays(-1),
            modifiedBy = new SfmcUser { id = 1, email = "mock@example.com", name = "Mock User", userId = "mock-user" },
            enterpriseId = 1,
            memberId = 1,
            status = new SfmcStatus { id = 1, name = "Active" },
            category = new SfmcCategory { id = folderId, parentId = 100, name = "Mock Folder" },
            content = content,
            contentType = "text/html",
            views = new SfmcViews { html = new SfmcHtml { content = content } },
            fileProperties = new SfmcFileProperties
            {
                fileName = $"{customerKey}.html",
                extension = "html",
                fileSize = content.Length,
                fileCreatedDate = DateTime.UtcNow.AddDays(-14),
            },
        };
    }

    private static SfmcAsset BuildImageAsset(int id, string customerKey, int folderId, string name, string fileName, string extension, string publishedPath)
    {
        return new SfmcAsset
        {
            id = id,
            customerKey = customerKey,
            objectID = $"asset-{id}",
            assetType = new SfmcAssetType { id = id, name = extension, displayName = name },
            name = name,
            description = $"Mock image asset for {name}",
            createdDate = DateTime.UtcNow.AddDays(-14),
            createdBy = new SfmcUser { id = 1, email = "mock@example.com", name = "Mock User", userId = "mock-user" },
            modifiedDate = DateTime.UtcNow.AddDays(-1),
            modifiedBy = new SfmcUser { id = 1, email = "mock@example.com", name = "Mock User", userId = "mock-user" },
            enterpriseId = 1,
            memberId = 1,
            status = new SfmcStatus { id = 1, name = "Active" },
            category = new SfmcCategory { id = folderId, parentId = 100, name = "Images" },
            contentType = "image/png",
            fileProperties = new SfmcFileProperties
            {
                fileName = fileName,
                extension = extension,
                fileSize = 68,
                fileCreatedDate = DateTime.UtcNow.AddDays(-14),
                width = 1,
                height = 1,
                publishedURL = publishedPath,
            },
        };
    }

    private static SfmcAsset CloneAsset(SfmcAsset asset, string baseUrl)
    {
        return new SfmcAsset
        {
            id = asset.id,
            customerKey = asset.customerKey,
            objectID = asset.objectID,
            assetType = new SfmcAssetType
            {
                id = asset.assetType.id,
                name = asset.assetType.name,
                displayName = asset.assetType.displayName,
            },
            name = asset.name,
            description = asset.description,
            createdDate = asset.createdDate,
            createdBy = new SfmcUser
            {
                id = asset.createdBy.id,
                email = asset.createdBy.email,
                name = asset.createdBy.name,
                userId = asset.createdBy.userId,
            },
            modifiedDate = asset.modifiedDate,
            modifiedBy = new SfmcUser
            {
                id = asset.modifiedBy.id,
                email = asset.modifiedBy.email,
                name = asset.modifiedBy.name,
                userId = asset.modifiedBy.userId,
            },
            enterpriseId = asset.enterpriseId,
            memberId = asset.memberId,
            status = new SfmcStatus
            {
                id = asset.status.id,
                name = asset.status.name,
            },
            category = new SfmcCategory
            {
                id = asset.category.id,
                parentId = asset.category.parentId,
                name = asset.category.name,
            },
            content = asset.content,
            contentType = asset.contentType,
            views = new SfmcViews
            {
                html = new SfmcHtml
                {
                    content = asset.views.html.content,
                },
            },
            fileProperties = new SfmcFileProperties
            {
                fileName = asset.fileProperties.fileName,
                extension = asset.fileProperties.extension,
                fileSize = asset.fileProperties.fileSize,
                fileCreatedDate = asset.fileProperties.fileCreatedDate,
                width = asset.fileProperties.width,
                height = asset.fileProperties.height,
                publishedURL = asset.fileProperties.publishedURL.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? asset.fileProperties.publishedURL
                    : $"{baseUrl}{asset.fileProperties.publishedURL}",
            },
        };
    }
}

public sealed class MockDataExtensionDefinition
{
    public string Name { get; set; } = string.Empty;
    public string CustomerKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public string ObjectId { get; set; } = string.Empty;
    public List<DataExtensionImportColumn> Columns { get; set; } = [];
    public List<Dictionary<string, string>> Rows { get; set; } = [];

    public MockDataExtensionDefinition Clone()
    {
        return new MockDataExtensionDefinition
        {
            Name = Name,
            CustomerKey = CustomerKey,
            Description = Description,
            CategoryId = CategoryId,
            ObjectId = ObjectId,
            Columns = Columns.Select(column => new DataExtensionImportColumn
            {
                Name = column.Name,
                DataType = column.DataType,
                IsNullable = column.IsNullable,
                MaxLength = column.MaxLength,
            }).ToList(),
            Rows = Rows.Select(row => new Dictionary<string, string>(row, StringComparer.OrdinalIgnoreCase)).ToList(),
        };
    }
}

public sealed record MockDataExtensionFolder(int Id, int ParentId, string Name, string ContentType, string Description);
public sealed record MockQueryDefinition(string Name, string Description, string CustomerKey, string DataExtensionTargetName, string FileSpec, string FileType, string QueryText);
public sealed record MockTrackingEvent(string SubscriberKey, DateTime EventDateUtc, int SendId, string EventType);
