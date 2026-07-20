using Apps.ServiceNow.Actions;
using Tests.ServiceNow.Base;

namespace Tests.ServiceNow;

[TestClass]
public class ActionTests : TestBase
{
    [TestMethod]
    public async Task Dynamic_handler_works()
    {
        var actions = new Actions(InvocationContext);

        await actions.Action();
    }
}
