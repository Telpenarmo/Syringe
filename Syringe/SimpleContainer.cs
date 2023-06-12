using System.Reflection;

namespace Syringe;

public class SimpleContainer
{
    private readonly Dictionary<Type, Func<object>> factories = new();

    public void RegisterType<T>(bool singleton = false)
    {
        RegisterType<T, T>(singleton);
    }

    public void RegisterType<TFrom, TTo>(bool singleton = false) where TTo : TFrom
    {
        var factory = FindConstructor(typeof(TFrom));
        if (factory is null)
            throw new InvalidOperationException($"No constructor for {typeof(TFrom)}");
        RegisterType<TFrom, TTo>(() => (TTo)factory(), singleton);
    }

    public void RegisterInstance<T>(T instance)
    {
        RegisterType<T, T>(() => instance, false);
    }

    public void RegisterType<TFrom, TTo>(Func<TTo> factory, bool singleton = false) where TTo : TFrom
    {
        if (singleton)
        {
            Lazy<TTo> lazy = new(factory);
            factory = () => lazy.Value;
        }
        factories[typeof(TFrom)] = () => factory()!;
    }

    public T Resolve<T>()
    {
        return factories.TryGetValue(typeof(T), out Func<object>? factory)
            ? (T)factory()
            : throw new InvalidOperationException($"No factory for {typeof(T)}");
    }

    private IEnumerable<ConstructorInfo> GetSortedConstructors(Type type)
    {
        return type.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length);
    }

    private Func<object>? FindConstructor(Type type)
    {
        if (type.IsAbstract)
            return null;

        if (factories.TryGetValue(type, out Func<object>? factory))
            return factory;

        var constructors = GetSortedConstructors(type);

        object[]? args = null;

        var constructor = constructors.FirstOrDefault(constructor =>
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            args = new object[parameters.Length];
            return Enumerable.Range(0, parameters.Length).All(i =>
            {
                Func<object>? parameterFactory = FindConstructor(parameters[i].ParameterType);
                if (parameterFactory is null)
                    return false;
                args[i] = parameterFactory();
                return true;
            });
        });
        
        if (constructor is null)
            return null;

        factory = () => constructor.Invoke(args);
        factories[type] = factory;
        return factory;
    }
}
