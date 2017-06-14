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
    public class AssignmentDocumentsStorage : SqlitePlainStorage<AssignmentDocument, int>
    {
        public AssignmentDocumentsStorage(ILogger logger, IFileSystemAccessor fileSystemAccessor, SqliteSettings settings) : base(logger, fileSystemAccessor, settings)
        {
            this.connection.CreateTable<AssignmentDocument.IdentifyingAnswer>();
        }

        public AssignmentDocumentsStorage(SQLiteConnectionWithLock storage, ILogger logger) : base(storage, logger)
        {
            storage.CreateTable<AssignmentDocument.IdentifyingAnswer>();
        }

        public override AssignmentDocument GetById(int id)
        {
            var entity = base.GetById(id);

            entity.IdentifyingData = this.connection.Table<AssignmentDocument.IdentifyingAnswer>()
                .Where(ia => ia.AssignmentId == id).ToList();

            return entity;
        }

        public override void Store(IEnumerable<AssignmentDocument> entities)
        {
            try
            {
                using (this.connection.Lock())
                {
                    this.connection.RunInTransaction(() =>
                    {
                        foreach (var entity in entities.Where(entity => entity != null))
                        {
                            this.connection.InsertOrReplace(entity);

                            this.connection.Table<AssignmentDocument.IdentifyingAnswer>()
                                .Delete(answer => answer.AssignmentId == entity.Id);

                            this.connection.InsertAll(entity.IdentifyingData);
                        }
                    });
                }

            }
            catch (SQLiteException ex)
            {
                this.logger.Fatal($"Failed to persist {entities.Count()} entities as batch", ex);
                throw;
            }
        }

        public override IReadOnlyCollection<AssignmentDocument> LoadAll()
        {
            var entities = this.connection.Table<AssignmentDocument>().ToList();
            var answers  = this.connection.Table<AssignmentDocument.IdentifyingAnswer>().ToList();

            var result = entities.GroupJoin(answers, entity => entity.Id, answer => answer.AssignmentId,
                (entity, entityAnswers) =>
                {
                    entity.IdentifyingData = entityAnswers.ToList();
                    return entity;
                });

            return result.ToReadOnlyCollection();
        }

        public override void RemoveAll()
        {
            base.RemoveAll();
            this.connection.DeleteAll<AssignmentDocument.IdentifyingAnswer>();
        }
    }
}