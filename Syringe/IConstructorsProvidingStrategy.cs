using System.Reflection;

namespace Syringe;

public interface IConstructorsProvidingStrategy
{
    IEnumerable<ConstructorInfo> GetConstructors(Type type);
}
