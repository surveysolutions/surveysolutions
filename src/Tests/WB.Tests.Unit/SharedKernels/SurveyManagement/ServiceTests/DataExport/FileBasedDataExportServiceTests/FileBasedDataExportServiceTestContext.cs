using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    [Subject(typeof(FileBasedDataExportRepositoryWriter))]
    internal class FileBasedDataExportServiceTestContext
    {
        protected static FileBasedDataExportRepositoryWriter CreateFileBasedDataExportService(
            IFileSystemAccessor fileSystemAccessor = null, IDataExportWriter dataExportWriter = null,
            IEnvironmentContentService environmentContentService = null, IPlainInterviewFileStorage plainFileRepository = null,
            InterviewDataExportView interviewDataExportView = null, IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryWriter = null, UserDocument user = null, InterviewData interviewData=null)
        {
            var currentFileSystemAccessor = fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>();
            return new FileBasedDataExportRepositoryWriter(
                dataExportWriter ?? Mock.Of<IDataExportWriter>(),
                environmentContentService ?? Mock.Of<IEnvironmentContentService>(), currentFileSystemAccessor,
                Mock.Of<ILogger>(),
                plainFileRepository ??
                    Mock.Of<IPlainInterviewFileStorage>(
                        _ => _.GetBinaryFilesForInterview(Moq.It.IsAny<Guid>()) == new List<InterviewBinaryDataDescriptor>()),
                Mock.Of<IReadSideKeyValueStorage<InterviewData>>(
                    _ => _.GetById(It.IsAny<string>()) == (interviewData ?? new InterviewData())),
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireExportStructure>>(
                    _ => _.GetById(It.IsAny<string>()) == new QuestionnaireExportStructure()),
                Mock.Of<IReadSideRepositoryWriter<UserDocument>>(_ => _.GetById(It.IsAny<string>()) == user),
                interviewSummaryWriter ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                Mock.Of<IExportViewFactory>(
                    _ =>
                        _.CreateInterviewDataExportView(It.IsAny<QuestionnaireExportStructure>(), It.IsAny<InterviewData>()) ==
                            (interviewDataExportView ??
                                new InterviewDataExportView(Guid.NewGuid(), Guid.NewGuid(), 1, new InterviewDataExportLevelView[0]))),
                filebasedExportedDataAccessor ?? Mock.Of<IFilebasedExportedDataAccessor>());
        }

        protected static void AddLevelToExportStructure(QuestionnaireExportStructure questionnaireExportStructure, Guid levelId,
            string levelName)
        {
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid> { levelId },
               new HeaderStructureForLevel() { LevelScopeVector = new ValueVector<Guid> { levelId }, LevelName = levelName });
        }

        protected static QuestionnaireExportStructure CreateQuestionnaireExportStructure()
        {
            return new QuestionnaireExportStructure();
        }
    }
}
