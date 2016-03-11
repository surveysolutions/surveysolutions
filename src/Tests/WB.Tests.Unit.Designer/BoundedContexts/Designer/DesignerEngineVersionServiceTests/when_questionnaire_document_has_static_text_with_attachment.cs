using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    internal class when_questionnaire_document_has_static_text_with_attachment
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
            version = designerEngineVersionService.GetQuestionnaireContentVersion(questionnaire);

        It should_return_13_version = () =>
            version.ShouldEqual(new Version(13, 0, 0));

        private static Version version;
        private static IDesignerEngineVersionService designerEngineVersionService;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}