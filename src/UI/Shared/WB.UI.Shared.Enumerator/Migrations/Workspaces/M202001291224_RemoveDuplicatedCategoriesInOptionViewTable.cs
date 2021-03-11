using SQLite;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Shared.Enumerator.Migrations.Workspaces
{
    [Migration(202001291224)]
    public class M202001291224_RemoveDuplicatedCategoriesInOptionViewTable : IMigration
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly SqliteSettings settings;

        public M202001291224_RemoveDuplicatedCategoriesInOptionViewTable(IFileSystemAccessor fileSystemAccessor, SqliteSettings settings)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.settings = settings;
        }

        public void Up()
        {
            var entityName = typeof(OptionView).Name;

            var pathToDatabase = fileSystemAccessor.CombinePath(settings.PathToDatabaseDirectory, entityName + "-data.sqlite3");

            var sqliteConnectionString = new SQLiteConnectionString(pathToDatabase,
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true, null);
            var connection = new SQLiteConnectionWithLock(sqliteConnectionString);

            connection.RunInTransaction(() =>
            {
                connection.Execute(@"DELETE FROM OptionView
                                        WHERE rowid NOT IN(
                                            SELECT MIN(rowid)
                                                FROM OptionView
                                                GROUP BY QuestionnaireId, QuestionId, CategoryId, TranslationId, Value, Title, SearchTitle, ParentValue
                                        );");
            });
        }
    }
}
