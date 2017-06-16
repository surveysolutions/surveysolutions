using System.Collections.Generic;
using System.Linq;
using SQLite;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Core.BoundedContexts.Interviewer.Services.Synchronization
{
    public class AssignmentDocumentsStorage :
        SqlitePlainStorage<AssignmentDocument, int>,
        IAssignmentDocumentsStorage
    {
        public AssignmentDocumentsStorage(ILogger logger, IFileSystemAccessor fileSystemAccessor, SqliteSettings settings)
            : base(logger, fileSystemAccessor, settings)
        {
            this.connection.CreateTable<AssignmentDocument.AssignmentAnswer>();
        }

        public AssignmentDocumentsStorage(SQLiteConnectionWithLock storage, ILogger logger)
            : base(storage, logger)
        {
            storage.CreateTable<AssignmentDocument.AssignmentAnswer>();
        }

        public new void Remove(int assignmentId)
        {
            RunInTransaction(table =>
            {
                table.Connection.Delete<AssignmentDocument>(assignmentId);

                table.Connection.Table<AssignmentDocument.AssignmentAnswer>()
                    .Delete(aa => aa.AssignmentId == assignmentId);
            });
        }

        public override void Store(AssignmentDocument entity)
        {
            RunInTransaction(table => StoreImplementation(table, entity));
        }

        public override void Store(IEnumerable<AssignmentDocument> entities)
        {
            try
            {
                RunInTransaction(table =>
                {
                    foreach (var entity in entities)
                    {
                        StoreImplementation(table, entity);
                    }
                });

            }
            catch (SQLiteException ex)
            {
                this.logger.Fatal($"Failed to persist {entities.Count()} entities as batch", ex);
                throw;
            }
        }

        private void StoreImplementation(TableQuery<AssignmentDocument> table, AssignmentDocument entity)
        {
            if (entity == null) return;

            table.Connection.InsertOrReplace(entity);
            table.Connection.Table<AssignmentDocument.AssignmentAnswer>().Delete(answer => answer.AssignmentId == entity.Id);
            table.Connection.InsertAll(entity.Answers);
            //table.Connection.InsertAll(entity.IdentifyingAnswers);
        }

        /// <summary>
        /// Load all assignment documents, without all preloaded data
        /// </summary>
        /// <returns></returns>
        public override IReadOnlyCollection<AssignmentDocument> LoadAll()
        {
            return RunInTransaction(assignmentTable =>
            {
                var answersLookup = assignmentTable.Connection.Table<AssignmentDocument.AssignmentAnswer>()
                    .Where(a => a.IsIdentifying)
                    .ToLookup(a => a.AssignmentId);

                IEnumerable<AssignmentDocument> FillAnswers()
                {
                    foreach (var assignment in assignmentTable)
                    {
                        assignment.IdentifyingAnswers = answersLookup[assignment.Id].ToList();
                        yield return assignment;
                    }
                }

                return FillAnswers().ToReadOnlyCollection();
            });
        }

        public AssignmentDocument FetchPreloadedData(AssignmentDocument document)
        {
            return RunInTransaction(documents =>
            {
                var answers = documents.Connection.Table<AssignmentDocument.AssignmentAnswer>()
                    .Where(a => a.AssignmentId == document.Id).ToList();

                document.Answers = answers;

                return document;
            });
        }

        public override void RemoveAll()
        {
            RunInTransaction(table =>
            {
                base.RemoveAll();
                table.Connection.DeleteAll<AssignmentDocument.AssignmentAnswer>();
            });
        }
    }
}