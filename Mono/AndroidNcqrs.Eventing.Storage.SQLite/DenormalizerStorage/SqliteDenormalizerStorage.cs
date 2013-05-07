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

namespace AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage
{
    public class SqliteDenormalizerStorage<T> : IFilterableDenormalizerStorage<T>, IMvxServiceConsumer
        where T : DenormalizerRow, new()
    {
        //        private readonly ISQLiteConnectionFactory _connectionFactory;
        private readonly ISQLiteConnection _connection;
        private const string _dbName = "Projections";

        public SqliteDenormalizerStorage()
        {
            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            var connectionFactory = this.GetService<ISQLiteConnectionFactory>();
            _connection = connectionFactory.Create(_dbName);

            _connection.CreateTable<T>();
        }

        public int Count()
        {
            return _connection.Table<T>().Count();
        }

        public T GetByGuid(Guid key)
        {
            var idString = key.ToString();
            //  Expression<Func<T, bool>> exp = (i) => i.Id == key.ToString();
            return ((TableQuery<T>) _connection.Table<T>()).Where((i) => i.Id == idString).FirstOrDefault();
        }

        public IEnumerable<T> Query(Expression<Func<T, bool>> predExpr)
        {
            return ((TableQuery<T>) _connection.Table<T>()).Where(predExpr);
        }

        public void Remove(Guid key)
        {
            _connection.Delete<T>(key);
        }

        public void Store(T denormalizer, Guid key)
        {
            try
            {
                _connection.Insert(denormalizer);
            }
            catch
            {
                _connection.Update(denormalizer);
            }
        }
    }

    public abstract class DenormalizerRow
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}