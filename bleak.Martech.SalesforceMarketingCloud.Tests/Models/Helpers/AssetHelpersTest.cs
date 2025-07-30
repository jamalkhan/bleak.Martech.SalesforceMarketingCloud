using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using bleak.Martech.SalesforceMarketingCloud.Models.Helpers;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace bleak.Martech.SalesforceMarketingCloud.Tests.Models.Converters;

[TestClass]
public class AssetHelpersTest
{
    [TestMethod]
    public void ToPocoList_ShouldReturnEmptyList_WhenInputIsEmpty()
    {
        // Arrange
        var assets = new List<SfmcAsset>();

        // Act
        var result = assets.ToPocoList();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }


    [TestMethod]
    public void ToPocoList_ShouldMapAllAssets()
    {
        // Arrange
        var assets = new List<SfmcAsset>
        {
            new SfmcAsset { id = 1, name = "Asset1", assetType = new SfmcAssetType() },
            new SfmcAsset { id = 2, name = "Asset2", assetType = new SfmcAssetType() }
        };

        // Act
        var result = assets.ToPocoList();

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Asset1", result[0].Name);
        Assert.AreEqual("Asset2", result[1].Name);
    }

    [TestMethod]
    public void ToPoco_ShouldMapBasicProperties()
    {
        // Arrange
        var asset = new SfmcAsset
        {
            id = 123,
            customerKey = "CKEY",
            objectID = "OID",
            name = "Test Asset",
            description = "Desc",
            createdDate = DateTime.UtcNow,
            modifiedDate = DateTime.UtcNow,
            enterpriseId = 42,
            memberId = 99,
            assetType = new SfmcAssetType
            {
                id = 1,
                name = "type",
                displayName = "Type"
            }
        };

        // Act
        var poco = asset.ToPoco();

        // Assert
        Assert.AreEqual(123, poco.Id);
        Assert.AreEqual("CKEY", poco.CustomerKey);
        Assert.AreEqual("OID", poco.ObjectID);
        Assert.AreEqual("Test Asset", poco.Name);
        Assert.AreEqual("Desc", poco.Description);
        Assert.AreEqual(42, poco.EnterpriseId);
        Assert.AreEqual(99, poco.MemberId);
        Assert.IsNotNull(poco.AssetType);
        Assert.AreEqual(1, poco.AssetType.Id);
        Assert.AreEqual("type", poco.AssetType.Name);
        Assert.AreEqual("Type", poco.AssetType.DisplayName);
    }

    [TestMethod]
    public void ToPoco_ShouldMapViewsHtml_WhenPresent()
    {
        // Arrange
        var asset = new SfmcAsset
        {
            assetType = new(),
            views = new()
            {
                html = new()
                {
                    content = "<h1>Hello</h1>"
                }
            }
        };

        // Act
        var poco = asset.ToPoco();

        // Assert
        Assert.IsNotNull(poco.Views);
        Assert.IsNotNull(poco.Views.Html);
        Assert.AreEqual("<h1>Hello</h1>", poco.Views.Html.Content);
    }

    [TestMethod]
    public void ToPoco_ShouldMapFileProperties_WhenPresent()
    {
        // Arrange
        var asset = new SfmcAsset
        {
            assetType = new(),
            fileProperties = new()
            {
                fileName = "file.txt",
                extension = ".txt",
                fileSize = 1234,
                fileCreatedDate = DateTime.UtcNow,
                width = 100,
                height = 200,
                publishedURL = "http://example.com/file.txt"
            }
        };

        // Act
        var poco = asset.ToPoco();

        // Assert
        Assert.IsNotNull(poco.FileProperties);
        Assert.AreEqual("file.txt", poco.FileProperties.FileName);
        Assert.AreEqual(".txt", poco.FileProperties.Extension);
        Assert.AreEqual(1234, poco.FileProperties.FileSize);
        Assert.AreEqual(asset.fileProperties.fileCreatedDate, poco.FileProperties.FileCreatedDate);
        Assert.AreEqual(100, poco.FileProperties.Width);
        Assert.AreEqual(200, poco.FileProperties.Height);
        Assert.AreEqual("http://example.com/file.txt", poco.FileProperties.PublishedURL);
    }

