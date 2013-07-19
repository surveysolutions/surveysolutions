using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Plugins.Sqlite;
using SQLite;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage
{
    public class SqliteReadSideRepositoryAccessor<TView> : IMvxServiceConsumer,IBackupable,
        IFilterableReadSideRepositoryReader<TView>, IFilterableReadSideRepositoryWriter<TView>
        where TView : DenormalizerRow, new()
    {
        private ISQLiteConnection connection;
        private readonly string dbName;
        public SqliteReadSideRepositoryAccessor(string dbName)
        {
            this.dbName=dbName;
            Cirrious.MvvmCross.Plugins.Sqlite.PluginLoader.Instance.EnsureLoaded();
            var connectionFactory = this.GetService<ISQLiteConnectionFactory>();
            connection = connectionFactory.Create(dbName);

            connection.CreateTable<TView>();
        }

        public int Count()
        {
            return connection.Table<TView>().Count();
        }

        public TView GetById(Guid id)
        {
            var idString = id.ToString();
            //  Expression<Func<T, bool>> exp = (i) => i.Id == key.ToString();
            return ((TableQuery<TView>) connection.Table<TView>()).Where((i) => i.Id == idString).FirstOrDefault();
        }

        public IEnumerable<TView> Filter(Expression<Func<TView, bool>> predExpr)
        {
            return ((TableQuery<TView>) connection.Table<TView>()).Where(predExpr);
        }

        public void Remove(Guid id)
        {
            connection.Delete<TView>(id.ToString());
        }

        public void Store(TView view, Guid id)
        {
            try
            {
                connection.Insert(view);
            }
            catch
            {
                connection.Update(view);
            }
        }

        public string GetPathToBakupFile()
        {
            return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                                          this.dbName);
        }

        public void RestoreFromBakupFolder(string path)
        {
            connection.Close();

            File.Copy(Path.Combine(path, dbName),
                      System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                                          this.dbName), true);
            var connectionFactory = this.GetService<ISQLiteConnectionFactory>();
            connection = connectionFactory.Create(dbName);
        }
    }

    public abstract class DenormalizerRow : IView
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}