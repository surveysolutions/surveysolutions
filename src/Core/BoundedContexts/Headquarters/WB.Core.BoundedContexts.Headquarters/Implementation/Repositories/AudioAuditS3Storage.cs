using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class AudioAuditS3FileStorage : AudioAuditFileS3StorageBase<AudioAuditFile>
    {
        private readonly IUnitOfWork unitOfWork;
        protected override string AudioAuditS3Folder => "audio_audit/";

        public AudioAuditS3FileStorage(IExternalFileStorage externalFileStorage, 
            IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor, 
            IUnitOfWork unitOfWork) 
            : base(externalFileStorage, filePlainStorageAccessor)
        {
            this.unitOfWork = unitOfWork;
        }
        
        public override Task<bool> HasAnyAudioAuditFilesStoredAsync(QuestionnaireIdentity questionnaire)
        {
            return this.unitOfWork.Session
                .CreateSQLQuery(@"select exists (
	                select 1
	                from audioauditfiles a
	                join interviewsummaries s on s.interviewid = a.interviewid
	                where s.questionnaireidentity = :questionnaireId
                )")
                .SetParameter("questionnaireId", questionnaire.Id)
                .UniqueResultAsync<bool>();
        }
    }
}
