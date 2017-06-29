using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetParentIdColumnIndexes_is_called_for_file_which_is_not_corresponding_to_any_roster : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter();

            importDataParsingService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
            () =>
                result =
                    importDataParsingService.GetParentIdColumnIndexes(
                        CreatePreloadedDataByFile(new string[] { "Id" }, new string[][] { new string[] { "1" } },
                            "random file name"));

        It should_return_null_result = () =>
            result.ShouldBeNull();


        private static ImportDataParsingService importDataParsingService;
        private static QuestionnaireDocument questionnaireDocument;
        private static int[] result;
    }
}
