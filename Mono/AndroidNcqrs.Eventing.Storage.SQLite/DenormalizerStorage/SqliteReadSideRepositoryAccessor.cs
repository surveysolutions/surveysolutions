using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using SQLite;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide;
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

        public TView GetById(Guid id)
        {
            return this.documentStore.GetById<TView>(id);
        }

        public IEnumerable<TView> Filter(Expression<Func<TView, bool>> predExpr)
        {
            return this.documentStore.Filter(predExpr);
        }

        public void Remove(Guid id)
        {
            this.documentStore.Remove<TView>(id);
        }

        public void Store(TView view, Guid id)
        {
            this.documentStore.Store<TView>(view,id);
        }
    }

    public abstract class DenormalizerRow : IView
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}