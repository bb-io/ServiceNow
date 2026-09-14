using Apps.ServiceNow.Actions;
using Apps.ServiceNow.Models.Identifiers;
using Apps.ServiceNow.Models.Requests.Tag;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Tests.ServiceNow.Base;

namespace Tests.ServiceNow;

[TestClass]
public class TagActionTests : TestBase
{
    private TagActions Actions => new(InvocationContext);
    
    [TestMethod]
    public async Task SearchTags_ReturnsTags()
    {
        // Arrange
        var input = new SearchTagsRequest
        {
            NameContains = "it"
        };
        
        // Act
        var result = await Actions.SearchTags(input);

        // Assert
        PrintJsonResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetTag_ReturnsTag()
    {
        // Arrange
        var tagInput = new TagIdentifier { TagId = "fed5da12471331007f47563dbb9a7184" };
        
        // Act
        var result = await Actions.GetTag(tagInput);

        // Assert
        PrintJsonResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task CreateTag_ReturnsCreatedTag()
    {
        // Arrange
        var createInput = new CreateTagRequest
        {
            TagName = "test from tests"
        };

        // Act
        var result = await Actions.CreateTag(createInput);

        // Assert
        PrintJsonResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task DeleteTag_IsSuccess()
    {
        // Arrange
        var tagInput = new TagIdentifier { TagId = "07aeb7b5935b0f106a23f41add03d662" };

        // Act
        await Actions.DeleteTag(tagInput);

        // Assert
        await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => Actions.GetTag(tagInput));
    }
}