using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using WB.Core.Infrastructure.Backup;

namespace AndroidNcqrs.Eventing.Storage.SQLite.PlainStorage
{
    public class SqlitePlainStore : IBackupable
    {
        private readonly string dbName;
        private readonly ISQLiteConnectionFactory connectionFactory;
        private readonly object locker = new object();

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

        private void WrapConnection(Action<ISQLiteConnection> action)
        {
            lock (locker)
            {
                using (var connection = connectionFactory.Create(FullPathToDataBase))
                {
                    connection.CreateTable<PlainStorageRow>();
                    action(connection);
                }
            }
        }

        private TResult WrapConnectionWithQuery<TResult>(Func<ITableQuery<PlainStorageRow>, TResult> query)
        {
            lock (locker)
            {
                using (var connection = connectionFactory.Create(FullPathToDataBase))
                {
                    connection.CreateTable<PlainStorageRow>();
                    return query.Invoke(connection.Table<PlainStorageRow>());
                }
            }
        }

        public PlainStorageRow GetById(string id)
        {
            return this.WrapConnectionWithQuery(_ => _.Where(i => i.Id == id).FirstOrDefault());
        }
        
        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> predExpr) where T : new()
        {
            return WrapConnectionWithQuery<IEnumerable<T>, T>((table) => table.Where(predExpr).ToList());
        }

        public void Remove(string id)
        {
            WrapConnection(c => c.Delete<PlainStorageRow>(id.ToString()));
        }

        public void Store(PlainStorageRow row)
        {
            WrapConnection(connection =>
            {
                try
                {
                    connection.Insert(row);
                }
                catch
                {
                    connection.Update(row);
                }
            });
        }

        public void Store(IEnumerable<PlainStorageRow> rows)
        {
            WrapConnection(connection =>
            {
                foreach (PlainStorageRow row in rows)
                {
                    try
                    {
                        connection.Insert(row);
                    }
                    catch
                    {
                        connection.Update(row);
                    }
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

        private TResult WrapConnectionWithQuery<TResult, TView>(Func<ITableQuery<TView>, TResult> query)
            where TView : new()
        {
            lock (this.locker)
            {
                using (
                    var connection = this.connectionFactory.Create(this.FullPathToDataBase))
                {
                    connection.CreateTable<TView>();
                    return query.Invoke(connection.Table<TView>());
                }
            }
        }
    }
}