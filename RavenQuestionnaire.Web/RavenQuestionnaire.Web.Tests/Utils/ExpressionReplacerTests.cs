using System;
using System.Collections.Generic;
using Main.Core.View;
using Moq;
using NUnit.Framework;
using Ninject;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Utils;

namespace RavenQuestionnaire.Web.Tests.Utils
{
    [TestFixture]
    public class ExpressionReplacerTests
    {
        #region Replacing stata captions

        [Test]
        public void ReplaceStataCaptionsWithGuids_When_expression_is_empty_Then_repository_should_not_be_called()
        {
            // Arrange
            var viewRepositoryMock = Mock.Get(Mock.Of<IViewRepository>());
            var replacer = CreateExpressionReplacer(viewRepository: viewRepositoryMock.Object);
            var emptyExpression = "";

            // Act
            replacer.ReplaceStataCaptionsWithGuids(emptyExpression, Guid.NewGuid());

            // Assert
            viewRepositoryMock.Verify(x => x.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()), Times.Never());
        }

        [Test]
        public void ReplaceStataCaptionsWithGuids_When_expression_has_no_known_captions_Then_result_should_match_with_given_expression()
        {
            // Arrange
            var repositoryWhichKnowsNoCaptions = CreateRepositoryStubWhichReturnsMapWithNoCaptions();
            var replacer = CreateExpressionReplacer(viewRepository: repositoryWhichKnowsNoCaptions);
            var noStataCaptionsExpression = "[a0d6ff6f-230e-4a1f-b940-97f93e037e08] == [unknown_stata_caption]";

            // Act
            var result = replacer.ReplaceStataCaptionsWithGuids(noStataCaptionsExpression, Guid.NewGuid());

            // Assert
            Assert.That(result, Is.EqualTo(noStataCaptionsExpression));
        }

        [Test]
        [TestCase("[caption2]", "[9e7bf746-ba13-4b53-aa1c-c0e5d9b2a1e0]")]
        [TestCase("[caption1] == 8 or [caption1] > [caption2]", "[a0d6ff6f-230e-4a1f-b940-97f93e037e08] == 8 or [a0d6ff6f-230e-4a1f-b940-97f93e037e08] > [9e7bf746-ba13-4b53-aa1c-c0e5d9b2a1e0]")]
        [TestCase("[caption1] == [unknown_caption]", "[a0d6ff6f-230e-4a1f-b940-97f93e037e08] == [unknown_caption]")]
        public void ReplaceStataCaptionsWithGuids_When_expression_contains_stata_captions_Then_all_known_should_be_replaced(
            string expressionWithStataCaptions, string expectedExpressionWithGuids)
        {
            // Arrange
            var replacer = CreateExpressionReplacer(viewRepository: CreateRepositoryStubWhichReturnsMapWithSeedData());

            // Act
            var result = replacer.ReplaceStataCaptionsWithGuids(expressionWithStataCaptions, Guid.NewGuid());

            // Assert
            Assert.That(result, Is.EqualTo(expectedExpressionWithGuids));
        }
        
        #endregion

        #region Replacing guids

        [Test]
        public void ReplaceGuidsWithStataCaptions_When_expression_contains_guids_Then_all_known_should_be_replaced()
        {
            // Arrange
            var replacer = CreateExpressionReplacer(viewRepository: CreateRepositoryStubWhichReturnsMapWithSeedData());
            var expressionWithGuids = "[a0d6ff6f-230e-4a1f-b940-97f93e037e08] == 8 or [a0d6ff6f-230e-4a1f-b940-97f93e037e08] > [9e7bf746-ba13-4b53-aa1c-c0e5d9b2a1e0]";

            // Act
            var result = replacer.ReplaceGuidsWithStataCaptions(expressionWithGuids, Guid.NewGuid());

            // Assert
            var expectedExpressionWithStataCaptions = "[caption1] == 8 or [caption1] > [caption2]";
            Assert.That(result, Is.EqualTo(expectedExpressionWithStataCaptions));
        }


        [Test]
        public void ReplaceGuidsWithStataCaptions_When_expression_is_empty_Then_repository_should_not_be_called()
        {
            // Arrange
            var viewRepositoryMock = Mock.Get(Mock.Of<IViewRepository>());
            var replacer = CreateExpressionReplacer(viewRepository: viewRepositoryMock.Object);
            var emptyExpression = "";

            // Act
            replacer.ReplaceGuidsWithStataCaptions(emptyExpression, Guid.NewGuid());

            // Assert
            viewRepositoryMock.Verify(x => x.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()), Times.Never());
        }

        [Test]
        public void ReplaceGuidsWithStataCaptions_When_expression_contains_no_guids_Then_result_should_match_with_given_expression()
        {
            // Arrange
            var replacer = CreateExpressionReplacer(viewRepository: CreateRepositoryStubWhichReturnsMapWithNoCaptions());
            var guidLessExpression = "[age] == 8 or [weight] > [tall]";

            // Act
            var result = replacer.ReplaceGuidsWithStataCaptions(guidLessExpression, Guid.NewGuid());

            // Assert
            Assert.That(result, Is.EqualTo(guidLessExpression));
        }

        #endregion

        private ExpressionReplacer CreateExpressionReplacer(IViewRepository viewRepository)
        {
            return new ExpressionReplacer(viewRepository);
        }

        private IViewRepository CreateRepositoryStubWhichReturnsMapWithNoCaptions()
        {
            var emptyStataMapView = new QuestionnaireStataMapView { StataMap = new List<KeyValuePair<Guid, string>>() };

            return Mock.Of<IViewRepository>(repository
                => repository.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()) == emptyStataMapView);
        }

        private IViewRepository CreateRepositoryStubWhichReturnsMapWithSeedData()
        {
            var stataMapWithSeedData = new QuestionnaireStataMapView
            {
                StataMap = new List<KeyValuePair<Guid, string>>()
                {
                    {new KeyValuePair<Guid, string>(Guid.Parse("a0d6ff6f-230e-4a1f-b940-97f93e037e08"), "caption1")}, 
                    {new KeyValuePair<Guid, string>(Guid.Parse("9e7bf746-ba13-4b53-aa1c-c0e5d9b2a1e0"), "caption2")}, 
                }
            };

            return Mock.Of<IViewRepository>(repository
                => repository.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()) == stataMapWithSeedData);
        }
    }
}
