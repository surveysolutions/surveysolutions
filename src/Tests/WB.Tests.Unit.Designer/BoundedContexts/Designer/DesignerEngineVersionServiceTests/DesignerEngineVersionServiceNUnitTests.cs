using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    [TestFixture]
    internal class DesignerEngineVersionServiceNUnitTests
    {
        private DesignerEngineVersionService CreateDesignerEngineVersionService()
        {
            return new DesignerEngineVersionService();
        }

        [Test]
        public void GetListOfNewFeaturesForClient_When_client_version_is_greater_then_10_Then_should_be_empty()
        {

            var designerEngineVersionService =this.CreateDesignerEngineVersionService();

            var result = designerEngineVersionService.GetListOfNewFeaturesForClient(Create.QuestionnaireDocument(), 11);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GetListOfNewFeaturesForClient_When_client_version_is_less_then_10_and_questionnaire_doesnt_have_hidden_questions_Then_should_be_empty()
        {

            var designerEngineVersionService = this.CreateDesignerEngineVersionService();

            var result = designerEngineVersionService.GetListOfNewFeaturesForClient(Create.QuestionnaireDocument(), 10);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GetListOfNewFeaturesForClient_When_client_version_is_less_then_10_and_questionnaire_has_hidden_question_Then_should_be_empty()
        {
            
            var designerEngineVersionService = this.CreateDesignerEngineVersionService();

            var result =
                designerEngineVersionService.GetListOfNewFeaturesForClient(
                    Create.QuestionnaireDocument(children:
                        new IComposite[] {Create.TextQuestion(scope: QuestionScope.Hidden)}), 9);
            
            Assert.IsEmpty(result);
        }

        [Test]
        public void When_questionnaire_has_question_linked_to_question_should_return_version_18()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                                        Create.Question(questionId: questionId, questionType: QuestionType.TextList), 
                                        Create.MultyOptionsQuestion(linkedToQuestionId: questionId));

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);

            Assert.That(contentVersion, Is.EqualTo(18));
        }

        [Test]
        public void when_questionnaire_contains_audio_question_Should_return_version_21()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.Question(questionType: QuestionType.Audio));

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);

            Assert.That(contentVersion, Is.EqualTo(21));
        }
    }
}