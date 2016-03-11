using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    internal class when_questionnaire_document_has_static_text_with_attachment_and_client_version_is_11
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(questionnaireId,
                Create.Group(groupId: groupId, children: new IComposite[]
                {
                    Create.StaticText(text: "hello", attachmentName: "bananas")
                }));

            designerEngineVersionService = Create.DesignerEngineVersionService();
        };

        Because of = () =>
            result = designerEngineVersionService.IsQuestionnaireDocumentSupportedByClientVersion(questionnaire, new Version(11, 0, 0));

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static IDesignerEngineVersionService designerEngineVersionService;
        private static QuestionnaireDocument questionnaire;
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}