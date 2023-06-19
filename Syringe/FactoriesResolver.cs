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

        var (constructor, argsFactories) = constructors.Select(constructor =>
        {
            var argsFactories = ResolveParameters(constructor, stack);
            return (constructor, argsFactories);
        }).FirstOrDefault<(ConstructorInfo constructor, Func<object?>[]? argsFactories)>(tuple => tuple.argsFactories is not null);

        return constructor is null ? null
            : () =>
            {
                var args = argsFactories!.Select(factory => factory()).ToArray();
                return constructor.Invoke(args);
            };
    }

    private Func<object?>[]? ResolveParameters(MethodBase method, Stack<Type> stack)
    {
        ParameterInfo[] parameters = method.GetParameters();
        var args = new Func<object?>[parameters.Length];

        bool success = Enumerable.Range(0, parameters.Length).All(i =>
        {
            Type parameterType = parameters[i].ParameterType;

            if (stack.Contains(parameterType))
                throw new InvalidOperationException(
                    $"Circular dependency detected: {string.Join(" -> ", stack)} -> {method.DeclaringType}");

            stack.Push(parameterType);
            Func<object>? parameterFactory = FindFactory(parameterType, stack);
            stack.Pop();

            if (parameterFactory is null)
                return false;

            args[i] = parameterFactory;
            return true;
        });

        return success ? args : null;
    }
}