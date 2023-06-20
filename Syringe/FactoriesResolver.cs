using System.Reflection;

namespace Syringe;

internal class FactoriesResolver
{
    private readonly Dictionary<Type, Func<object>> factories;
    private readonly IConstructorsProvidingStrategy constructorsProvidingStrategy;

    public FactoriesResolver(Dictionary<Type, Func<object>> factories, IConstructorsProvidingStrategy constructorsProvidingStrategy)
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
        }).FirstOrDefault(tuple => tuple.argsFactories is not null);

        return constructor is null ? null
            : () =>
            {
                var args = argsFactories!.Select(factory => factory()).ToArray();
                var obj = constructor.Invoke(args);

                factories.TryGetValue(type, out Func<object>? oldFactory);
                factories[type] = () => obj;

                Augment(obj);

                if (oldFactory is not null)
                    factories[type] = oldFactory;
                else
                    factories.Remove(type);

                return obj;
            };
    }

    public void Augment(object obj)
    {
        Type t = obj.GetType();

        IEnumerable<MethodInfo> methods = t.GetRuntimeMethods()
            .Where(m => m.GetCustomAttribute<DependencyAttribute>() is not null);

        IEnumerable<MethodInfo> properties = t.GetRuntimeProperties()
            .Where(p => p.CanWrite)
            .Where(m => m.GetCustomAttribute<DependencyAttribute>() is not null)
            .Select(p => p.SetMethod!);

        var dependencies = methods.Union(properties)
            .Select(method => (method, argsFactories: ResolveParameters(method, new Stack<Type>())))
            .Where(tuple => tuple.argsFactories is not null);

        foreach (var (method, argsFactories) in dependencies)
        {
            object?[] args = argsFactories!.Select(factory => factory()).ToArray();
            _ = method.Invoke(obj, args!);
        }
    }

    private Func<object?>[]? ResolveParameters(MethodBase method, Stack<Type> stack)
    {
        ParameterInfo[] parameters = method.GetParameters();
        Func<object?>[] args = new Func<object?>[parameters.Length];

        bool success = Enumerable.Range(0, parameters.Length).All(i =>
        {
            Type parameterType = parameters[i].ParameterType;

            if (stack.Contains(parameterType))
                throw new InvalidOperationException(
                    $"Circular dependency detected: {string.Join(" -> ", stack)} -> {method.DeclaringType}");

            stack.Push(parameterType);
            Func<object>? parameterFactory = FindFactory(parameterType, stack);
            _ = stack.Pop();

            if (parameterFactory is null)
                return false;

            args[i] = parameterFactory;
            return true;
        });

        return success ? args : null;
    }
}
