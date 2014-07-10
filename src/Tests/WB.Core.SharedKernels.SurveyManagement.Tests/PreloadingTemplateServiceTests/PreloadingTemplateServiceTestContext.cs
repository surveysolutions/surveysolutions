using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadingTemplateServiceTests
{
    [Subject(typeof(PreloadingTemplateService))]
    internal class PreloadingTemplateServiceTestContext
    {
        protected static PreloadingTemplateService CreatePreloadingTemplateService(IDataFileExportService dataFileExportService = null,
            QuestionnaireExportStructure questionnaireExportStructure = null, IFileSystemAccessor fileSystemAccessor=null)
        {
            var currentFileSystemAccessor = fileSystemAccessor ?? CreateIFileSystemAccessorMock().Object;
            return new PreloadingTemplateService(currentFileSystemAccessor,
                Mock.Of<IVersionedReadSideRepositoryReader<QuestionnaireExportStructure>>(
                    _ => _.GetById(Moq.It.IsAny<string>(), Moq.It.IsAny<long>()) == questionnaireExportStructure), "",
                dataFileExportService ?? CreateIDataFileExportServiceMock().Object,
                Mock.Of<IArchiveUtils>());
        }

        protected static Mock<IFileSystemAccessor> CreateIFileSystemAccessorMock()
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            fileSystemAccessorMock.Setup(x => x.MakeValidFileName(Moq.It.IsAny<string>()))
                .Returns<string>(name => name);
            fileSystemAccessorMock.Setup(x => x.GetFileName(Moq.It.IsAny<string>()))
               .Returns<string>(Path.GetFileName);
            return fileSystemAccessorMock;
        }

        protected static Mock<IDataFileExportService> CreateIDataFileExportServiceMock()
        {
            var dataFileExportServiceMock = new Mock<IDataFileExportService>();
            dataFileExportServiceMock.Setup(x => x.GetInterviewExportedDataFileName(Moq.It.IsAny<string>()))
                .Returns<string>(levelName => levelName);
            return dataFileExportServiceMock;
        }

        protected static QuestionnaireExportStructure CreateQuestionnaireExportStructure(int levelCount = 1)
        {
            var questionnaireExportStructure = new QuestionnaireExportStructure();
            for (int i = 0; i < levelCount; i++)
            {

                questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid> { Guid.NewGuid() },
                    new HeaderStructureForLevel() { LevelName = "level" + i, LevelScopeVector = new ValueVector<Guid> { Guid.NewGuid() } });
            }
            return questionnaireExportStructure;
        }
    }
}
