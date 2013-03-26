// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectionsDataBaseHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The projections data base helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Android.Content;
using Android.Database.Sqlite;

namespace CAPI.Android.Core.Model.ProjectionStorage
{
    /// <summary>
    /// The projections data base helper.
    /// </summary>
    public class ProjectionsDataBaseHelper : SQLiteOpenHelper
    {
        #region Constants

        /// <summary>
        /// The databas e_ name.
        /// </summary>
        public const string DATABASE_NAME = "Projections";

        /// <summary>
        /// The databas e_ version.
        /// </summary>
        public const int DATABASE_VERSION = 1;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionsDataBaseHelper"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public ProjectionsDataBaseHelper(Context context)
            : base(context, DATABASE_NAME, null, DATABASE_VERSION)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on create.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public override void OnCreate(SQLiteDatabase db)
        {
            this.CreateTables(db);
        }

        /// <summary>
        /// The on upgrade.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        /// <param name="oldVersion">
        /// The old version.
        /// </param>
        /// <param name="newVersion">
        /// The new version.
        /// </param>
        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            //create smart udate procedure depending on versions 
            
            //this.DoUpdate(db, oldVersion, newVersion);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The create tables.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        private void CreateTables(SQLiteDatabase db)
        {
            db.ExecSQL(ProjectionQuery.CreateTables());
        }

        /// <summary>
        /// The do update.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        /// <param name="oldVersion">
        /// The old version.
        /// </param>
        /// <param name="newVersion">
        /// The new version.
        /// </param>
        private void DoUpdate(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            this.DropTables(db);
            this.CreateTables(db);
        }

        /// <summary>
        /// The drop tables.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        private void DropTables(SQLiteDatabase db)
        {
            db.ExecSQL(ProjectionQuery.DropTables());
        }
        
        #endregion
    }
}