using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB;
using WB.Core;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public abstract class AudioAuditStorageBase : IAudioAuditFileStorage
    {
        private readonly IUnitOfWork unitOfWork;

        public AudioAuditStorageBase(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public abstract Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId);
        public abstract byte[] GetInterviewBinaryData(Guid interviewId, string fileName);
        public abstract Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName);
        public abstract Task RemoveInterviewBinaryData(Guid interviewId, string fileName);
        public abstract void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType);

        public Task<bool> HasAnyAudioAuditFilesStoredAsync(QuestionnaireIdentity questionnaire)
        {
            return this.unitOfWork.Session
                .CreateSQLQuery(@"select exists (
	                select 1
	                from plainstore.audioauditfiles a
	                join readside.interviewsummaries s on s.interviewid = a.interviewid
	                where s.questionnaireidentity = :questionnaireId
                )")
                .SetParameter("questionnaireId", questionnaire.Id)
                .UniqueResultAsync<bool>();
        }
    }
}
