namespace Syringe.Tests;

public class MembersInjectingTests
{
    [Fact]
    public void Using_DependencyAttribute_breaks_dependency_circle()
    {
        SimpleContainer container = new();
        container.RegisterType<X>();
        container.RegisterType<Y>();

        X x = container.Resolve<X>();
        Y y = container.Resolve<Y>();

        Assert.NotNull(x.Y);
        Assert.NotNull(y.X);

        // Assert.Same(x, y.X);
        // Assert.Same(y, x.Y);

        Assert.Same(x, x.Y.X);
        Assert.Same(y, y.X.Y);
    }

    [Fact]
    public void Augment_sets_dependencies()
    {
        SimpleContainer container = new();

        X x = new();

        Assert.Null(x.Y);

        container.Augment(x);

        Assert.NotNull(x.Y);
    }
}