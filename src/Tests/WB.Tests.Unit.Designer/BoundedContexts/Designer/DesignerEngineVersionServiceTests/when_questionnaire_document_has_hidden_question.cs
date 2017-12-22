using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    internal class when_questionnaire_document_has_hidden_question
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(questionnaireId,
                Create.Group(groupId: groupId, children: new[]
                {
                    Create.TextQuestion(questionId: questionId, variable: "hidden", scope: QuestionScope.Hidden)
                })
                );

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
        private static readonly Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}