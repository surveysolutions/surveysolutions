using System;
using Android.Database.Sqlite;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
	public class SQLiteContext
	{
		private readonly SQLiteOpenHelper _database;

		public SQLiteContext(SQLiteOpenHelper database)
		{
			_database = database;
		}

		public void ExecuteWithTransaction(Action action)
		{
			_database.WritableDatabase.BeginTransaction();
			try
			{
				action();
				_database.WritableDatabase.SetTransactionSuccessful();
			}
			finally
			{
				_database.WritableDatabase.EndTransaction();
			}
		}
	}
}