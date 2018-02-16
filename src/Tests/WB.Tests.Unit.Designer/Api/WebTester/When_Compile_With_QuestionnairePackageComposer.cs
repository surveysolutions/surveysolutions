using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using AutoFixture;
using AutoFixture.AutoMoq;
using Main.Core.Documents;
using Microsoft.CodeAnalysis;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;
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
        private Mock<IExpressionProcessorGenerator> assemblyGeneratorMock;

        [SetUp]
        public void Arrange()
        {
            fixture = Abc.Create.Other.AutoFixture();
            document = Create.QuestionnaireDocumentWithOneChapter(Id.gA, Create.NumericIntegerQuestion());
            questionnaireView = Create.QuestionnaireView(document);

            // will make provided type singletone
            fixture.Freeze<Mock<IQuestionnaireViewFactory>>()
                .Setup(q => q.Load(It.IsAny<QuestionnaireViewInputModel>()))
                .Returns(questionnaireView);

            fixture.Freeze<Mock<IPlainStorageAccessor<QuestionnaireChangeRecord>>>()
                .Setup(q => q.Query(It.IsAny<Func<IQueryable<QuestionnaireChangeRecord>, int?>>())).Returns(1);

            // ReSharper disable once RedundantAssignment - value will be used in GenerateProcessorStateAssembly usage
            string assembly = fixture.Create<string>();

            assemblyGeneratorMock = fixture.Freeze<Mock<IExpressionProcessorGenerator>>();
            assemblyGeneratorMock
                .Setup(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly))
                .Returns(new GenerationResult(true, Array.Empty<Diagnostic>()));

            subj = fixture.Create<QuestionnairePackageComposer>();
        }

        [Test]
        public void should_get_questionnaire_once()
        {
            // ACT
            result = subj.ComposeQuestionnaire(Id.gA);

            fixture.Create<Mock<IQuestionnaireViewFactory>>()
                .Verify(m => m.Load(It.IsAny<QuestionnaireViewInputModel>()), Times.Once());
        }

        [Test]
        public void should_call_generateProcessorStateAssembly_once()
        {
            // ACT
            result = subj.ComposeQuestionnaire(Id.gA);

            string assembly;
            assemblyGeneratorMock
                .Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Once);
        }

        [Test]
        public void should_cache_questionnaire_generation_and_call_generateProcessorStateAssembly_once()
        {
            // ACT
            result = subj.ComposeQuestionnaire(Id.gA);

            // calling compose two more times
            var result1 = subj.ComposeQuestionnaire(Id.gA);
            var result2 = subj.ComposeQuestionnaire(Id.gA);

            string assembly;
            // should still generate assembly only once
            assemblyGeneratorMock
                .Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Once);

            Assert.That(result, Is.EqualTo(result1));
            Assert.That(result, Is.EqualTo(result2));
        }

        [Test]
        public void should_invalidate_cache_if_questionnaire_changed_and_call_generateProcessorStateAssembly_once()
        {
            // ACT
            result = subj.ComposeQuestionnaire(Id.gA);

            string assembly;
            // should still generate assembly only once
            assemblyGeneratorMock
                .Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Once);

            fixture.Freeze<Mock<IPlainStorageAccessor<QuestionnaireChangeRecord>>>()
                .Setup(q => q.Query(It.IsAny<Func<IQueryable<QuestionnaireChangeRecord>, int?>>())).Returns(2);

            assemblyGeneratorMock.ResetCalls();
            subj.ComposeQuestionnaire(Id.gA);
            subj.ComposeQuestionnaire(Id.gA);

            assemblyGeneratorMock
                .Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Once);
        }

        [Test]
        public void should_not_cache_questionnaire_with_error()
        {
            string assembly;

            assemblyGeneratorMock
                .Setup(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly))
                .Throws<HttpException>();

            // Act
            Assert.Throws<HttpResponseException>(() => result = subj.ComposeQuestionnaire(Id.gA));

            // ensure that generate processor were called
            assemblyGeneratorMock.Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Once);
            assemblyGeneratorMock.ResetCalls();

            // should not cache error, and throw again and try to generate assembly
            Assert.Throws<HttpResponseException>(() => result = subj.ComposeQuestionnaire(Id.gA));
            assemblyGeneratorMock.Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Once);
        }
    }
}