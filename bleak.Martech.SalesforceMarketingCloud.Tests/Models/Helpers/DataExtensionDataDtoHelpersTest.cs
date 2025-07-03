using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using bleak.Martech.SalesforceMarketingCloud.Models.Helpers;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;

namespace bleak.Martech.SalesforceMarketingCloud.Tests.Models.Converters;

[TestClass]
public class DataExtensionDataDtoConvertersTest
{
    [TestMethod]
    public void ToModel_ShouldConvertDtoToModel()
    {
        // Arrange
        var dto = new DataExtensionDataDto
        {
            items = new List<ItemDto>
            {
                new ItemDto
                {
                    keys = new Dictionary<string, string> { { "Id", "123" } },
                    values = new Dictionary<string, string> { { "Name", "Test" } }
                },
                new ItemDto
                {
                    keys = new Dictionary<string, string> { { "Id", "111" } },
                    values = new Dictionary<string, string> { { "Name", "Test 2" } }
                }
            }
        };

        var results = DataExtensionDataDtoHelpers.ToDictionaryList(dto);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(dto.items.Count, results.Count);

        Assert.AreEqual("123", results[0]["Id"]);
        Assert.AreEqual("Test", results[0]["Name"]);

        Assert.AreEqual("111", results[1]["Id"]);
        Assert.AreEqual("Test 2", results[1]["Name"]);
    }

    [TestMethod]
    public void ToDictionaryList_ShouldReturnEmptyList_WhenItemsIsEmpty()
    {
        var dto = new DataExtensionDataDto
        {
            items = new List<ItemDto>()
        };

        var results = DataExtensionDataDtoHelpers.ToDictionaryList(dto);

        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void ToDictionaryList_ShouldHandleNullKeysAndValues()
    {
        var dto = new DataExtensionDataDto
        {
            items = new List<ItemDto>
            {
                new ItemDto
                {
                    keys = null,
                    values = null
                }
            }
        };

        var results = DataExtensionDataDtoHelpers.ToDictionaryList(dto);

        Assert.IsNotNull(results);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual(0, results[0].Count);
    }

    [TestMethod]
    public void ToDictionaryList_ShouldHandleOverlappingKeys()
    {
        var dto = new DataExtensionDataDto
        {
            items = new List<ItemDto>
            {
                new ItemDto
                {
                    keys = new Dictionary<string, string> { { "Key", "ValueFromKeys" } },
                    values = new Dictionary<string, string> { { "Key", "ValueFromValues" } }
                }
            }
        };

        var results = DataExtensionDataDtoHelpers.ToDictionaryList(dto);

        Assert.IsNotNull(results);
        Assert.AreEqual(1, results.Count);
        // Value from 'values' should overwrite value from 'keys'
        Assert.AreEqual("ValueFromValues", results[0]["Key"]);
    }

    [TestMethod]
    public void ToDictionaryList_ShouldHandleNullDto()
    {
        DataExtensionDataDto dto = null;
        Assert.ThrowsException<System.NullReferenceException>(() =>
        {
            DataExtensionDataDtoHelpers.ToDictionaryList(dto);
        });
    }
}
