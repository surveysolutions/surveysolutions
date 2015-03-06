using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public interface IRepository<T> {
        T GetById(string id);
        void Remove(string id);
        void Store(T view, string id);
        void BulkStore(List<Tuple<T, string>> bulk);
        void Clear();
        TResult Query<TResult>(Func<IQueryable<T>, TResult> query);
    }
}