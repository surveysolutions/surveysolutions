using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using Main.Core.Documents;
using Microsoft.CodeAnalysis;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Tests.Abc;
using WB.UI.Designer.Api.WebTester;

namespace WB.Tests.Unit.Designer.Api.WebTester
{
    [TestFixture]
    public class When_Compile_With_QuestionnairePackageComposer
    {
        private Questionnaire result;
        private QuestionnairePackageComposer subj;
        private IFixture fixture;
        private QuestionnaireDocument document;
        private QuestionnaireView questionnaireView;

        [SetUp]
        public void Arrange()
        {
            this.fixture = new Fixture().Customize(new AutoMoqCustomization());
            this.document = Create.QuestionnaireDocumentWithOneChapter(Id.gA, Create.NumericIntegerQuestion());
            this.questionnaireView = Create.QuestionnaireView(document);

            // will make provided type singletone
            fixture.Freeze<Mock<IQuestionnaireViewFactory>>()
                .Setup(q => q.Load(It.IsAny<QuestionnaireViewInputModel>()))
                .Returns(questionnaireView);

            string assembly = fixture.Create<string>();

            fixture.Freeze<Mock<IExpressionProcessorGenerator>>()
                .Setup(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly))
                .Returns(new GenerationResult(true, Array.Empty<Diagnostic>()));

            // ACT
            this.subj = fixture.Create<QuestionnairePackageComposer>();
            this.result = subj.ComposeQuestionnaire(Id.gA);
        }

        [Test]
        public void should_get_questionnaire_two_times()
        {
            fixture.Create<Mock<IQuestionnaireViewFactory>>().Verify(m => m.Load(It.IsAny<QuestionnaireViewInputModel>()), Times.Exactly(2));
        }

        [Test]
        public void should_call_generateProcessorStateAssembly_once()
        {
            string assembly;
            fixture.Create<Mock<IExpressionProcessorGenerator>>()
                .Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Once);
        }

        [Test]
        public void should_cache_questionnaire_generation_and_call_generateProcessorStateAssembly_once()
        {
            // calling compose two more times
            var result1 = subj.ComposeQuestionnaire(Id.gA);
            var result2 = subj.ComposeQuestionnaire(Id.gA);

            string assembly;
            // should still generate assembly only once
            fixture.Create<Mock<IExpressionProcessorGenerator>>()
                .Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Once);

            Assert.That(result, Is.EqualTo(result1));
            Assert.That(result, Is.EqualTo(result2));
        }

        [Test]
        public void should_invalidate_cache_if_questionnaire_changed_and_call_generateProcessorStateAssembly_twice()
        {
            string assembly;
            // should still generate assembly only once
            fixture.Create<Mock<IExpressionProcessorGenerator>>()
                .Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Once);

            this.questionnaireView.Source.LastEntryDate = DateTime.Now.AddDays(1);

            subj.ComposeQuestionnaire(Id.gA);
            subj.ComposeQuestionnaire(Id.gA);

            fixture.Create<Mock<IExpressionProcessorGenerator>>()
                .Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Exactly(2));
        }
    }
}