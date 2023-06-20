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
    public void Resolving_Type_Registered_As_Instance_Returns_Same_Instance()
    {
        SimpleContainer container = new();
        List<int> list = new();
        container.RegisterInstance(list);

        List<int> resolved = container.Resolve<List<int>>();

        Assert.Same(list, resolved);
    }

    [Fact]
    public void Registering_the_same_type_twice_overrides_the_previous_registration()
    {
        SimpleContainer container = new();
        List<int> list1 = new();
        container.RegisterInstance(list1);
        List<int> list2 = new();
        container.RegisterInstance(list2);

        Assert.Same(list2, container.Resolve<List<int>>());
    }

    [Fact]
    public void Resolving_Type_With_Dependencies_Returns_Instance()
    {
        SimpleContainer container = new();
        container.RegisterType<Dependent>();

        var dependent = container.Resolve<Dependent>();

        Assert.NotNull(dependent);
        Assert.NotNull(dependent.Dependency);
    }

    [Fact]
    public void Resolving_Type_With_Circular_Dependencies_Throws()
    {
        SimpleContainer container = new();
        Assert.Throws<InvalidOperationException>(() => container.RegisterType<A>());
    }

    [Fact]
    public void With_multiple_constructors_chooses_by_attribute_and_parameters_count()
    {
        SimpleContainer container = new();
        container.RegisterInstance("");
        container.RegisterInstance(0);

        // should not throw
        container.RegisterType<ManyConstructors>();
    }
}
