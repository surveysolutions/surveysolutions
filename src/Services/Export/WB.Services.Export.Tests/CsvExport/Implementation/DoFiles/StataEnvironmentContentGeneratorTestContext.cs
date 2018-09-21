using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Implementation.DoFiles
{
    [NUnit.Framework.TestOf(typeof(StataEnvironmentContentService))]
    internal class StataEnvironmentContentGeneratorTestContext
    {
        protected static StataEnvironmentContentService CreateStataEnvironmentContentGenerator(
            IFileSystemAccessor fileSystemAccessor)
        {
            return new StataEnvironmentContentService(fileSystemAccessor, 
                new QuestionnaireLabelFactory(),
                Create.InterviewActionsExporter(Create.HeadquartersApi()),
                Create.CommentsExporter(),
                Create.InterviewErrorsExporter(),
                Create.DiagnosticsExporter());
        }

        protected static IFileSystemAccessor CreateFileSystemAccessor(Action<string> returnContentAction)
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.MakeValidFileName(Moq.It.IsAny<string>()))
                .Returns<string>(s => s);
            fileSystemAccessorMock.Setup(x => x.WriteAllText(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Callback<string, string>((path, content) => returnContentAction(content));

            return fileSystemAccessorMock.Object;
        }

        protected static HeaderStructureForLevel CreateHeaderStructureForLevel(string levelName = null,
            params ExportedQuestionHeaderItem[] exportedQuestionHeaderItems)
        {
            var result = new HeaderStructureForLevel();
            result.LevelScopeVector = new ValueVector<Guid>();
            result.LevelName = levelName;
            result.LevelIdColumnName = "Id";
            foreach (var exportedHeaderItem in exportedQuestionHeaderItems)
            {
                result.HeaderItems.Add(exportedHeaderItem.PublicKey, exportedHeaderItem);
            }

            return result;
        }

        protected static ExportedQuestionHeaderItem CreateExportedHeaderItem(string variableName = "item",
            string title = "some item",
            params LabelItem[] labels)
        {
            return new ExportedQuestionHeaderItem()
            {
                PublicKey = Guid.NewGuid(),
                VariableName = variableName,
                QuestionType = QuestionType.Numeric,
                ColumnHeaders = GetHeaderColumns(variableName, title),
                Labels = (labels ?? new LabelItem[0]).ToList()
            };
        }

        protected static LabelItem CreateLabelItem(string caption = "caption", string title = "title")
        {
            return new LabelItem {Caption = caption, Title = title};
        }

        protected static List<HeaderColumn> GetHeaderColumns(string variableName, string title)
        {
            return new List<HeaderColumn> {new HeaderColumn {Name = variableName, Title = title}};
        }
    }
}
