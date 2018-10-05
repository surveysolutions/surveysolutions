using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class AssignmentDocumentsStorage :
        SqlitePlainStorage<AssignmentDocument, int>,
        IAssignmentDocumentsStorage
    {
        private readonly IEncryptionService encryptionService;

        public AssignmentDocumentsStorage(ILogger logger, IFileSystemAccessor fileSystemAccessor,
            SqliteSettings settings, IEncryptionService encryptionService)
            : base(logger, fileSystemAccessor, settings)
        {
            this.encryptionService = encryptionService;
            this.connection.CreateTable<AssignmentDocument.AssignmentAnswer>();
            this.connection.CreateTable<AssignmentDocument.AssignmentProtectedVariable>();
        }

        public AssignmentDocumentsStorage(SQLiteConnectionWithLock storage, ILogger logger, IEncryptionService encryptionService)
            : base(storage, logger)
        {
            this.encryptionService = encryptionService;
            storage.CreateTable<AssignmentDocument.AssignmentAnswer>();
            this.connection.CreateTable<AssignmentDocument.AssignmentProtectedVariable>();
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
            return RunInTransaction(query =>
            {
                var assignment = query.Connection.Find<AssignmentDocument>(id);

                if (assignment == null) return null;

                assignment.Answers = query.Connection.Table<AssignmentDocument.AssignmentAnswer>()
                    .Where(aa => aa.AssignmentId == id)
                    .Select(DecryptedAnswer)
                    .ToList();

                return assignment;
            });
        }

        public void DecreaseInterviewsCount(int assignmentId)
        {
            AssignmentDocument assignmentDocument = this.GetById(assignmentId);
            if (assignmentDocument == null) return;
            assignmentDocument.CreatedInterviewsCount = assignmentDocument.CreatedInterviewsCount - 1;
            this.Store(assignmentDocument);
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

            if (entity.Answers != null)
            {
                table.Connection.Table<AssignmentDocument.AssignmentAnswer>().Delete(answer => answer.AssignmentId == entity.Id);
                table.Connection.InsertAll(entity.Answers.Select(EncryptedAnswer));
            }

            if (entity.ProtectedVariables?.Count > 0)
            {
                table.Connection.Table<AssignmentDocument.AssignmentProtectedVariable>()
                                .Delete(variable => variable.AssignmentId == entity.Id);
                table.Connection.InsertAll(entity.ProtectedVariables);
            }
        }

        /// <summary>
        /// Load all assignment documents, without all preloaded data
        /// </summary>
        /// <returns></returns>
        public override IReadOnlyCollection<AssignmentDocument> LoadAll()
        {
            return LoadAll(null);
        }

        public IReadOnlyCollection<AssignmentDocument> LoadAll(Guid? responsibleId)
        {
            return RunInTransaction(assignmentTable =>
            {
                var assignments = assignmentTable;

                assignments = responsibleId == null
                    ? assignments
                    : assignments.Where(ass => ass.ResponsibleId == responsibleId);

                var ids = assignments.ToList().Select(a => a.Id).ToArray();

                var answersLookup = assignmentTable.Connection.Table<AssignmentDocument.AssignmentAnswer>()
                    .Where(a => a.IsIdentifying && ids.Contains(a.AssignmentId))
                    .Select(DecryptedAnswer)
                    .ToLookup(a => a.AssignmentId);

                IEnumerable<AssignmentDocument> FillAnswers()
                {
                    foreach (var assignment in assignments)
                    {
                        assignment.IdentifyingAnswers = answersLookup[assignment.Id]
                            .Where(answer => answer.Identity.Id != assignment.LocationQuestionId)
                            .ToList();
                        
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
                    .Where(a => a.AssignmentId == document.Id)
                    .Select(DecryptedAnswer)
                    .ToList();

                document.Answers = answers;

                var protectedVariables = documents.Connection.Table<AssignmentDocument.AssignmentProtectedVariable>()
                    .Where(a => a.AssignmentId == document.Id).ToList();
                document.ProtectedVariables = protectedVariables;

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

        private AssignmentDocument.AssignmentAnswer EncryptedAnswer(AssignmentDocument.AssignmentAnswer answer)
        {
            answer.EncryptedAnswerAsString = this.encryptionService.Encrypt(answer.AnswerAsString);
            answer.EncryptedQuestion = this.encryptionService.Encrypt(answer.Question);
            answer.EncryptedSerializedAnswer = this.encryptionService.Encrypt(answer.SerializedAnswer);

            answer.AnswerAsString = null;
            answer.Question = null;
            answer.SerializedAnswer = null;

            return answer;
        }

        private AssignmentDocument.AssignmentAnswer DecryptedAnswer(AssignmentDocument.AssignmentAnswer answer)
        {
            if (answer.EncryptedAnswerAsString != null)
            {
                answer.AnswerAsString = this.encryptionService.Decrypt(answer.EncryptedAnswerAsString);
                answer.EncryptedAnswerAsString = null;
            }

            if (answer.EncryptedQuestion != null)
            {
                answer.Question = this.encryptionService.Decrypt(answer.EncryptedQuestion);
                answer.EncryptedQuestion = null;
            }

            if (answer.EncryptedSerializedAnswer != null)
            {
                answer.SerializedAnswer = this.encryptionService.Decrypt(answer.EncryptedSerializedAnswer);
                answer.EncryptedSerializedAnswer = null;
            }

            return answer;
        }
    }
}
