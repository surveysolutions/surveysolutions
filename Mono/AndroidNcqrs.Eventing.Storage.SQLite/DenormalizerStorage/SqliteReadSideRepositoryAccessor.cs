using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Main.DenormalizerStorage;
using SQLite;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage
{
    public class SqliteReadSideRepositoryAccessor<TView> : IMvxServiceConsumer,
        IFilterableReadSideRepositoryReader<TView>, IFilterableReadSideRepositoryWriter<TView>
        where TView : DenormalizerRow, new()
    {
        //        private readonly ISQLiteConnectionFactory _connectionFactory;
        private readonly ISQLiteConnection _connection;
        private const string _dbName = "Projections";

        public SqliteReadSideRepositoryAccessor()
        {
            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            var connectionFactory = this.GetService<ISQLiteConnectionFactory>();
            _connection = connectionFactory.Create(_dbName);

            _connection.CreateTable<TView>();
        }

        public int Count()
        {
            return _connection.Table<TView>().Count();
        }

        public TView GetById(Guid id    )
        {
            var idString = id.ToString();
            //  Expression<Func<T, bool>> exp = (i) => i.Id == key.ToString();
            return ((TableQuery<TView>) _connection.Table<TView>()).Where((i) => i.Id == idString).FirstOrDefault();
        }

        public IEnumerable<TView> Filter(Expression<Func<TView, bool>> predExpr)
        {
            return ((TableQuery<TView>) _connection.Table<TView>()).Where(predExpr);
        }

        public void Remove(Guid id)
        {
            _connection.Delete<TView>(id.ToString());
        }

        public void Store(TView view, Guid id)
        {
            try
            {
                _connection.Insert(view);
            }
            catch
            {
                _connection.Update(view);
            }
        }
    }

    public abstract class DenormalizerRow : IView
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}