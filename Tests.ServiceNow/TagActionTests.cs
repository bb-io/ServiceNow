using Apps.ServiceNow.Actions;
using Apps.ServiceNow.Models.Requests.Tag;
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
}