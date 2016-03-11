using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    internal class when_questionnaire_document_has_question_with_multiple_validations
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(questionnaireId,
                Create.Group(groupId: groupId, children: new IComposite[]
                {
                    Create.TextQuestion(questionId: questionId, variable: "hidden", validationConditions: new List<ValidationCondition>
                    {
                        Create.ValidationCondition("expression 1", "message 1"),
                        Create.ValidationCondition("expression 2", "message 2")
                    })
                }));

            designerEngineVersionService = Create.DesignerEngineVersionService();
        };

        Because of = () =>
            version = designerEngineVersionService.GetQuestionnaireContentVersion(questionnaire);

        It should_return_12_version = () =>
            version.ShouldEqual(new Version(12, 0, 0));

        private static Version version;
        private static IDesignerEngineVersionService designerEngineVersionService;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionId = Id.g1;
        private static readonly Guid rosterId = Id.g2;
        private static readonly Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}