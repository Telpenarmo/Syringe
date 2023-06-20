namespace Syringe.Tests;

internal class Dependent
{
    public Dependency Dependency { get; }

    public Dependent(Dependency dependency)
    {
        Dependency = dependency;
    }
}

internal class Dependency
{
}

internal class A
{
    public B B { get; }
    public A(B b)
    {
        B = b;
    }
}

internal class B
{
    public C C { get; }
    public B(C c)
    {
        C = c;
    }
}

internal class C
{
    public A A { get; }
    public C(A a)
    {
        A = a;
    }
}

internal class X
{
    [Dependency]
    public Y? Y { get; set; }
}

internal class Y
{
    [Dependency]
    public X? X { get; set; }
}

internal class ManyConstructors
{
    public ManyConstructors(string s)
    {
        throw new Exception("Wrong constructor");
    }

    [DependencyConstructor]
    public ManyConstructors(int x)
    {
        throw new Exception("Wrong constructor");
    }

    public ManyConstructors(int x, string s)
    {
        throw new Exception("Wrong constructor");
    }

    [DependencyConstructor]
    public ManyConstructors(int x, int y)
    { }
}