using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage
{
    public class SqliteReadSideRepositoryAccessor<TView> : 
        IFilterableReadSideRepositoryReader<TView>, IFilterableReadSideRepositoryWriter<TView>
        where TView : DenormalizerRow, new()
    {
        private readonly SqliteDenormalizerStore documentStore;
        public SqliteReadSideRepositoryAccessor(SqliteDenormalizerStore documentStore)
        {
            this.documentStore = documentStore;
        }

        public int Count()
        {
            return this.documentStore.Count<TView>();
        }

        public TView GetById(string id)
        {
            return this.documentStore.GetById<TView>(id);
        }

        public IEnumerable<TView> Filter(Expression<Func<TView, bool>> predExpr)
        {
            return this.documentStore.Filter(predExpr);
        }

        public void Remove(string id)
        {
            this.documentStore.Remove<TView>(id);
        }

        public void Store(TView view, string id)
        {
            this.documentStore.Store<TView>(view,id);
        }

        public void BulkStore(List<Tuple<TView, string>> bulk)
        {
            foreach (var tuple in bulk)
            {
                this.Store(tuple.Item1, tuple.Item2);
            }
        }

        public Type ViewType
        {
            get { return typeof (TView); }
        }

        public string GetReadableStatus()
        {
            return "SQLite";
        }
    }
}