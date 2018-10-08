using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Tests.Services.TabularDataToExternalStatPackageExportServiceTests
{
    [NUnit.Framework.TestOf(typeof (TabularDataToExternalStatPackageExportService))]
    internal class TabularDataToExternalStatPackageExportServiceTestContext
    {
        protected static TabularDataToExternalStatPackageExportService CreateSqlToTabDataExportService(
            QuestionnaireExportStructure questionnaireExportStructure = null,
            ITabFileReader tabFileReader = null,
            IDataQueryFactory dataQueryFactory = null,
            IDatasetWriterFactory datasetWriterFactory = null,
            IExportServiceDataProvider exportServiceDataProvider = null)
        {
            return new TabularDataToExternalStatPackageExportService(
                Mock.Of<ILogger<TabularDataToExternalStatPackageExportService>>(),
                tabFileReader ?? Mock.Of<ITabFileReader>(),
                dataQueryFactory ?? Mock.Of<IDataQueryFactory> (),
                datasetWriterFactory ?? Mock.Of<IDatasetWriterFactory>(), 
                new QuestionnaireLabelFactory(),
                Mock.Of<IQuestionnaireExportStructureFactory>(x => x.GetQuestionnaireExportStructure(It.IsAny<TenantInfo>(), It.IsAny<QuestionnaireId>()) == questionnaireExportStructure),
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
