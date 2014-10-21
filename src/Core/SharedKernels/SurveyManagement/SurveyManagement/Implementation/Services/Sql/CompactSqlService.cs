using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql
{
    internal class CompactSqlService : ISqlService
    {
        private DbConnection dbConnection;
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

        public void ExecuteCommand(string commandText)
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
                  /*  if (dbConnection.State != System.Data.ConnectionState.Closed)
                    {
                        dbConnection.Close();
                    }*/

                    dbConnection.Dispose();
                    dbConnection = null;
                }
            }
        }
    }
}
