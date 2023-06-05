namespace Syringe;

public class SimpleContainer
{
    private readonly Dictionary<Type, Func<object>> factories = new();

    public void RegisterType<T>(bool singleton = false) where T : new()
    {
        RegisterType<T, T>(singleton);
    }

    public void RegisterType<TFrom, TTo>(bool singleton = false) where TTo : TFrom, new()
    {
        RegisterType<TFrom, TTo>(() => new TTo(), singleton);
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
