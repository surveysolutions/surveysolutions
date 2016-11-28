using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetAvalibleIdListForParent_is_called_for_file_corresponding_to_nested_fixed_roster : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(

                    Create.Entity.FixedRoster(rosterId: rosterGroupId, title: "rosterGroup",  variable: "rosterGroup",
                        obsoleteFixedTitles: new[] {"1", "2"},
                        children: new IComposite[]
                        {
                            Create.Entity.FixedRoster(rosterId: nestedRosterId, variable: "nestedRoster",
                                obsoleteFixedTitles: new[] {"a"}, title: "nestedRoster")
                        }));

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
            () =>
                result =
                    preloadedDataService.GetAvailableIdListForParent(
                        CreatePreloadedDataByFile(new string[] { "Id", "ParentId1",  }, new string[][] { new string[] { "1", "1"} },
                            "rosterGroup"), new ValueVector<Guid> { rosterGroupId, nestedRosterId }, new[] { "1", "1" }, new PreloadedDataByFile[0]);

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_result_have_2_ids_1_and_2 = () =>
            result.SequenceEqual(new decimal[] { 1, 2 });

        private static PreloadedDataService preloadedDataService;
        private static QuestionnaireDocument questionnaireDocument;
        private static decimal[] result;
        private static Guid rosterGroupId = Guid.NewGuid();
        private static Guid nestedRosterId = Guid.NewGuid();
    }
}
