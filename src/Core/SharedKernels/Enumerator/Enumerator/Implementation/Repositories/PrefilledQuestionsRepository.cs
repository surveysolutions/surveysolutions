using System.Collections.Generic;
using System.Linq;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class PrefilledQuestionsRepository : SqlitePlainStorage<PrefilledQuestionView>
    {
        private readonly IEncryptionService encryptionService;

        public PrefilledQuestionsRepository(ILogger logger, IFileSystemAccessor fileSystemAccessor,
            SqliteSettings settings, IEncryptionService encryptionService) : base(logger, fileSystemAccessor, settings)
        {
            this.encryptionService = encryptionService;
        }

        public PrefilledQuestionsRepository(SQLiteConnectionWithLock storage, ILogger logger) : base(storage, logger)
        {
        }

        public override void Store(IEnumerable<PrefilledQuestionView> entities) => base.Store(entities.Select(ToEncryptedEntity));
        protected override PrefilledQuestionView ToModifiedEntity(PrefilledQuestionView entity)
            => ToDecryptedEntity(entity);

        private PrefilledQuestionView ToEncryptedEntity(PrefilledQuestionView entity)
        {
            entity.EncryptedAnswer = this.encryptionService.Encrypt(entity.Answer);
            entity.EncryptedQuestionText = this.encryptionService.Encrypt(entity.QuestionText);

            entity.Answer = null;
            entity.QuestionText = null;

            return entity;
        }

        private PrefilledQuestionView ToDecryptedEntity(PrefilledQuestionView entity)
        {
            if (entity.EncryptedAnswer != null)
            {
                entity.Answer = this.encryptionService.Decrypt(entity.EncryptedAnswer);
                entity.EncryptedAnswer = null;
            }

            if (entity.EncryptedQuestionText != null)
            {
                entity.QuestionText = this.encryptionService.Decrypt(entity.EncryptedQuestionText);
                entity.EncryptedQuestionText = null;
            }

            return entity;
        }
    }
}