    [TestMethod]
    public void ToPoco_ShouldHandleNullViewsAndFileProperties()
    {
        // Arrange
        var asset = new SfmcAsset
        {
            assetType = new(),
            views = null,
            fileProperties = null
        };

        // Act
        var poco = asset.ToPoco();

        // Assert
        //Assert.IsNull(poco.Views);
        Assert.AreEqual("", poco?.Views?.Html.Content);
        Assert.AreEqual("", poco?.FileProperties?.FileName);
    }

    [TestMethod]
    public void GetContentBlocks_ShouldReturnEmptyList_WhenInputIsEmpty()
    {
        // Arrange
        var input = string.Empty;
        AssetPoco asset = new AssetPoco
        {
            Content = input
        };

        // Act
        var result = asset.GetContentBlocks();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }



    [TestMethod]
    public void GetContentBlocks_ShouldReturnContentBlocks_WhenInputContainsValidBlocks()
    {
        // Arrange
        string input = @"
            Doing the Keys
            %%=ContentBlockByKey(""Key1"")=%%
            Key with Spaces
            %%=  ContentBlockByKey  (  ""Key2"" ) =%%
            Doing the Names
            %%=ContentBlockByName(""Name1"")=%%
            Name with Spaces
            %%=  ContentBlockByName  (  ""Name2"" ) =%%
            Doing the IDs
            %%=ContentBlockByID(1)=%%
            Id with Spaces
            %%=  ContentBlockByID  (  2  ) =%%
            with quotes
            %%=ContentBlockByID(""3"")=%%
            with quotes and spaces
            %%=  ContentBlockByID  (  ""4""  ) =%%
        ";
        AssetPoco asset = new AssetPoco
        {
            Content = input
        };

        // Act
        var result = asset.GetContentBlocks();

        Console.WriteLine("Found IDs:");
        foreach (var b in result)
        {
            Console.WriteLine($"result Name={b.Name}; Key={b.Key}; Id={b.Id}");
        }

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(8, result.Count);

        result[0].Key = "Key1";
        result[1].Key = "Key2";
        result[2].Name = "Name1";
        result[3].Name = "Name2";
        result[4].Id = 1;
        result[5].Id = 2;
        result[6].Id = 3;
        result[7].Id = 4;
    }

