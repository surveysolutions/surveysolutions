using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetAvalibleIdListForParent_is_called_for_file_corresponding_to_nested_fixed_roster : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(

                    Create.Entity.FixedRoster(rosterId: rosterGroupId, title: "rosterGroup",  variable: "rosterGroup",
                        fixedTitles: new []{ Create.Entity.FixedTitle(1), Create.Entity.FixedTitle(2) },
                        children: new IComposite[]
                        {
                            Create.Entity.FixedRoster(rosterId: nestedRosterId, variable: "nestedRoster", fixedTitles: new []{ Create.Entity.FixedTitle(3), Create.Entity.FixedTitle(4) }, title: "nestedRoster")
                        }));

            importDataParsingService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of = () => 
            result = importDataParsingService.GetAvailableIdListForParent(
                CreatePreloadedDataByFile(
                    header: new string[] { "Id", "rosterGroup__id", ServiceColumns.InterviewId  },
                    content: new string[][]
                    {
                        new string[] { "1", "1", "1"}
                    }, 
                    fileName: "nestedRoster"), 
                new ValueVector<Guid> { rosterGroupId, nestedRosterId }, new[] { "1", "1", "1" }, new PreloadedDataByFile[0]);

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_result_have_2_ids_3_and_4 = () =>
            result.SequenceEqual(new [] { 3, 4 });

        private static ImportDataParsingService importDataParsingService;
        private static QuestionnaireDocument questionnaireDocument;
        private static int[] result;
        private static readonly Guid rosterGroupId = Id.g1;
        private static readonly Guid nestedRosterId = Id.g2;
    }
}
