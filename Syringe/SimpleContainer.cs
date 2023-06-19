using System.Reflection;

namespace Syringe;

public class SimpleContainer
{
    private readonly Dictionary<Type, Func<object>> factories = new();
    internal readonly FactoriesResolver factoriesResolver;

    public SimpleContainer(IConstructorsProvidingStrategy constructorsProvidingStrategy)
    {
        factoriesResolver = new FactoriesResolver(factories, constructorsProvidingStrategy);
    }

    public SimpleContainer() : this(new ConstructorsProvidingStrategy())
    {
    }

    public void RegisterType<T>(bool singleton = false)
    {
        RegisterType<T, T>(singleton);
    }

    public void RegisterType<TFrom, TTo>(bool singleton = false) where TTo : TFrom
    {
        var factory = factoriesResolver.FindFactory(typeof(TFrom))
            ?? throw new InvalidOperationException($"No constructor for {typeof(TFrom)}");
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
}
