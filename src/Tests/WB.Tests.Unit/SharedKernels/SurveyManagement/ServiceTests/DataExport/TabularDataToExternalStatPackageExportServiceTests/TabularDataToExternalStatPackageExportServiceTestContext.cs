using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.TabularDataToExternalStatPackageExportServiceTests
{
    [NUnit.Framework.TestOf(typeof (TabularDataToExternalStatPackageExportService))]
    internal class TabularDataToExternalStatPackageExportServiceTestContext
    {
        protected static TabularDataToExternalStatPackageExportService CreateSqlToTabDataExportService(
            QuestionnaireExportStructure questionnaireExportStructure = null,
            IFileSystemAccessor fileSystemAccessor = null,
            ITabFileReader tabFileReader = null,
            IDataQueryFactory dataQueryFactory = null,
            IDatasetWriterFactory datasetWriterFactory = null,
            IExportServiceDataProvider exportServiceDataProvider = null)
        {
            fileSystemAccessor = fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>();
            return new TabularDataToExternalStatPackageExportService(
                fileSystemAccessor,
                Mock.Of<ILogger>(),
                tabFileReader ?? Mock.Of<ITabFileReader>(),
                dataQueryFactory ?? Mock.Of< IDataQueryFactory> (),
                datasetWriterFactory ?? Mock.Of<IDatasetWriterFactory>(), new QuestionnaireLabelFactory(),
                Mock.Of<IQuestionnaireExportStructureStorage>(
                    _ =>
                        _.GetQuestionnaireExportStructure(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaireExportStructure),
                exportServiceDataProvider ?? Mock.Of <IExportServiceDataProvider>());
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
            return new ExportedQuestionHeaderItem()
            {

                ColumnHeaders = columnNames == null ? new List<HeaderColumn>(){new HeaderColumn(){Name = "1", Title = "1"}} :
                    columnNames.Select(x => new HeaderColumn(){Name = x, Title = x}).ToList(),
                QuestionType = type
            };
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
