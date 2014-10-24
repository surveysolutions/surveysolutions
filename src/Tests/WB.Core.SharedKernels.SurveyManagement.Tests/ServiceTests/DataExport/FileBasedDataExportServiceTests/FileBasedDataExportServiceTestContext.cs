using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Moq.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    [Subject(typeof(FileBasedDataExportRepositoryWriter))]
    internal class FileBasedDataExportServiceTestContext
    {
        protected static FileBasedDataExportRepositoryWriter CreateFileBasedDataExportService(
            IFileSystemAccessor fileSystemAccessor = null, IDataExportWriter dataExportWriter = null,
            IEnvironmentContentService environmentContentService = null, IPlainInterviewFileStorage plainFileRepository = null,
            InterviewDataExportView interviewDataExportView = null, IFilebaseExportRouteService filebaseExportRouteService = null)
        {
            var currentFileSystemAccessor = fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>();
            return new FileBasedDataExportRepositoryWriter(Mock.Of<IReadSideRepositoryCleanerRegistry>(),
                dataExportWriter ?? Mock.Of<IDataExportWriter>(),
                environmentContentService ?? Mock.Of<IEnvironmentContentService>(), currentFileSystemAccessor,
                Mock.Of<ILogger>(),
                plainFileRepository ??
                    Mock.Of<IPlainInterviewFileStorage>(
                        _ => _.GetBinaryFilesForInterview(Moq.It.IsAny<Guid>()) == new List<InterviewBinaryDataDescriptor>()),
                Mock.Of<IReadSideRepositoryWriterRegistry>(),
                Mock.Of<IReadSideRepositoryWriter<ViewWithSequence<InterviewData>>>(
                    _ => _.GetById(It.IsAny<string>()) == new ViewWithSequence<InterviewData>(new InterviewData(), 1)),
                Mock.Of<IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure>>(
                    _ => _.GetById(It.IsAny<string>(), It.IsAny<long>()) == new QuestionnaireExportStructure()),
                Mock.Of<IReadSideRepositoryWriter<UserDocument>>(), Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                Mock.Of<IExportViewFactory>(
                    _ =>
                        _.CreateInterviewDataExportView(It.IsAny<QuestionnaireExportStructure>(), It.IsAny<InterviewData>()) ==
                            (interviewDataExportView ??
                                new InterviewDataExportView(Guid.NewGuid(), Guid.NewGuid(), 1, new InterviewDataExportLevelView[0]))),
                filebaseExportRouteService ?? CreateFilebaseExportRouteService().Object,Mock.Of<IExportedDataFormatter>());
        }

        protected static Mock<IFilebaseExportRouteService> CreateFilebaseExportRouteService()
        {
            var result = new Mock<IFilebaseExportRouteService>();
            result.Setup(x => x.GetFolderPathOfDataByQuestionnaire(It.IsAny<Guid>(), It.IsAny<long>()))
                .Returns<Guid, long>((id, v) => string.Format("ExportedData\\{0}-{1}", id, v));
            result.Setup(x => x.GetFolderPathOfFilesByQuestionnaire(It.IsAny<Guid>(), It.IsAny<long>()))
                .Returns<Guid, long>((id, v) => string.Format("ExportedFiles\\exported_files_{0}-{1}", id, v));
            result.Setup(x => x.GetFolderPathOfFilesByQuestionnaireForInterview(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<Guid>()))
                .Returns<Guid, long, Guid>((id, v, interviewId) => string.Format("ExportedFiles\\{0}-{1}-{2}", id, v, interviewId));
            result.Setup(x => x.PathToExportedData).Returns("ExportedData");
            result.Setup(x => x.PathToExportedFiles).Returns("ExportedFiles");
            return result;
        }

        protected static void AddLevelToExportStructure(QuestionnaireExportStructure questionnaireExportStructure, Guid levelId,
            string levelName)
        {
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid> { levelId },
               new HeaderStructureForLevel() { LevelScopeVector = new ValueVector<Guid> { levelId }, LevelName = levelName });
        }
    }
}
