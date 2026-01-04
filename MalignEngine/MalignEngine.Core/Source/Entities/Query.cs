
namespace MalignEngine;

public sealed class Query
{
    public IEnumerable<Type> All => all;
    public IEnumerable<Type> None => none;

    private readonly List<Type> all = new List<Type>();
    private readonly List<Type> none = new List<Type>();

    public Query() { }

    public Query Include(Type type)
    {
        all.Add(type);
        return this;
    }

    public Query Exclude(Type type)
    {
        none.Add(type);
        return this;
    }

    public Query Include<T>() => Include(typeof(T));
    public Query Exclude<T>() => Exclude(typeof(T));

    public Query WithAll<T1>() => Include(typeof(T1));
    public Query WithAll<T1, T2>() => Include(typeof(T1)).Include(typeof(T2));
    public Query WithAll<T1, T2, T3>() => Include(typeof(T1)).Include(typeof(T2)).Include(typeof(T3));
    public Query WithAll<T1, T2, T3, T4>() => Include(typeof(T1)).Include(typeof(T2)).Include(typeof(T3)).Include(typeof(T4));
}
