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
        public IKernel Kernel { get; set; }

        public Mock<IViewRepository> ViewRepositoryMock { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            this.Kernel = new StandardKernel();
            this.ViewRepositoryMock = new Mock<IViewRepository>();
            this.ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()))
                .Returns(new QuestionnaireStataMapView()
                    {
                        StataMap = new List<KeyValuePair<Guid, string>>()
                            {
                                {new KeyValuePair<Guid, string>(Guid.NewGuid(), "caption1")},
                                {new KeyValuePair<Guid, string>(Guid.NewGuid(), "caption2")},
                                {new KeyValuePair<Guid, string>(Guid.NewGuid(), "caption3")},
                                {new KeyValuePair<Guid, string>(Guid.NewGuid(), "caption4")},
                                {new KeyValuePair<Guid, string>(Guid.NewGuid(), "caption5")}
                            }
                    });
            this.Kernel.Bind<IViewRepository>().ToConstant(this.ViewRepositoryMock.Object);
        }

        [Test]
        public void ReplaceStataCaptionsWithGuids_When_expression_is_empty_Then_factory_doesnot_loading()
        {
            // Arrange
            var replacer = CreateExpressionReplacer();
            var emptyExpression = "";

            // Act
            replacer.ReplaceStataCaptionsWithGuids(emptyExpression, Guid.NewGuid());

            // Assert
            ViewRepositoryMock.Verify(x => x.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()), Times.Never());
        }

        [Test]
        public void ReplaceGuidsWithStataCaptions_When_expression_is_empty_Then_factory_doesnot_loading()
        {
            // Arrange
            var replacer = CreateExpressionReplacer();
            var emptyExpression = "";

            // Act
            replacer.ReplaceGuidsWithStataCaptions(emptyExpression, Guid.NewGuid());
            
            // Assert
            ViewRepositoryMock.Verify(x => x.Load<QuestionnaireViewInputModel, QuestionnaireStataMapView>(It.IsAny<QuestionnaireViewInputModel>()), Times.Never());
        }

        private ExpressionReplacer CreateExpressionReplacer()
        {
            return new ExpressionReplacer(ViewRepositoryMock.Object);
        }
    }
}
