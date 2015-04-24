using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Transactions;
using Dapper;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql
{
    internal class SqliteService : ISqlService
    {
        private IDbConnection dbConnection;
        private TransactionScope scope;

        public SqliteService(string pathToDb, IFileSystemAccessor fileSystemAccessor)
        {
            string connectionString = "Data Source=" + pathToDb;

            if (!fileSystemAccessor.IsFileExists(pathToDb))
            {
                SQLiteConnection.CreateFile(pathToDb);
            }

            dbConnection = new SQLiteConnection(connectionString);
            scope = new TransactionScope(TransactionScopeOption.RequiresNew);
            dbConnection.Open();
        }

        public IEnumerable<dynamic> Query(string sql, object param = null)
        {
            return dbConnection.Query(sql, param);
        }

        public IEnumerable<T> Query<T>(string sql, object param = null) where T : class
        {
            return dbConnection.Query<T>(sql, param);
        }

        public void ExecuteCommand(string sql, object param = null)
        {
            dbConnection.Execute(sql, param);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (scope != null)
                {
                    scope.Complete();
                    scope.Dispose();
                    scope = null;
                }
                if (dbConnection != null)
                {
                    dbConnection.Dispose();
                    dbConnection = null;
                }
            }
        }
    }
}
