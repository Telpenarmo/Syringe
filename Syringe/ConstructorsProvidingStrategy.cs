using System.Reflection;

namespace Syringe;

internal class ConstructorsProvidingStrategy : IConstructorsProvidingStrategy
{
    public IEnumerable<ConstructorInfo> GetConstructors(Type type)
    {
        return type.GetConstructors()
            .OrderBy(c => c.GetCustomAttribute<DependencyConstructorAttribute>() is null)
            .OrderByDescending(c => c.GetParameters().Length);
    }
}
