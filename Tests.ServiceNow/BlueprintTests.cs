using System.Reflection;
using Apps.ServiceNow.Actions;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Tests.ServiceNow;

[TestClass]
public class BlueprintTests
{
    [TestMethod]
    public void DownloadAndUploadContent_ReturnBlueprintCompatibleOutputs()
    {
        var actions = typeof(ArticleActions)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<BlueprintActionDefinitionAttribute>() is not null)
            .ToList();

        Assert.AreEqual(2, actions.Count, "Both the download and the upload content blueprint should be declared.");

        foreach (var action in actions)
        {
            var returnType = Unwrap(action.ReturnType);
            Console.WriteLine($"{action.Name} -> {returnType.Name}");

            Assert.IsTrue(typeof(IDownloadContentOutput).IsAssignableFrom(returnType),
                $"{action.Name} must return an IDownloadContentOutput so the blueprint stays wired up.");
        }
    }

    [TestMethod]
    public void UploadContent_ExposesTheWrittenLanguageVariant()
    {
        var method = typeof(ArticleActions).GetMethod(nameof(ArticleActions.UploadArticle))!;
        var returnType = Unwrap(method.ReturnType);

        Assert.IsNotNull(returnType.GetProperty("TargetEntryId"),
            "Upload has to surface the article it wrote, which is not always the root article.");
    }

    private static Type Unwrap(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>)
            ? type.GetGenericArguments()[0]
            : type;
}
