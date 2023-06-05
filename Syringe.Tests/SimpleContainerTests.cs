namespace Syringe.Tests;

public class SimpleContainerTests
{
    [Fact]
    public void Resolving_Unregistered_Type_Throws()
    {
        var container = new SimpleContainer();
        Assert.Throws<InvalidOperationException>(() => container.Resolve<int>());
    }

    [Fact]
    public void Resolving_Registered_Type_Returns_Instance()
    {
        var container = new SimpleContainer();

        container.RegisterType<List<int>>();
        var list = container.Resolve<List<int>>();

        Assert.NotNull(list);
        Assert.IsType<List<int>>(list);
    }

    [Fact]
    public void Resolving_Registered_Type_With_Factory_Returns_Instance()
    {
        var container = new SimpleContainer();
        var list = new List<int>();
        container.RegisterType<List<int>, List<int>>(() => list);

        var resolved = container.Resolve<List<int>>();

        Assert.NotNull(resolved);
        Assert.Same(list, resolved);
    }

    [Fact]
    public void Resolving_Type_Registered_As_Singleton_Returns_Same_Instance()
    {
        var container = new SimpleContainer();
        container.RegisterType<List<int>, List<int>>(true);

        var list1 = container.Resolve<List<int>>();
        var list2 = container.Resolve<List<int>>();

        Assert.Same(list1, list2);
    }

    [Fact]
    public void Singleton_Is_Resolved_Lazily()
    {
        SimpleContainer container = new();

        List<int> x = new();

        container.RegisterType<List<int>, List<int>>(() =>
        {
            x.Add(1);
            return x;
        }, true);

        Assert.Empty(x);

        _ = container.Resolve<List<int>>();

        Assert.Single(x);

        _ = container.Resolve<List<int>>();

        Assert.Single(x);
    }

    [Fact]
    public void Resoling_Type_Registered_As_Instance_Returns_Same_Instance()
    {
        SimpleContainer container = new();
        List<int> list = new();
        container.RegisterInstance(list);

        List<int> resolved = container.Resolve<List<int>>();

        Assert.Same(list, resolved);
    }
}
