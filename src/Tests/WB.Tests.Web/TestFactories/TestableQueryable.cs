using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class TestingQueryable<T> : IQueryable<T>
{
    private readonly IQueryable<T> _queryable;

    public TestingQueryable(IQueryable<T> queryable)
    {
        _queryable = queryable;     
        Provider = new TestingQueryProvider<T>(_queryable);
    }

    public Type ElementType => _queryable.ElementType;

    public Expression Expression =>  _queryable.Expression;

    public IQueryProvider Provider { get; }

    public IEnumerator<T> GetEnumerator()
    {
        return _queryable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _queryable.GetEnumerator();
    }
}
