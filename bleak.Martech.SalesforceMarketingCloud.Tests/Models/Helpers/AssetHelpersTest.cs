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
            %%=ContentBlockByID(""1"")=%%
            Id with Spaces
            %%=  ContentBlockByID  (  ""2"" ) =%%
        ";
        AssetPoco asset = new AssetPoco
        {
            Content = input
        };

        // Act
        var result = asset.GetContentBlocks();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(6, result.Count);

        Assert.AreEqual
        (
            "Key1",
            result.Where(cb => cb.Key != null).Skip(0).First().Key
        );
        Assert.AreEqual
        (
            "Key2",
            result.Where(cb => cb.Key != null).Skip(1).First().Key
        );
        Assert.AreEqual
        (
            "Name1",
            result.Where(cb => cb.Name != null).Skip(0).First().Name
        );
        Assert.AreEqual
        (
            "Name2",
            result.Where(cb => cb.Name != null).Skip(1).First().Name
        );
        Assert.AreEqual
        (
            1,
            result.Where(cb => cb.Id != null).Skip(0).First().Id
        );
        Assert.AreEqual
        (
            2,
            result.Where(cb => cb.Id != null).Skip(1).First().Id
        );
    }





    [TestMethod]
    public void GetContentBlocks_ShouldReturnContentBlocks_WhenInputContainsNonNumericIds()
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
            %%=ContentBlockByID(""STRING_1"")=%%
            Id with Spaces
            %%=  ContentBlockByID  (  ""STRING_2"" ) =%%
        ";
        AssetPoco asset = new AssetPoco
        {
            Content = input
        };

        // Act
        var result = asset.GetContentBlocks();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(6, result.Count);

        // What happens when the ID is not numeric?
        Assert.AreEqual
        (
            null,
            result.Where(cb => cb.Id != null).Skip(0)?.FirstOrDefault()?.Id
        );
        Assert.AreEqual
        (
            null,
            result.Where(cb => cb.Id != null).Skip(1)?.FirstOrDefault()?.Id
        );
    }


    [TestMethod]
    public async Task FillContentBlocksAsyncTest()
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
----------- END CustomerKey = ABC-111 -----------
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
            Name = "MyyContentBlock4",
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
            .Setup
            (
                api => api.GetAsset(null, "ABC-111", null)
            )
            .Returns(asset_Key_eq_ABC111);

        mockApi
            .Setup
            (
                api => api.GetAsset(null, "ABC-222", null)
            )
            .Returns(asset_Key_eq_ABC222);
        mockApi
            .Setup
            (
                api => api.GetAsset(4, null, null)
            )
            .Returns(asset_Id_eq_4);
        mockApi
            .Setup
            (
                api => api.GetAsset(null, null, "MyContentBlock3")
            )
            .Returns(asset_Key_eq_ABC111);

        var api = mockApi.Object;

        asset_Key_eq_ABC111.FillContentExpandedAsync(api);

        Assert.IsTrue(asset_Key_eq_ABC111.ContentExpanded.Contains("ABC-111"));
        Assert.IsTrue(asset_Key_eq_ABC111.ContentExpanded.Contains("ABC-222"));
        Assert.IsTrue(asset_Key_eq_ABC111.ContentExpanded.Contains("MyContentBlock3"));
        Assert.IsTrue(asset_Key_eq_ABC111.ContentExpanded.Contains("ID=4"));
    }
}