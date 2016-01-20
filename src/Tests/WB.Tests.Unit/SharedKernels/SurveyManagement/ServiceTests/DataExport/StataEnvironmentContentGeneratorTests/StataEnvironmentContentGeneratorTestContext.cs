using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Moq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    [Subject(typeof(StataEnvironmentContentService))]
    internal class StataEnvironmentContentGeneratorTestContext
    {
        protected static StataEnvironmentContentService CreateStataEnvironmentContentGenerator(IFileSystemAccessor fileSystemAccessor)
        {
            return new StataEnvironmentContentService(fileSystemAccessor, new QuestionnaireLabelFactory());
        }

        protected static IFileSystemAccessor CreateFileSystemAccessor(Action<string> returnContentAction)
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.WriteAllText(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Callback<string, string>((path, content) => returnContentAction(content));

            return fileSystemAccessorMock.Object;
        }

        protected static HeaderStructureForLevel CreateHeaderStructureForLevel(string levelName = null,
            params ExportedHeaderItem[] exportedHeaderItems)
        {
            var result = new HeaderStructureForLevel();
            result.LevelScopeVector = new ValueVector<Guid>();
            result.LevelName = levelName;
            result.LevelIdColumnName = "Id";
            foreach (var exportedHeaderItem in exportedHeaderItems)
            {
                result.HeaderItems.Add(exportedHeaderItem.PublicKey, exportedHeaderItem);
            }
            return result;
        }

        protected static ExportedHeaderItem CreateExportedHeaderItem(string variableName = "item", string title = "some item",
            params LabelItem[] labels)
        {
            return new ExportedHeaderItem()
            {
                PublicKey = Guid.NewGuid(),
                VariableName = variableName,
                QuestionType = QuestionType.Numeric,
                ColumnNames = new[] { variableName },
                Titles = new[] { title },
                Labels = (labels ?? new LabelItem[0]).ToDictionary((l)=>l.PublicKey,(l)=>l)
            };
        }

        protected static LabelItem CreateLabelItem(string caption="caption", string title="title")
        {
            return new LabelItem() { PublicKey = Guid.NewGuid(), Caption = caption, Title = title };
        }
    }
}
