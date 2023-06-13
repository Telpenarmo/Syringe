using System.Reflection;

namespace Syringe;

internal class FactoriesResolver
{
    private readonly IReadOnlyDictionary<Type, Func<object>> factories;
    private readonly IConstructorsProvidingStrategy constructorsProvidingStrategy;

    public FactoriesResolver(IReadOnlyDictionary<Type, Func<object>> factories, IConstructorsProvidingStrategy constructorsProvidingStrategy)
    {
        this.factories = factories;
        this.constructorsProvidingStrategy = constructorsProvidingStrategy;
    }

    public Func<object>? FindFactory(Type type) => FindFactory(type, new Stack<Type>());
    
    private Func<object>? FindFactory(Type type, Stack<Type> stack)
    {
        if (type.IsAbstract)
            return null;

        if (factories.TryGetValue(type, out Func<object>? factory))
            return factory;

        IEnumerable<ConstructorInfo> constructors = constructorsProvidingStrategy.GetConstructors(type);

        object[]? args = null;

        var constructor = constructors.FirstOrDefault(constructor =>
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            args = new object[parameters.Length];

            return Enumerable.Range(0, parameters.Length).All(i =>
            {
                Type parameterType = parameters[i].ParameterType;

                if (stack.Contains(parameterType))
                    throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", stack)} -> {type}");

                stack.Push(parameterType);
                Func<object>? parameterFactory = FindFactory(parameterType, stack);
                stack.Pop();

                if (parameterFactory is null)
                    return false;

                args[i] = parameterFactory();
                return true;
            });
        });

        return constructor is null ? null
            : () => constructor.Invoke(args);
    }
}