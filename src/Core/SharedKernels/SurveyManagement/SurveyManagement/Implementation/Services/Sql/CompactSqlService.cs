using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql
{
    internal class CompactSqlService : ISqlService
    {
        private IDbConnection dbConnection;
        private TransactionScope scope;
        public CompactSqlService(string pathToDb, IFileSystemAccessor fileSystemAccessor)
        {
            string connectionString = "DataSource=\"" + pathToDb + "\"";
            if (!fileSystemAccessor.IsFileExists(pathToDb))
            {
                using (var en = new SqlCeEngine(connectionString))
                {
                    en.CreateDatabase();
                }
            }
            dbConnection = new SqlCeConnection(connectionString);
            scope = new TransactionScope(TransactionScopeOption.RequiresNew);
            dbConnection.Open();
        }

       /* public void ExecuteCommand(string commandText)
        {
            ExecuteCommands(new[] { commandText });
        }

        public void ExecuteCommands(IEnumerable<string> commands)
        {
            foreach (var commandText in commands)
            {
                using (var command = dbConnection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
            }
        }

        public object[][] ExecuteReader(string query)
        {
            var result = new List<object[]>();
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandText = query;
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var row = new object[dr.FieldCount];
                        for (int i = 0; i < dr.FieldCount; i++)
                        {
                            row[i] = dr[i];
                        }
                        result.Add(row);
                    }
                }
            }

            return result.ToArray();
        }*/

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
