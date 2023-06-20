namespace Syringe.Tests;

public class SimpleContainerSingletonTests
{
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
}
