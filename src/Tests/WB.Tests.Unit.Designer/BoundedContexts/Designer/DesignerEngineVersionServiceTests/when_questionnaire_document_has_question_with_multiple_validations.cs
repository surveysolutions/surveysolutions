using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    internal class when_questionnaire_document_has_question_with_multiple_validations
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
            BecauseOf();
        }

        private void BecauseOf() =>
            version = designerEngineVersionService.GetQuestionnaireContentVersion(questionnaire);

        [NUnit.Framework.Test] public void should_return_16_version () =>
            version.ShouldEqual(16);

        private static int version;
        private static IDesignerEngineVersionService designerEngineVersionService;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionId = Id.g1;
        private static readonly Guid rosterId = Id.g2;
        private static readonly Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}