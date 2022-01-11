using SQLite;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Shared.Enumerator.Migrations.Workspaces
{
    [Migration(201912091307)]
    public class M201912091307_ChangeIdTypeInOptionViewTable : IMigration
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly SqliteSettings settings;

        public class New
        {
            public class OptionViewNew : IPlainStorageEntity<int?>
            {
                [PrimaryKey, AutoIncrement]
                public int? Id { get; set; }

                [Indexed(Name = "entity_idx", Order = 1)]
                public string QuestionnaireId { get; set; }

                [Indexed(Name = "entity_idx", Order = 2)]
                public string QuestionId { get; set; }

                [Indexed(Name = "entity_idx", Order = 3)]
                public string CategoryId { get; set; }

                [Indexed(Name = "entity_idx", Order = 4)]
                public string TranslationId { get; set; }

                [Indexed(Name = "OptionView_Value")]
                public decimal Value { get; set; }

                public string Title { get; set; }

                public string SearchTitle { get; set; }

                [Indexed(Name = "OptionView_ParentValue")]
                public decimal? ParentValue { get; set; }

                public int SortOrder { get; set; }
            }
        }       
        
        public class Old
        {
            public class OptionView : IPlainStorageEntity
            {
                [PrimaryKey]
                public string Id { get; set; }

                [Indexed]
                public string QuestionnaireId { get; set; }

                [Indexed]
                public string QuestionId { get; set; }

                public decimal Value { get; set; }

                [Indexed]
                public string Title { get; set; }

                [Indexed]
                public string SearchTitle { get; set; }

                public decimal? ParentValue { get; set; }

                public int SortOrder { get; set; }

                public string TranslationId { get; set; }
            }
        }

        public M201912091307_ChangeIdTypeInOptionViewTable(IFileSystemAccessor fileSystemAccessor, SqliteSettings settings)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.settings = settings;
        }

        public void Up()
        {
            var entityName = typeof(Old.OptionView).Name;

            var pathToDatabase = fileSystemAccessor.CombinePath(settings.PathToDatabaseDirectory, entityName + "-data.sqlite3");

            var sqliteConnectionString = new SQLiteConnectionString(pathToDatabase,
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true, null);
            var connection = new SQLiteConnectionWithLock(sqliteConnectionString);

            connection.RunInTransaction(() =>
            {
                connection.CreateTable<Old.OptionView>();
                connection.CreateTable<New.OptionViewNew>();

                connection.Table<New.OptionViewNew>().Delete(_ => true);

                var allExistedData = connection.Table<Old.OptionView>();
                foreach (var row in allExistedData)
                {
                    var newRow = new New.OptionViewNew()
                    {
                        CategoryId = null,
                        QuestionId = row.QuestionId,
                        ParentValue = row.ParentValue,
                        QuestionnaireId = row.QuestionnaireId,
                        SearchTitle = row.SearchTitle,
                        SortOrder = row.SortOrder,
                        Title = row.Title,
                        TranslationId = row.TranslationId,
                        Value = row.Value
                    };
                    connection.Insert(newRow);
                }

                connection.DropTable<Old.OptionView>();

                connection.Execute($"ALTER TABLE {nameof(New.OptionViewNew)} RENAME TO {nameof(Old.OptionView)};");
            });
        }
    }
}
