using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Tests.Unit.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    [TestFixture]
    internal class DesignerEngineVersionServiceNUnitTests
    {
        private DesignerEngineVersionService CreateDesignerEngineVersionService()
        {
            return new DesignerEngineVersionService();
        }

        [Test]
        public void
            IsQuestionnaireDocumentSupportedByClientVersion_When_client_version_is_greater_then_10_Then_should_return_true
            ()
        {

            var designerEngineVersionService =
                this.CreateDesignerEngineVersionService();

            var result =
                designerEngineVersionService.IsQuestionnaireDocumentSupportedByClientVersion(
                    Create.QuestionnaireDocument(), new Version(10, 1, 1));

            Assert.That(result, Is.True);
        }

        [Test]
        public void
            IsQuestionnaireDocumentSupportedByClientVersion_When_client_version_is_less_then_10_and_questionnaire_doesnt_have_hidden_questions_Then_should_return_true
            ()
        {

            var designerEngineVersionService =
                this.CreateDesignerEngineVersionService();

            var expressionsEngineVersionService = Setup.DesignerEngineVersionService();

            var result =
                designerEngineVersionService.IsQuestionnaireDocumentSupportedByClientVersion(
                    Create.QuestionnaireDocument(), new Version(9, 0, 0));

            Assert.That(result, Is.True);
        }

        [Test]
        public void
            IsQuestionnaireDocumentSupportedByClientVersion_When_client_version_is_less_then_10_and_questionnaire_has_hidden_question_Then_should_return_false
            ()
        {

            var designerEngineVersionService =
                this.CreateDesignerEngineVersionService();

            var result =
                designerEngineVersionService.IsQuestionnaireDocumentSupportedByClientVersion(
                    Create.QuestionnaireDocument(children:
                        new IComposite[] {Create.TextQuestion(scope: QuestionScope.Hidden)}), new Version(9, 0, 0));

            Assert.That(result, Is.False);
        }
    }
}