using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Events;
using Main.Core.Export;
using Main.Core.View;
using Main.Core.View.Export;
using Moq;
using NUnit.Framework;
using Ninject;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Tests.Export;
using RavenQuestionnaire.Web.Utils;

namespace RavenQuestionnaire.Web.Tests.Utils
{
    [TestFixture]
    public class ExpressionReplacerTests
    {
        private IKernel kernel;
        private Mock<IViewRepository> viewRepositoryMock;

        [SetUp]
        public void Init()
        {
            this.kernel = new StandardKernel();

            var dummyQuestionnaireStataMapView = new QuestionnaireStataMapView
            {
                StataMap = new List<KeyValuePair<Guid, string>>()
                {
                    {new KeyValuePair<Guid, string>(Guid.Parse("a0d6ff6f-230e-4a1f-b940-97f93e037e08"), "caption1")}, 
                    {new KeyValuePair<Guid, string>(Guid.Parse("9e7bf746-ba13-4b53-aa1c-c0e5d9b2a1e0"), "caption2")}, 
                    {new KeyValuePair<Guid, string>(Guid.Parse("702ed23e-5e83-4c68-852c-7222c7e9ab3d"), "caption3")}, 
                    {new KeyValuePair<Guid, string>(Guid.Parse("589d55a8-6c14-4dd3-9e79-15fa2762f2e1"), "caption4")}, 
                    {new KeyValuePair<Guid, string>(Guid.Parse("bae427c7-c116-4327-b428-768ff26fd7a0"), "caption5")},
                }
            };

            this.viewRepositoryMock = Mock.Get(Mock.Of<IViewRepository>(repository
                => repository.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()) == dummyQuestionnaireStataMapView));

            this.kernel.Bind<IViewRepository>().ToConstant(this.viewRepositoryMock.Object);
        }

        [TearDown]
        public void Dispose()
        {
            this.kernel.Dispose();
        }

        #region Replacing stata captions
        [Test]
        public void ReplaceStataCaptionsWithGuids_When_expression_is_empty_Then_factory_should_not_be_called()
        {
            // Arrange
            var replacer = CreateExpressionReplacer();
            var emptyExpression = "";

            // Act
            replacer.ReplaceStataCaptionsWithGuids(emptyExpression, Guid.NewGuid());

            // Assert
            this.viewRepositoryMock.Verify(x => x.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()), Times.Never());
        }

        [Test]
        public void ReplaceStataCaptionsWithGuids_When_expression_is_not_empty_Then_factory_should_be_called_only_ones()
        {
            // Arrange
            var replacer = CreateExpressionReplacer();
            var notEmptyExpression = "some expression";

            // Act
            replacer.ReplaceStataCaptionsWithGuids(notEmptyExpression, Guid.NewGuid());

            // Assert
            this.viewRepositoryMock.Verify(x => x.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()), Times.Once());
        }

        [Test]
        public void ReplaceStataCaptionsWithGuids_When_expression_has_no_stata_captions_Then_result_should_match_with_given_expression()
        {
            // Arrange
            var replacer = CreateExpressionReplacer();
            var noStataCaptionsExpression = "[a0d6ff6f-230e-4a1f-b940-97f93e037e08] == 8 or [a0d6ff6f-230e-4a1f-b940-97f93e037e08] > [9e7bf746-ba13-4b53-aa1c-c0e5d9b2a1e0]";

            // Act
            var result = replacer.ReplaceStataCaptionsWithGuids(noStataCaptionsExpression, Guid.NewGuid());

            // Assert
            Assert.That(result, Is.EqualTo(noStataCaptionsExpression));
        }
        
        #endregion

        #region Replacing guids

        [Test]
        public void ReplaceGuidsWithStataCaptions_When_expression_contains_guids_Then_all_known_should_be_replaced()
        {
            // Arrange
            var replacer = CreateExpressionReplacer();
            var expressionWithGuids = "[a0d6ff6f-230e-4a1f-b940-97f93e037e08] == 8 or [a0d6ff6f-230e-4a1f-b940-97f93e037e08] > [9e7bf746-ba13-4b53-aa1c-c0e5d9b2a1e0]";

            // Act
            var result = replacer.ReplaceGuidsWithStataCaptions(expressionWithGuids, Guid.NewGuid());

            // Assert
            var expectedExpressionWithStataCaptions = "[caption1] == 8 or [caption1] > [caption2]";
            Assert.That(result, Is.EqualTo(expectedExpressionWithStataCaptions));
        }


        [Test]
        public void ReplaceGuidsWithStataCaptions_When_expression_is_empty_Then_factory_should_not_be_called()
        {
            // Arrange
            var replacer = CreateExpressionReplacer();
            var emptyExpression = "";

            // Act
            replacer.ReplaceGuidsWithStataCaptions(emptyExpression, Guid.NewGuid());

            // Assert
            this.viewRepositoryMock.Verify(x => x.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()), Times.Never());
        }

        [Test]
        public void ReplaceGuidsWithStataCaptions_When_expression_is_not_empty_Then_factory_should_be_called_only_ones()
        {
            // Arrange
            var replacer = CreateExpressionReplacer();
            var notEmptyExpression = "some expression";

            // Act
            replacer.ReplaceGuidsWithStataCaptions(notEmptyExpression, Guid.NewGuid());

            // Assert
            this.viewRepositoryMock.Verify(x => x.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()), Times.Once());
        }

        [Test]
        public void ReplaceGuidsWithStataCaptions_When_expression_contains_no_guids_Then_result_should_match_with_given_expression()
        {
            // Arrange
            var replacer = CreateExpressionReplacer();
            var guidLessExpression = "[age] == 8 or [weight] > [tall]";

            // Act
            var result = replacer.ReplaceGuidsWithStataCaptions(guidLessExpression, Guid.NewGuid());

            // Assert
            Assert.That(result, Is.EqualTo(guidLessExpression));
        }

        [Test]
        public void ReplaceGuidsWithStataCaptions_When_expression_contains_stata_captions_Then_all_known_should_be_replaced()
        {
            // Arrange
            var replacer = CreateExpressionReplacer();
            var expressionWithGuids = "[caption1] == 8 or [caption1] > [caption2]";

            // Act
            var result = replacer.ReplaceGuidsWithStataCaptions(expressionWithGuids, Guid.NewGuid());

            // Assert
            var expectedExpressionWithGuids = "[a0d6ff6f-230e-4a1f-b940-97f93e037e08] == 8 or [a0d6ff6f-230e-4a1f-b940-97f93e037e08] > [9e7bf746-ba13-4b53-aa1c-c0e5d9b2a1e0]";
            Assert.That(result, Is.EqualTo(expectedExpressionWithGuids));
        }
        #endregion

        private ExpressionReplacer CreateExpressionReplacer()
        {
            return new ExpressionReplacer(this.viewRepositoryMock.Object);
        }
    }
}
