using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using WB.Core.Infrastructure.Backup;

namespace WB.UI.Interviewer.Implementations.PlainStorage
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
            this.connectionFactory = Mvx.GetSingleton<ISQLiteConnectionFactory>();
        }

        private string FullPathToDataBase
        {
            get
            {
                return
                    System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                                           this.dbName);
            }
        }

        private void WrapConnection(Action<ISQLiteConnection> action)
        {
            lock (this.locker)
            {
                using (var connection = this.connectionFactory.Create(this.FullPathToDataBase))
                {
                    connection.CreateTable<PlainStorageRow>();
                    action(connection);
                }
            }
        }

        private TResult WrapConnectionWithQuery<TResult>(Func<ITableQuery<PlainStorageRow>, TResult> query)
        {
            lock (this.locker)
            {
                using (var connection = this.connectionFactory.Create(this.FullPathToDataBase))
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
            return this.WrapConnectionWithQuery<IEnumerable<T>, T>((table) => table.Where(predExpr).ToList());
        }

        public void Remove(string id)
        {
            this.WrapConnection(c => c.Delete<PlainStorageRow>(id.ToString()));
        }

        public void Store(PlainStorageRow row)
        {
            this.WrapConnection(connection =>
            {
                connection.Delete<PlainStorageRow>(row.Id.ToString());
                connection.Insert(row);
            });
        }

        public void Store(IEnumerable<PlainStorageRow> rows)
        {
            this.WrapConnection(connection =>
            {
                foreach (PlainStorageRow row in rows)
                {
                    connection.Delete<PlainStorageRow>(row.Id.ToString());
                    connection.Insert(row);
                }
            });
        }

        public string GetPathToBackupFile()
        {
            return this.FullPathToDataBase;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var pathToEventStore = Path.Combine(path, this.dbName);

            lock (this.locker)
            {
                if (!File.Exists(pathToEventStore))
                    File.Delete(this.FullPathToDataBase);
                else
                    File.Copy(pathToEventStore, this.FullPathToDataBase, true);
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