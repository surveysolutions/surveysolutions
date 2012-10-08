using System;
using Android.Content;
using Android.Database.Sqlite;
using Android.Runtime;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
	public class DataBaseHelper : SQLiteOpenHelper
	{
		public const string DATABASE_NAME = "EventStore";
		public const int DATABASE_VERSION = 3;

		public DataBaseHelper(Context context) 
			: base(context, DATABASE_NAME, null, DATABASE_VERSION)
		{
		}

		public override void OnCreate(SQLiteDatabase db)
		{
			CreateTables(db);
		}

		public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
		{
			DropTables(db);

			CreateTables(db);
		}

		private void CreateTables(SQLiteDatabase db)
		{
			db.ExecSQL(Query.CreateTables());
		}

		private void DropTables(SQLiteDatabase db)
		{
			db.ExecSQL(Query.DropTables());
		}
	}
}