    [TestMethod]
    public void GetContentBlocks_ShouldReturnContentBlocks_ForKeys()
    {
        // Arrange
        string input = @"
        Doing the Keys
        %%=ContentBlockByKey(""Key1"")=%%
        Key with Spaces
        %%=  ContentBlockByKey  (  ""Key2"" ) =%%
        Key with Optional Parameter
        %%=ContentBlockByKey(""Key3"", ""RegionA"")=%%
        Key with Optional Parameter
        %%=ContentBlockByKey(""Key4"", ""RegionB"", false, ""Oops!"", -1)=%%
    ";
        AssetPoco asset = new AssetPoco { Content = input };

        // Act
        var result = asset.GetContentBlocks();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.Count);
        Assert.AreEqual("Key1", result[0].Key);
        Assert.AreEqual("Key2", result[1].Key);
        Assert.AreEqual("Key3", result[2].Key);
        Assert.AreEqual("Key4", result[3].Key);
    }

    [TestMethod]
    public void GetContentBlocks_ShouldReturnContentBlocks_ForNames()
    {
        // Arrange
        string input = @"
        Doing the Names
        %%=ContentBlockByName(""Name1"")=%%
        Name with Spaces
        %%=  ContentBlockByName  (  ""Name2"" ) =%%
        Name with optional parameter 
        %%=ContentBlockByName(""Name3"", ""RegionA"")=%%
        Name with optional parameter 
        %%=ContentBlockByName(""Name4"", ""RegionB"", false, ""Block not found"", -1)=%%
    ";
        AssetPoco asset = new AssetPoco { Content = input };

        // Act
        var result = asset.GetContentBlocks();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.Count);
        Assert.AreEqual("Name1", result[0].Name);
        Assert.AreEqual("Name2", result[1].Name);
        Assert.AreEqual("Name3", result[2].Name);
        Assert.AreEqual("Name4", result[3].Name);
    }

    [TestMethod]
    public void GetContentBlocks_ShouldReturnContentBlocks_ForIDs()
    {
        // Arrange
        string input = @"
        Doing the IDs
        %%=ContentBlockByID(1)=%%
        Id with Spaces
        %%=  ContentBlockByID  (  2  ) =%%
        with quotes
        %%=ContentBlockByID(""3"")=%%
        with quotes and spaces
        %%=  ContentBlockByID  (  ""4""  ) =%%
        with optional parameter region
        %%=ContentBlockByID(5, ""Region1"")=%%
        with optional parameter region and missing default value
        %%=ContentBlockByID(""6"", ""Region2"", false, ""Missing!"", -1)=%%
        with optional parameter region and missing default value
        %%=ContentBlockByID(7, ""Region3"", true, ""All good"", 0)=%%
    ";
        AssetPoco asset = new AssetPoco { Content = input };

        // Act
        var result = asset.GetContentBlocks();

        // Debug output
        Console.WriteLine("Found IDs:");
        foreach (var b in result)
        {
            Console.WriteLine($"result Id={b.Id}");
        }

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(7, result.Count);
        Assert.AreEqual(1, result[0].Id);
        Assert.AreEqual(2, result[1].Id);
        Assert.AreEqual(3, result[2].Id);
        Assert.AreEqual(4, result[3].Id);
        Assert.AreEqual(5, result[4].Id);
        Assert.AreEqual(6, result[5].Id);
        Assert.AreEqual(7, result[6].Id);
    }

    [TestMethod]
    public void FillContentBlocksAsyncTest()
    {
        var asset_Key_eq_ABC111 = new AssetPoco()
        {
            Id = 1,
            CustomerKey = "ABC-111",
            Name = "ABC111 Name",
            Content =
            @"
----------- BEGIN CustomerKey = ABC-111 -----------
This is ABC-111 Content #1
%%=ContentBlockByKey(""ABC-222"")=%%
This is ABC-111 Content #2

This is ABC-111 Content #3
----------- END CustomerKey = ABC-111 -----------
            "
        };

        var asset_Key_eq_ABC222 = new AssetPoco()
        {
            Id = 2,
            CustomerKey = "ABC-222",
            Name = "ABC222 Name",
            Content =
            @"
----------- BEGIN CustomerKey = ABC-222 -----------
This is ABC-222 Content #1
%%=ContentBlockByName(""MyContentBlock3"")=%%

This is ABC-222 Content #2
%%=ContentBlockById(4)=%%
----------- END CustomerKey = ABC-222 -----------
            "
        };

        var asset_Name_eq_MyContentBlock3 = new AssetPoco()
        {
            Id = 3,
            CustomerKey = "ABC-333",
            Name = "MyContentBlock3",
            Content =
            @"
----------- BEGIN Name = MyContentBlock3 -----------
This is MyContentBlock3 Content #1

This is MyContentBlock3 Content #2

This is MyContentBlock3 Content #3
----------- END Name = MyContentBlock3 -----------
            "
        };

        var asset_Id_eq_4 = new AssetPoco()
        {
            Id = 4,
            CustomerKey = "ABC-444",
            Name = "MyContentBlock4",
            Content =
            @"
----------- BEGIN Id = 4 -----------
This is ID=4 Content #1

This is ID=4 Content #2

This is ID=4 Content #3
----------- END Id = 4 -----------
            "
        };

        var mockApi = new Mock<IAssetRestApi>();
        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-111"), It.IsAny<string>()))
            .Returns(asset_Key_eq_ABC111);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-111"), It.IsAny<string>()))
            .ReturnsAsync(asset_Key_eq_ABC111);

        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-222"), It.IsAny<string>()))
            .Returns(asset_Key_eq_ABC222);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-222"), It.IsAny<string>()))
            .ReturnsAsync(asset_Key_eq_ABC222);

        mockApi
            .Setup(api => api.GetAsset(It.Is<int?>(id => id == 4), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(asset_Id_eq_4);
        mockApi
            .Setup(api => api.GetAssetAsync(It.Is<int?>(id => id == 4), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(asset_Id_eq_4);

        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.IsAny<string>(), "MyContentBlock3"))
            .Returns(asset_Name_eq_MyContentBlock3);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.IsAny<string>(), "MyContentBlock3"))
            .ReturnsAsync(asset_Name_eq_MyContentBlock3);

        var api = mockApi.Object;

        var asset222 = api.GetAsset(null, "ABC-222", null);
        Assert.AreEqual("ABC-222", asset222.CustomerKey);

        var expandedContent = asset_Key_eq_ABC111.GetExpandedContent(api);

        Console.WriteLine("------------------");
        Console.WriteLine(expandedContent);
        Console.WriteLine("------------------");

        Assert.IsTrue(expandedContent.Contains("ABC-111"));
        Assert.IsTrue(expandedContent.Contains("ABC-222"));
        Assert.IsTrue(expandedContent.Contains("MyContentBlock3"));
        Assert.IsTrue(expandedContent.Contains("ID=4"));
    }

    [TestMethod]
    public void FillContentBlocksAsync_with_Html_Views_Content_Test()
    {
        var asset_Key_eq_ABC111 = new AssetPoco()
        {
            Id = 1,
            CustomerKey = "ABC-111",
            Name = "ABC111 Name",
            Views = new()
            {
                Html = new()
                {
                    Content =
@"
----------- BEGIN CustomerKey = ABC-111 -----------
This is ABC-111 Content #1
%%=ContentBlockByKey(""ABC-222"")=%%
This is ABC-111 Content #2

This is ABC-111 Content #3
----------- END CustomerKey = ABC-111 -----------
"
                }
            }
        };

        var asset_Key_eq_ABC222 = new AssetPoco()
        {
            Id = 2,
            CustomerKey = "ABC-222",
            Name = "ABC222 Name",
            Views = new()
            {
                Html = new()
                {
                    Content =
@"
----------- BEGIN CustomerKey = ABC-222 -----------
This is ABC-222 Content #1
%%=ContentBlockByName(""MyContentBlock3"")=%%

This is ABC-222 Content #2
%%=ContentBlockById(4)=%%
----------- END CustomerKey = ABC-222 -----------
"
                }
            }
        };

        var asset_Name_eq_MyContentBlock3 = new AssetPoco()
        {
            Id = 3,
            CustomerKey = "ABC-333",
            Name = "MyContentBlock3",
            Views = new()
            {
                Html = new()
                {
                    Content =
            @"
----------- BEGIN Name = MyContentBlock3 -----------
This is MyContentBlock3 Content #1

This is MyContentBlock3 Content #2

This is MyContentBlock3 Content #3
----------- END Name = MyContentBlock3 -----------
            "
                }
            }
        };

        var asset_Id_eq_4 = new AssetPoco()
        {
            Id = 4,
            CustomerKey = "ABC-444",
            Name = "MyContentBlock4",
            Views = new()
            {
                Html = new()
                {
                    Content =
            @"
----------- BEGIN Id = 4 -----------
This is ID=4 Content #1

This is ID=4 Content #2

This is ID=4 Content #3
----------- END Id = 4 -----------
            "
                }
            }
        };

        var mockApi = new Mock<IAssetRestApi>();
        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-111"), It.IsAny<string>()))
            .Returns(asset_Key_eq_ABC111);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-111"), It.IsAny<string>()))
            .ReturnsAsync(asset_Key_eq_ABC111);

        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-222"), It.IsAny<string>()))
            .Returns(asset_Key_eq_ABC222);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-222"), It.IsAny<string>()))
            .ReturnsAsync(asset_Key_eq_ABC222);

        mockApi
            .Setup(api => api.GetAsset(It.Is<int?>(id => id == 4), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(asset_Id_eq_4);
        mockApi
            .Setup(api => api.GetAssetAsync(It.Is<int?>(id => id == 4), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(asset_Id_eq_4);

        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.IsAny<string>(), "MyContentBlock3"))
            .Returns(asset_Name_eq_MyContentBlock3);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.IsAny<string>(), "MyContentBlock3"))
            .ReturnsAsync(asset_Name_eq_MyContentBlock3);

        var api = mockApi.Object;

        var asset222 = api.GetAsset(null, "ABC-222", null);
        Assert.AreEqual("ABC-222", asset222.CustomerKey);

        var expandedContent = asset_Key_eq_ABC111.GetExpandedContent(api);
        Console.WriteLine("------------------");
        Console.WriteLine("FillContentBlocksAsync_with_Html_Views_Content_Test()");
        Console.WriteLine("------------------");
        Console.WriteLine(expandedContent);
        Console.WriteLine("------------------");

        Assert.IsTrue(expandedContent.Contains("ABC-111"));
        Assert.IsTrue(expandedContent.Contains("ABC-222"));
        Assert.IsTrue(expandedContent.Contains("MyContentBlock3"));
        Assert.IsTrue(expandedContent.Contains("ID=4"));
    }

    [TestMethod]
    public void PerformRegexReplacementAsyncTest()
    {
        Console.WriteLine("preparing mock data...");
        var asset_Key_eq_ABC111 = new AssetPoco()
        {
            Id = 1,
            CustomerKey = "ABC-111",
            Name = "ABC111 Name",
            Content =
            @"
----------- BEGIN CustomerKey = ABC-111 -----------
This is ABC-111 Content #1
%%=ContentBlockByKey(""ABC-222"")=%%
This is ABC-111 Content #2

This is ABC-111 Content #3
----------- END CustomerKey = ABC-111 -----------
            "
        };

        var asset_Key_eq_ABC222 = new AssetPoco()
        {
            Id = 2,
            CustomerKey = "ABC-222",
            Name = "ABC222 Name",
            Content =
            @"
----------- BEGIN CustomerKey = ABC-222 -----------
This is ABC-222 Content #1
This is ABC-222 Content #2
            "
        };


        var mockApi = new Mock<IAssetRestApi>();
        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-111"), It.IsAny<string>()))
            .Returns(asset_Key_eq_ABC111);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-111"), It.IsAny<string>()))
            .ReturnsAsync(asset_Key_eq_ABC222);

        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-222"), It.IsAny<string>()))
            .Returns(asset_Key_eq_ABC222);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.Is<string?>(k => k == "ABC-222"), It.IsAny<string>()))
            .ReturnsAsync(asset_Key_eq_ABC222);

        var api = mockApi.Object;

        Console.WriteLine("Before PerformRegexReplacement:");
        var result = AssetHelpers.PerformRegexReplacement(
            api,
            new ContentBlock
            {
                Key = "ABC-222",
            },
            asset_Key_eq_ABC111.Content);
        Console.WriteLine("------------------");
        Console.WriteLine(result);
        Console.WriteLine("------------------");
        Console.WriteLine("After FillContentExpandedAsync:");

        Assert.IsTrue(result.Contains("ABC-111"));
        Assert.IsTrue(result.Contains("ABC-222"));
    }

    [TestMethod]
    public void IntegrationTest()
    {
        // Arrange
        var asset1 = new AssetPoco
        {
            Id = 111,
            CustomerKey = "IntegrationTest-Key-111",
            ObjectID = "1111111-1111-1111-1111-111111111111",
            Name = "IntegrationTest-Name-111",
            AssetType = new()
            {
                Id = 208,
                Name = "htmlemail",
                DisplayName = "HTML Email"
            },
            Views = new()
            {
                Html = new()
                {
                    Content = @"
----------- BEGIN CustomerKey = IntegrationTest-111 -----------
This is IntegrationTest-111 Referencing Content #333

%%=ContentBlockByName(""IntegrationTest-Name-333"")=%%

This is IntegrationTest-111 Referencing Content #444
%%=ContentBlockByKey(""IntegrationTest-Key-444"")=%%

----------- END CustomerKey = IntegrationTest-111 -----------"
                }
            }
        };

        var asset3 = new AssetPoco
        {
            Id = 333,
            CustomerKey = "IntegrationTest-Key-333",
            ObjectID = "33333333-3333-3333-3333-333333333333",
            Name = "IntegrationTest-Name-333",
            AssetType = new()
            {
                Id = 220,
                Name = "codesnippetblock",
                DisplayName = "Code Snippet Block"
            },
            Content = @"
/***********/
IntegrationTest-Text-333
/***********/"
        };


        var asset4 = new AssetPoco
        {
            Id = 444,
            CustomerKey = "IntegrationTest-Key-444",
            ObjectID = "44444444-4444-4444-4444-444444444444",
            Name = "IntegrationTest-Name-444",
            AssetType = new()
            {
                Id = 220,
                Name = "codesnippetblock",
                DisplayName = "Code Snippet Block"
            },
            Content = @"
/***********/
IntegrationTest-Text-444
/***********/"
        };


        var mockApi = new Mock<IAssetRestApi>();
        // asset1
        mockApi
            .Setup(api => api.GetAsset(It.Is<int?>(id => id == 111), It.IsAny<string?>(), It.IsAny<string>()))
            .Returns(asset1);
        mockApi
            .Setup(api => api.GetAssetAsync(It.Is<int?>(id => id == 111), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(asset1);
        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.Is<string?>(k => k == "IntegrationTest-Key-111"), It.IsAny<string>()))
            .Returns(asset1);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.Is<string?>(k => k == "IntegrationTest-Key-111"), It.IsAny<string>()))
            .ReturnsAsync(asset1);
        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.IsAny<string?>(), It.Is<string?>(k => k == "IntegrationTest-Name-111")))
            .Returns(asset1);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.Is<string?>(k => k == "IntegrationTest-Name-111")))
            .ReturnsAsync(asset1);

        // asset3
        mockApi
            .Setup(api => api.GetAsset(It.Is<int?>(id => id == 333), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(asset3);
        mockApi
            .Setup(api => api.GetAssetAsync(It.Is<int?>(id => id == 333), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(asset3);
        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.Is<string?>(k => k == "IntegrationTest-Key-333"), It.IsAny<string>()))
            .Returns(asset3);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.Is<string?>(k => k == "IntegrationTest-Key-333"), It.IsAny<string>()))
            .ReturnsAsync(asset3);
        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.IsAny<string>(), It.Is<string?>(k => k == "IntegrationTest-Name-333")))
            .Returns(asset3);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.IsAny<string>(), It.Is<string?>(k => k == "IntegrationTest-Name-333")))
            .ReturnsAsync(asset3);

        // asset4
        mockApi
            .Setup(api => api.GetAsset(It.Is<int?>(id => id == 444), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(asset4);
        mockApi
            .Setup(api => api.GetAssetAsync(It.Is<int?>(id => id == 444), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(asset4);
        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.Is<string?>(k => k == "IntegrationTest-Key-444"), It.IsAny<string>()))
            .Returns(asset4);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.Is<string?>(k => k == "IntegrationTest-Key-444"), It.IsAny<string>()))
            .ReturnsAsync(asset4);
        mockApi
            .Setup(api => api.GetAsset(It.IsAny<int?>(), It.IsAny<string>(), It.Is<string?>(k => k == "IntegrationTest-Name-444")))
            .Returns(asset4);
        mockApi
            .Setup(api => api.GetAssetAsync(It.IsAny<int?>(), It.IsAny<string>(), It.Is<string?>(k => k == "IntegrationTest-Name-444")))
            .ReturnsAsync(asset4);

        var api = mockApi.Object;

        var retrievedAsset1 = api.GetAsset(111, null, null);
        Assert.IsNotNull(retrievedAsset1);
        Assert.AreEqual("IntegrationTest-Key-111", retrievedAsset1.CustomerKey);

        var expandedContent = asset1.GetExpandedContent(api);
        var contentBlocks = asset1.GetContentBlocks();

        /*
        Assert.AreEqual(1, contentBlocks.Count);
        Assert.AreEqual("IntegrationTest-Name-333", contentBlocks[0].Name);
        Assert.AreEqual("IntegrationTest-Key-333", contentBlocks[0].Key);
*/
        foreach (var contentBlock in contentBlocks)
        {
            Console.WriteLine($"Content Block: Name={contentBlock.Name}, Key={contentBlock.Key}, Id={contentBlock.Id}");
        }

        Console.WriteLine("------------------");
        Console.WriteLine("IntergrationTest() - Content Expanded");
        Console.WriteLine("------------------");
        Console.WriteLine(expandedContent);
        Console.WriteLine("------------------");

        Assert.IsTrue(expandedContent.Contains("IntegrationTest-Text-333"));
        Assert.IsTrue(expandedContent.Contains("IntegrationTest-Text-444"));

    }
    


    
}