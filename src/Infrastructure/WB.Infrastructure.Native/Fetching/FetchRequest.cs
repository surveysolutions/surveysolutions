using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq;
using WB.Core.Infrastructure.Fetching;

namespace WB.Infrastructure.Native.Fetching
{
    internal class FetchRequest<TQueried, TFetch> : IFetchRequest<TQueried, TFetch>
    {
        public FetchRequest(INhFetchRequest<TQueried, TFetch> nhFetchRequest)
        {
            this.NhFetchRequest = nhFetchRequest;
        }

        public INhFetchRequest<TQueried, TFetch> NhFetchRequest { get; }

        public IEnumerator<TQueried> GetEnumerator()
        {
            return this.NhFetchRequest.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.NhFetchRequest.GetEnumerator();
        }

        public Type ElementType => this.NhFetchRequest.ElementType;

        public Expression Expression => this.NhFetchRequest.Expression;

        public IQueryProvider Provider => this.NhFetchRequest.Provider;
    }
}