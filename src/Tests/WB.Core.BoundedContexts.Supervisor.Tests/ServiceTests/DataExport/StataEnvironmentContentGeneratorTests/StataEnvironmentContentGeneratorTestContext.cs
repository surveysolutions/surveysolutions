using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Commands.Sync;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    internal class StataEnvironmentContentGeneratorTestContext
    {
        protected static StataEnvironmentContentGenerator CreateStataEnvironmentContentGenerator(HeaderStructureForLevel headerStructureForLevel, string dataFileName = "data_file_name.csv")
        {
            return new StataEnvironmentContentGenerator(headerStructureForLevel ?? CreateHeaderStructureForLevel(), dataFileName);
        }

        protected static HeaderStructureForLevel CreateHeaderStructureForLevel(params ExportedHeaderItem[] exportedHeaderItems)
        {
            var result = new HeaderStructureForLevel();
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
