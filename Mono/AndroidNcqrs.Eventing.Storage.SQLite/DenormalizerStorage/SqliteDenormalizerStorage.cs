using System;
using System.Collections.Generic;
using System.Linq;
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

namespace AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage
{
    public class SqliteDenormalizerStorage<T> : IDenormalizerStorage<T>, IMvxServiceConsumer
        where T : DenormalizerRow, new()
    {
        private readonly ISQLiteConnectionFactory _connectionFactory;
        private const string _dbName="Projections";
        public SqliteDenormalizerStorage()
        {
            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            _connectionFactory = this.GetService<ISQLiteConnectionFactory>();
            WrapConnection((c) => c.CreateTable<T>());
        }

        public int Count()
        {
            return WrapConnection((c) => c.Table<T>().Count());
        }

        public T GetByGuid(Guid key)
        {
            return WrapConnection((c) => c.Table<T>().FirstOrDefault(x => x.Id == key.ToString()));
        }

        public IQueryable<T> Query()
        {
            return WrapConnection((c) => c.Table<T>().ToList().AsQueryable());
        }

        public void Remove(Guid key)
        {
            WrapConnection((c) => c.Delete<T>(key));
        }

        public void Store(T denormalizer, Guid key)
        {
            WrapConnection((c) =>
                {
                    try
                    {
                        c.Insert(denormalizer);
                    }
                    catch
                    {
                        c.Update(denormalizer);
                    }
                    return 0;
                })
                ;
        }

        private TOut WrapConnection<TOut>(Func<ISQLiteConnection, TOut> action)
        {
            using (var connection = _connectionFactory.Create(_dbName))
            {
                return action(connection);
            }
        }
    }

    public abstract class DenormalizerRow
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}