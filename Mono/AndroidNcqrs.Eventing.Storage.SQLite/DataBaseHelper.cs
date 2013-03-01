namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    using Android.Content;

    using Mono.Data.Sqlite;

    using File = System.IO.File;

    public class DataBaseHelper /*: SQLiteOpenHelper*/
    {
        public const string DATABASE_NAME = "EventStore";
        public const int DATABASE_VERSION = 1;
        private readonly string dbPath;

        public DataBaseHelper(Context context)
          //  : base(context, DATABASE_NAME, null, DATABASE_VERSION)
        {
            dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal), DATABASE_NAME);
            
            bool exists = File.Exists(dbPath);
            if (!exists)
            {
                SqliteConnection.CreateFile(dbPath);
                CreateTables();
            }
        }

        public void ExecuteCommand(string command)
        {
            using (var connection = new SqliteConnection("Data Source=" + dbPath))
            {
                connection.Open();
                using (var c = connection.CreateCommand())
                {
                    c.CommandText = command;
                    c.ExecuteNonQuery();
                }

            }
        }

        public void ExecuteCommandsInTrnsactionScope(IEnumerable<CommandWithParams> commands)
        {
            if (!commands.Any())
                return;

            try
            {
                using (var connection = new SqliteConnection("Data Source=" + dbPath))
                {
                    connection.Open();
                    var tran = connection.BeginTransaction();
                    foreach (CommandWithParams command in commands)
                    {
                        // var c = connection.CreateCommand();
                        using (var c = connection.CreateCommand())
                        {
                            c.Transaction = tran;

                            c.CommandText = command.Command;
                            c.CommandType = CommandType.Text;
                            foreach (var param in command.Paramaters)
                            {
                                c.Parameters.Add(param);
                            }
                            c.ExecuteNonQuery();
                        }
                    }
                    tran.Commit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }

        }

        public IEnumerable<object[]> QueryData(string query)
        {
            using (var connection = new SqliteConnection("Data Source=" + dbPath))
            {
                connection.Open();
                using (var c = connection.CreateCommand())
                {
                    c.CommandText = query;
                    var r = c.ExecuteReader();

                    while (r.Read())
                    {
                        var values = new object[r.FieldCount];
                        var i = r.GetValues(values);
                        yield return values;
                    }
                }
            }
        }

        private void CreateTables()
        {
            ExecuteCommand(Query.CreateTables());
        }

        private void DropTables()
        {
            ExecuteCommand(Query.DropTables());
        }
    }
}