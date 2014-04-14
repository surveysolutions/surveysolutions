using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using WB.Core.Infrastructure.Backup;

namespace AndroidNcqrs.Eventing.Storage.SQLite.PlainStorage
{
    public class SqlitePlainStore : IBackupable
    {
        private readonly string dbName;
        private readonly ISQLiteConnectionFactory connectionFactory;
        private object locker = new object();

        public SqlitePlainStore(string dbName)
        {
            this.dbName = dbName;

            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            connectionFactory = Mvx.GetSingleton<ISQLiteConnectionFactory>();
        }

        private string FullPathToDataBase
        {
            get
            {
                return
                    System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                                           dbName);
            }
        }

        private void WrapConnection<TEntity>(Action<ISQLiteConnection> action)
        {
            lock (locker)
            {
                using (
                    var connection = connectionFactory.Create(FullPathToDataBase))
                {
                    connection.CreateTable<TEntity>();
                    action(connection);
                }
            }
        }

        private TResult WrapConnectionWithQuery<TResult, TEntity>(Func<ITableQuery<TEntity>, TResult> query)
            where TEntity : PlainStorageRow, new()
        {
            lock (locker)
            {
                using (
                    var connection = connectionFactory.Create(FullPathToDataBase))
                {
                    connection.CreateTable<TEntity>();
                    return query.Invoke(connection.Table<TEntity>());
                }
            }
        }

        public TEntity GetById<TEntity>(string id) where TEntity : PlainStorageRow, new()
        {
            var idString = id.ToString();
            return WrapConnectionWithQuery<TEntity, TEntity>((_) => _.Where((i) => i.Id == idString).FirstOrDefault());
        }

        public void Remove<TEntity>(string id) where TEntity : PlainStorageRow, new()
        {
            WrapConnection<TEntity>((c) => c.Delete<TEntity>(id.ToString()));
        }

        public void Store<TEntity>(TEntity view, string id) where TEntity : PlainStorageRow, new()
        {
            WrapConnection<TEntity>((connection) =>
            {

                try
                {
                    connection.Insert(view);
                }
                catch
                {
                    connection.Update(view);
                }
            });
        }

        public string GetPathToBackupFile()
        {
            return FullPathToDataBase;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var pathToEventStore = Path.Combine(path, dbName);

            lock (locker)
            {
                if (!File.Exists(pathToEventStore))
                    File.Delete(FullPathToDataBase);
                else
                    File.Copy(pathToEventStore, FullPathToDataBase, true);
            }
        }
    }
}