using System;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.OneDrive.Sdk;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class OnedriveBinaryDataExportHandler : AbstractExternalStorageDataExportHandler
    {
        public OnedriveBinaryDataExportHandler(
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

        protected override IDisposable GetClient(string accessToken)
        {
            throw new NotImplementedException();
        }

        protected override string CreateApplicationFolder()
        {
            throw new NotImplementedException();
        }

        protected override string CreateFolder(string applicatioFolder, string folderName)
        {
            throw new NotImplementedException();
        }

        protected override void UploadFile(string folder, byte[] fileContent, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
