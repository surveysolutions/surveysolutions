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

        public override AssignmentDocument GetById(int id)
        {
            return RunInTransaction(table =>
            {
                var entity = table.Connection.Find<AssignmentDocument>(id);

                if (entity == null) return null;

                entity.Answers = table.Connection.Table<AssignmentDocument.AssignmentAnswer>()
                    .Where(ia => ia.AssignmentId == id)
                    .ToList();

                return entity;
            });
        }

        public override void Store(IEnumerable<AssignmentDocument> entities)
        {
            try
            {
                RunInTransaction(table =>
                {
                    foreach (var entity in entities.Where(entity => entity != null))
                    {
                        table.Connection.InsertOrReplace(entity);

                        table.Connection.Table<AssignmentDocument.AssignmentAnswer>()
                            .Delete(answer => answer.AssignmentId == entity.Id);

                        table.Connection.InsertAll(entity.Answers);
                    }
                });

            }
            catch (SQLiteException ex)
            {
                this.logger.Fatal($"Failed to persist {entities.Count()} entities as batch", ex);
                throw;
            }
        }

        public override IReadOnlyCollection<AssignmentDocument> LoadAll()
        {
            return RunInTransaction(documents =>
            {
                var answersLookup = documents.Connection.Table<AssignmentDocument.AssignmentAnswer>().ToLookup(a => a.AssignmentId);

                IEnumerable<AssignmentDocument> FillAnswers()
                {
                    foreach (var assignment in documents)
                    {
                        assignment.Answers = answersLookup[assignment.Id].ToList();
                        yield return assignment;
                    }
                }

                return FillAnswers().ToReadOnlyCollection();
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