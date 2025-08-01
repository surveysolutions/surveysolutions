﻿using System;
using System.IO;
using Microsoft.Extensions.Options;
//using JsonDiffPatchDotNet;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Templates;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Configs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadingTemplateServiceTests
{
    [NUnit.Framework.TestOf(typeof(AssignmentImportTemplateGenerator))]
    internal class PreloadingTemplateServiceTestContext
    {
        protected static AssignmentImportTemplateGenerator CreatePreloadingTemplateService(
            IFileSystemAccessor fileSystemAccessor = null, 
            ITabularFormatExportService tabularFormatExportService = null,
            IExportFileNameService exportFileNameService = null)
        {
            var currentFileSystemAccessor = fileSystemAccessor ?? CreateIFileSystemAccessorMock().Object;
            return new AssignmentImportTemplateGenerator(currentFileSystemAccessor, Options.Create(new FileStorageConfig()),//",
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
            return new AssignmentImportTemplateGenerator(currentFileSystemAccessor, Options.Create(new FileStorageConfig()),
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
