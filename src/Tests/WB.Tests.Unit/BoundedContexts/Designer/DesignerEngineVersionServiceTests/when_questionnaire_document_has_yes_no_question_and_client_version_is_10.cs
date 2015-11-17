using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    internal class when_questionnaire_document_has_yes_no_question_and_client_version_is_10
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(questionnaireId,
                Create.Group(groupId: groupId, children: new[]
                {
                    Create.MultyOptionsQuestion(id: questionId, variable: "yesno", yesNoView: true, 
                        answers: new List<Answer>
                        {
                            Create.Option(text: "option 1", value: "1"),
                            Create.Option(text: "option 2", value: "2"),
                        })
                })
            );

            designerEngineVersionService = Create.DesignerEngineVersionService();
        };

        Because of = () =>
            result = designerEngineVersionService.IsQuestionnaireDocumentSupportedByClientVersion(questionnaire, new Version(10, 0, 0));

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static IDesignerEngineVersionService designerEngineVersionService;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}
