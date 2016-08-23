using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    internal class when_questionnaire_document_has_question_and_isemailvalid_usage
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(questionnaireId,
                Create.Group(groupId: groupId, children: new[]
                {
                    Create.TextQuestion(questionId: questionId, variable: "text", 
                    validationConditions:new List<ValidationCondition>()
                    {
                        new ValidationCondition("self.IsValidEmail()", "valid email")
                    })
                })
                );

            designerEngineVersionService = Create.DesignerEngineVersionService();
        };

        Because of = () =>
            version = designerEngineVersionService.GetQuestionnaireContentVersion(questionnaire);

        It should_return_11_version = () =>
            version.ShouldEqual(11);

        private static int version;
        private static IDesignerEngineVersionService designerEngineVersionService;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}