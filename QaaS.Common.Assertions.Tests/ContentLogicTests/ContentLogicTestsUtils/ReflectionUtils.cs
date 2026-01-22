using System.Reflection;
using QaaS.Common.Assertions.CommonAssertionsConfigs.ContentLogic;
using QaaS.Common.Assertions.ContentLogic;

namespace QaaS.Common.Assertions.Tests.ContentLogicTests.ContentLogicTestsUtils;

/// <summary>
/// Class of reflection utils. Being used to get information from the BaseOutputContentByExpectedResults.
/// Created to help in the testing of that assertion.
/// </summary>
/// <typeparam name="TConfig"> The type of the configuration that the assertion needs to have </typeparam>
internal static class ReflectionUtils<TConfig> where TConfig : OutputContentByExpectedResultsConfiguration, new()
{
    public static PropertyInfo GetPropertyInfo(string propertyName) =>
        typeof(BaseOutputContentByExpectedResults<TConfig>).GetProperty(propertyName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
    
    public static MethodInfo GetMethodInfo(string methodName) =>
        typeof(BaseOutputContentByExpectedResults<TConfig>).GetMethod(methodName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
}