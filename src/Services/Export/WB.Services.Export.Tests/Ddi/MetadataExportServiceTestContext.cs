using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Ddi;
using WB.Services.Export.Ddi.Implementation;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Tests.Ddi
{
    [NUnit.Framework.TestOf(typeof (DdiMetadataFactory))]
    internal class MetadataExportServiceTestContext
    {
        protected static DdiMetadataFactory CreateMetadataExportService(
            QuestionnaireDocument questionnaireDocument,
            IMetaDescriptionFactory metaDescriptionFactory = null,
            IQuestionnaireLabelFactory questionnaireLabelFactory=null)
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string[]>())).Returns<string[]>(Path.Combine);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            questionnaireStorage.SetupIgnoreArgs(x => x.GetQuestionnaireAsync(null, null))
                .ReturnsAsync(questionnaireDocument);

            var questionnaireExportStructureFactory = new Mock<IQuestionnaireExportStructureFactory>();
            questionnaireExportStructureFactory.Setup(x =>
                    x.CreateQuestionnaireExportStructure(It.IsAny<QuestionnaireDocument>()))
                .Returns(new QuestionnaireExportStructure());

            questionnaireExportStructureFactory.Setup(x =>
                    x.GetQuestionnaireExportStructureAsync(It.IsAny<TenantInfo>(), It.IsAny<QuestionnaireId>()))
                .Returns(Task.FromResult(new QuestionnaireExportStructure()));

            return new DdiMetadataFactory(
                fileSystemAccessor.Object,
                Mock.Of<ILogger<DdiMetadataFactory>>(),
                metaDescriptionFactory ?? Mock.Of<IMetaDescriptionFactory>(),
                questionnaireLabelFactory ?? new QuestionnaireLabelFactory(),
                questionnaireStorage.Object,
                questionnaireExportStructureFactory.Object);
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

        protected static ExportedQuestionHeaderItem CreateExportedHeaderItem(QuestionType type = QuestionType.Text,
            string[] columnNames = null, Guid? questionId = null, params LabelItem[] labels)
        {
            return new ExportedQuestionHeaderItem()
            {
                ColumnHeaders = columnNames?.Select(x => new HeaderColumn(){Name = x, Title = x}).ToList() 
                                ?? new List<HeaderColumn>() { new HeaderColumn() { Name = "1", Title = "1"} },
                QuestionType = type,
                VariableName = Guid.NewGuid().ToString(),
                PublicKey = questionId ?? Guid.NewGuid(),
                Labels = labels.ToList()
            };
        }

        protected static LabelItem CreateLabelItem(string caption="caption", string title="title")
        {
            return new LabelItem() {Caption = caption, Title = title};
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
