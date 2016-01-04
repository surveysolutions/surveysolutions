using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.MetadataExportServiceTests
{
    [Subject(typeof (MetadataExportService))]
    internal class MetadataExportServiceTestContext
    {
        protected static MetadataExportService CreateMetadataExportService(
            QuestionnaireExportStructure questionnaireExportStructure = null,
            IFileSystemAccessor fileSystemAccessor = null,
            ITabFileReader tabFileReader = null,
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage = null,
            IMetaDescriptionFactory metaDescriptionFactory = null)
        {
            fileSystemAccessor = fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>();

            return new MetadataExportService(
                fileSystemAccessor,
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireExportStructure>>(_ => _.GetById(
                    It.IsAny<string>()) == questionnaireExportStructure),
                Mock.Of<ITransactionManagerProvider>(_ => _.GetTransactionManager() == Mock.Of<ITransactionManager>()),
                Mock.Of<ILogger>(),
                questionnaireDocumentVersionedStorage ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(),
                metaDescriptionFactory ?? Mock.Of< IMetaDescriptionFactory> (), new QuestionnaireLabelFactory());
        }

        protected static HeaderStructureForLevel CreateHeaderStructureForLevel(string levelName = "table name", string[] referenceNames = null, ValueVector<Guid> levelScopeVector = null)
        {
            return new HeaderStructureForLevel()
            {
                LevelScopeVector = levelScopeVector ?? new ValueVector<Guid>(),
                LevelName = levelName,
                LevelIdColumnName = "Id",
                IsTextListScope = referenceNames != null,
                ReferencedNames = referenceNames,
                HeaderItems =
                    new Dictionary<Guid, ExportedHeaderItem>
                    {
                        { Guid.NewGuid(), CreateExportedHeaderItem() },
                        { Guid.NewGuid(), CreateExportedHeaderItem(QuestionType.Numeric, new[] { "a" }) }
                    }
            };
        }

        protected static ExportedHeaderItem CreateExportedHeaderItem(QuestionType type = QuestionType.Text, string[] columnNames = null)
        {
            return new ExportedHeaderItem() { ColumnNames = columnNames ?? new[] { "1" }, Titles = columnNames ?? new[] { "1" }, QuestionType = type, VariableName = Guid.NewGuid().ToString()};
        }

        protected static QuestionnaireExportStructure CreateQuestionnaireExportStructure(params HeaderStructureForLevel[] levels)
        {
            var header = new Dictionary<ValueVector<Guid>, HeaderStructureForLevel>();
            if (levels != null && levels.Length > 0)
            {
                header = levels.ToDictionary((i) => i.LevelScopeVector, (i) => i);
            }
            return new QuestionnaireExportStructure() { HeaderToLevelMap = header };
        }
    }
}
