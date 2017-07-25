using System;
using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Templates;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadingTemplateServiceTests
{
    [Subject(typeof(AssignmentImportTemplateGenerator))]
    internal class PreloadingTemplateServiceTestContext
    {
        protected static AssignmentImportTemplateGenerator CreatePreloadingTemplateService(
            IFileSystemAccessor fileSystemAccessor = null, 
            ITabularFormatExportService tabularFormatExportService = null,
            IExportFileNameService exportFileNameService = null)
        {
            var currentFileSystemAccessor = fileSystemAccessor ?? CreateIFileSystemAccessorMock().Object;
            return new AssignmentImportTemplateGenerator(currentFileSystemAccessor, "",
                tabularFormatExportService ?? Mock.Of<ITabularFormatExportService>(),
                Mock.Of<IArchiveUtils>(),
                exportFileNameService ?? Mock.Of<IExportFileNameService>(),
                Mock.Of<ISampleUploadViewFactory>());
        }

        protected static IPreloadingTemplateService CreatePreloadingTemplateServiceForGeneratePrefilledTemplate(
            ISampleUploadViewFactory sampleUploadViewFactory,
            IFileSystemAccessor fileSystemAccessor = null)
        {
            var currentFileSystemAccessor = fileSystemAccessor ?? CreateIFileSystemAccessorMock().Object;
            return new AssignmentImportTemplateGenerator(currentFileSystemAccessor, "",
                Mock.Of<ITabularFormatExportService>(),
                Mock.Of<IArchiveUtils>(),
                Mock.Of<IExportFileNameService>(),
                sampleUploadViewFactory);
        }

        protected static Mock<IFileSystemAccessor> CreateIFileSystemAccessorMock()
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            fileSystemAccessorMock.Setup(x => x.MakeStataCompatibleFileName(Moq.It.IsAny<string>()))
                .Returns<string>(name => name);
            fileSystemAccessorMock.Setup(x => x.GetFileName(Moq.It.IsAny<string>()))
               .Returns<string>(Path.GetFileName);
            return fileSystemAccessorMock;
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
