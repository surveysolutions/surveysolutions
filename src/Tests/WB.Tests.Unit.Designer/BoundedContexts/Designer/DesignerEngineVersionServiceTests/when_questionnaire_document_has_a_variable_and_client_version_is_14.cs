using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    internal class when_questionnaire_document_has_a_variable_and_client_version_is_14
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(questionnaireId,Create.Variable(id: variableId));

            designerEngineVersionService = Create.DesignerEngineVersionService();
        };

        Because of = () =>
            result = designerEngineVersionService.GetListOfNewFeaturesForClient(questionnaire, 14);

        It should_return_false = () =>
            result.ShouldNotBeEmpty();

        private static IEnumerable<string> result;
        private static IDesignerEngineVersionService designerEngineVersionService;
        private static QuestionnaireDocument questionnaire;
        private static Guid variableId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}