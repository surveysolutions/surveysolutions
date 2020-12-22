using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Type;

namespace WB.Tests.Abc
{
    public class TestingQueryProvider<T> : INhQueryProvider
    {
        public TestingQueryProvider(IQueryable<T> source)
        {
            Source = source;
        }

        public IQueryable<T> Source { get; set; }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestingQueryable<TElement>(Source.Provider.CreateQuery<TElement>(expression));
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return Source.Provider.Execute<TResult>(expression);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute<TResult>(expression));
        }

        public int ExecuteDml<T1>(QueryMode queryMode, Expression expression)
        {
            throw new NotImplementedException();
        }

        public Task<int> ExecuteDmlAsync<T1>(QueryMode queryMode, Expression expression, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IFutureEnumerable<TResult> ExecuteFuture<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IFutureValue<TResult> ExecuteFutureValue<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public void SetResultTransformerAndAdditionalCriteria(IQuery query, NhLinqExpression nhExpression, IDictionary<string, Tuple<object, IType>> parameters)
        {
            throw new NotImplementedException();
        }
    }
}
