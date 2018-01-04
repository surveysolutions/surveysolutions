﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetParentDataFile_is_called_for_top_level_of_questionnaire : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    Create.Entity.FixedRoster(rosterId: rosterGroupId,
                        obsoleteFixedTitles: new[] {"1"}, title: "Roster Group"));

            importDataParsingService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
           () =>
               result =
                   importDataParsingService.GetParentDataFile(questionnaireDocument.Title, Create.Entity.PreloadedData(CreatePreloadedDataByFile(null, null, "Roster Group"), CreatePreloadedDataByFile(null, null, questionnaireDocument.Title)));

        It should_result_be_null = () =>
           result.ShouldBeNull();

        private static ImportDataParsingService importDataParsingService;
        private static QuestionnaireDocument questionnaireDocument;
        private static PreloadedDataByFile result;
        private static Guid rosterGroupId = Guid.NewGuid();
    }
}
