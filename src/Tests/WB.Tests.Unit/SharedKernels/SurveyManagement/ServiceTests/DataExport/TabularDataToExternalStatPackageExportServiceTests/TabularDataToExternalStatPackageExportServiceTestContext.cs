using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.TabularDataToExternalStatPackageExportServiceTests
{
    [Subject(typeof (TabularDataToExternalStatPackageExportService))]
    internal class TabularDataToExternalStatPackageExportServiceTestContext
    {
        protected static TabularDataToExternalStatPackageExportService CreateSqlToTabDataExportService(
            QuestionnaireExportStructure questionnaireExportStructure = null,
            IFileSystemAccessor fileSystemAccessor = null,
            ITabFileReader tabFileReader = null,
            IDataQueryFactory dataQueryFactory = null,
            IDatasetWriterFactory datasetWriterFactory = null)
        {
            fileSystemAccessor = fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>();
            return new TabularDataToExternalStatPackageExportService(
                fileSystemAccessor,
                Mock.Of<ITransactionManagerProvider>(_ => _.GetTransactionManager() == Mock.Of<ITransactionManager>()),
                Mock.Of<ILogger>(),
                tabFileReader ?? Mock.Of<ITabFileReader>(),
                dataQueryFactory ?? Mock.Of< IDataQueryFactory> (),
                datasetWriterFactory ?? Mock.Of<IDatasetWriterFactory>(), new QuestionnaireLabelFactory(),
                Mock.Of<IQuestionnaireExportStructureStorage>(
                    _ =>
                        _.GetQuestionnaireExportStructure(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaireExportStructure));
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
                    new Dictionary<Guid, IExportedHeaderItem>
                    {
                        { Guid.NewGuid(), CreateExportedHeaderItem() },
                        { Guid.NewGuid(), CreateExportedHeaderItem(QuestionType.Numeric, new[] { "a" }) }
                    }
            };
        }

        protected static ExportedQuestionHeaderItem CreateExportedHeaderItem(QuestionType type = QuestionType.Text, string[] columnNames = null)
        {
            return new ExportedQuestionHeaderItem() { ColumnNames = columnNames ?? new[] { "1" }, Titles = columnNames ?? new[] { "1" }, QuestionType = type };
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
