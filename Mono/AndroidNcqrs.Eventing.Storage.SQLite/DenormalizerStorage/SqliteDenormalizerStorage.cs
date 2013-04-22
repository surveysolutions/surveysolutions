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
        private readonly ISQLiteConnection _connection;

        public SqliteDenormalizerStorage()
        {
            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            var factory = this.GetService<ISQLiteConnectionFactory>();
            _connection = factory.Create("DenormalizerStorage");

            _connection.CreateTable<T>();
        }

        public int Count()
        {
            return _connection.Table<T>().Count();
        }

        public T GetByGuid(Guid key)
        {
            return _connection.Table<T>().FirstOrDefault(x => x.Id == key);
        }

        public IQueryable<T> Query()
        {
            return _connection.Table<T>().AsQueryable();
        }

        public void Remove(Guid key)
        {
            _connection.Delete<T>(key);
        }

        public void Store(T denormalizer, Guid key)
        {
            _connection.Insert(denormalizer);
        }
    }

    public abstract class DenormalizerRow
    {
        [PrimaryKey]
        public Guid Id { get; set; }
    }
}