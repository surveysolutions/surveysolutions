using Android.Content;
using Android.Database.Sqlite;

namespace AndroidApp.Core.Model.ProjectionStorage
{
    public class ProjectionsDataBaseHelper: SQLiteOpenHelper
	{
		public const string DATABASE_NAME = "Projections";
		public const int DATABASE_VERSION = 1;

        public ProjectionsDataBaseHelper(Context context) 
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
            db.ExecSQL(ProjectionQuery.CreateTables());
		}

		private void DropTables(SQLiteDatabase db)
		{
            db.ExecSQL(ProjectionQuery.DropTables());
		}
    }
}