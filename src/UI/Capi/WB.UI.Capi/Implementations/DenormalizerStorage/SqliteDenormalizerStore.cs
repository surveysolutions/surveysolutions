using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;

using WB.Core.Infrastructure.Backup;

namespace WB.UI.Capi.Implementations.DenormalizerStorage
{
    public class SqliteDenormalizerStore: IBackupable
    {
        private readonly string dbName;
        private readonly ISQLiteConnectionFactory connectionFactory;
        private object locker = new object();

        public SqliteDenormalizerStore(string dbName)
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

        private void WrapConnection<TView>(Action<ISQLiteConnection> action)
        {
            lock (this.locker)
            {
                using (
                    var connection = this.connectionFactory.Create(this.FullPathToDataBase))
                {
                    connection.CreateTable<TView>();
                    action(connection);
                }
            }
        }

        public TResult WrapConnectionWithQuery<TResult, TView>(Func<ITableQuery<TView>, TResult> query)
            where TView : DenormalizerRow, new()
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

        public int Count<TView>() where TView : DenormalizerRow, new()
        {
            return this.WrapConnectionWithQuery<int, TView>((_) => _.Count());
        }

        public TView GetById<TView>(string id) where TView : DenormalizerRow, new()
        {
            var idString = id.ToString();
             return this.WrapConnectionWithQuery<TView, TView>((_) => _.Where((i) => i.Id == idString).FirstOrDefault());
        }

        public IEnumerable<TView> Filter<TView>(Expression<Func<TView, bool>> predExpr) where TView : DenormalizerRow, new()
        {
            return this.WrapConnectionWithQuery<IEnumerable<TView>, TView>((table) => table.Where(predExpr).ToList());
        }

        public void Remove<TView>(string id) where TView : DenormalizerRow, new()
        {
            this.WrapConnection<TView>((c)=>c.Delete<TView>(id.ToString()));
        }

        public void Store<TView>(TView view, string id) where TView : DenormalizerRow, new()
        {
            this.WrapConnection<TView>((connection) =>
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
    }
}