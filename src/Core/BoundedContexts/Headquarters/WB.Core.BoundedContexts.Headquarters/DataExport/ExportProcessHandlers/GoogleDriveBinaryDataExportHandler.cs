using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class GoogleDriveBinaryDataExportHandler : AbstractExternalStorageDataExportHandler
    {
        public GoogleDriveBinaryDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IImageFileStorage imageFileRepository,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings,
            ITransactionManager transactionManager,
            IInterviewFactory interviewFactory,
            IDataExportProcessesService dataExportProcessesService,
            IQuestionnaireStorage questionnaireStorage,
            IDataExportFileAccessor dataExportFileAccessor,
            IAudioFileStorage audioFileStorage,
            IPlainTransactionManagerProvider plainTransactionManagerProvider)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor, questionnaireStorage, transactionManager,
                interviewFactory, imageFileRepository, audioFileStorage, plainTransactionManagerProvider)
        {
        }

        protected override void ConnectToExternalStorage(string accessToken)
        {
            
        }

        protected override void SendImage(Guid interviewId, byte[] fileContent, string fileName)
        {
            
        }

        protected override void SendAudio(Guid interviewId, byte[] fileContent, string fileName)
        {
            
        }
    }
}
