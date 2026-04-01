using bleak.Martech.SalesforceMarketingCloud.MockService;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.IncludeFields = true;
});
builder.Services.AddSingleton<MockSfmcStore>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    name = "SFMC Mock Service",
    authBaseUrl = "/v2/token",
    restBaseUrl = "/",
    soapBaseUrl = "/Service.asmx",
}));

app.MapPost("/v2/token", (HttpContext context, MockSfmcStore store) =>
{
    return Results.Ok(store.Authenticate(context.Request.Scheme, context.Request.Host));
});

app.MapGet("/asset/v1/content/categories", (HttpRequest request, MockSfmcStore store) =>
{
    var page = ParseInt(request.Query["$page"], 1);
    var pageSize = ParseInt(request.Query["$pagesize"], 500);
    var folders = Paginate(store.GetAssetFolders(), page, pageSize);

    return Results.Ok(new SfmcRestWrapper<SfmcFolder>
    {
        count = folders.Count,
        page = page,
        pageSize = pageSize,
        items = folders,
    });
});

app.MapGet("/asset/v1/content/assets", (HttpRequest request, MockSfmcStore store) =>
{
    var page = ParseInt(request.Query["$page"], 1);
    var pageSize = ParseInt(request.Query["$pagesize"], 500);
    var filter = request.Query["$filter"].ToString();
    var assets = store.BuildAssetsForResponse(request.Scheme, request.Host, Paginate(store.GetAssets(filter), page, pageSize));

    return Results.Ok(new SfmcRestWrapper<SfmcAsset>
    {
        count = assets.Count,
        page = page,
        pageSize = pageSize,
        items = assets.ToList(),
    });
});

app.MapGet("/legacy/v1/beta/object", (HttpRequest request, MockSfmcStore store) =>
{
    var page = ParseInt(request.Query["$page"], 1);
    var pageSize = ParseInt(request.Query["$pagesize"], 500);
    var folders = Paginate(
        store.GetDataExtensionFolders("dataextension")
            .Select(folder => new SfmcFolder
            {
                id = folder.Id,
                parentId = folder.ParentId,
                name = folder.Name,
                description = folder.Description,
                categoryType = folder.ContentType,
                enterpriseId = 1,
                memberId = 1,
            }).ToList(),
        page,
        pageSize);

    return Results.Ok(new SfmcRestWrapper<SfmcFolder>
    {
        count = folders.Count,
        page = page,
        pageSize = pageSize,
        items = folders,
    });
});

app.MapGet("/data/v1/customobjectdata/key/{customerKey}/rowset", (HttpRequest request, string customerKey, MockSfmcStore store) =>
{
    var page = ParseInt(request.Query["$page"], 1);
    var pageSize = ParseInt(request.Query["$pageSize"], 2500);
    var definition = store.GetDataExtension(customerKey);
    if (definition is null)
    {
        return Results.NotFound();
    }

    var rows = Paginate(definition.Rows, page, pageSize);
    var hasMore = definition.Rows.Count > page * pageSize;

    return Results.Ok(new DataExtensionDataDto
    {
        customObjectId = definition.ObjectId,
        customObjectKey = definition.CustomerKey,
        requestToken = "mock-request-token",
        tokenExpireDateUtc = DateTime.UtcNow.AddMinutes(5),
        page = page,
        pageSize = pageSize,
        count = rows.Count,
        top = pageSize,
        links = new LinksDto
        {
            self = request.Path + request.QueryString,
            next = hasMore ? $"{request.Path}?$page={page + 1}&$pageSize={pageSize}" : string.Empty,
        },
        items = rows.Select(row => new ItemDto
        {
            values = new Dictionary<string, string>(row, StringComparer.OrdinalIgnoreCase),
        }).ToList(),
    });
});

app.MapGet("/mock-content/{fileName}", (string fileName, MockSfmcStore store) =>
{
    var bytes = store.GetMockContent(fileName);
    var contentType = fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "application/octet-stream";
    return Results.File(bytes, contentType, fileName);
});

app.MapPost("/Service.asmx", async (HttpRequest request, MockSfmcStore store) =>
{
    using var reader = new StreamReader(request.Body);
    var requestXml = await reader.ReadToEndAsync();
    var responseXml = MockSoapResponses.Handle(requestXml, store);
    return Results.Text(responseXml, "text/xml");
});

app.Run();

static int ParseInt(string? rawValue, int fallback)
{
    return int.TryParse(rawValue, out var parsed) ? parsed : fallback;
}

static List<T> Paginate<T>(IReadOnlyList<T> items, int page, int pageSize)
{
    return items
        .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
        .Take(Math.Max(pageSize, 1))
        .ToList();
}
