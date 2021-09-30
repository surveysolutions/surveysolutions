using System;
using AutoFixture;

using Main.Core.Documents;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Tests.Abc;
using WB.UI.Designer.Api.WebTester;
using WB.UI.Designer.Controllers.Api.WebTester;

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
        private DesignerDbContext dbContext;

        [SetUp]
        public void Arrange()
        {
            fixture = Create.AutoFixture();
            document = Create.QuestionnaireDocumentWithOneChapter(questionnaireId: Id.gA, children: Create.NumericIntegerQuestion());
            questionnaireView = Create.QuestionnaireView(document);

            // will make provided type singleton
            fixture.Freeze<Mock<IQuestionnaireViewFactory>>()
                .Setup(q => q.Load(It.IsAny<QuestionnaireViewInputModel>()))
                .Returns(questionnaireView);

            fixture.Register<IQuestionnaireCacheStorage>(() => new QuestionnaireCacheStorage(new MemoryCache(Options.Create(new MemoryCacheOptions()))));

            fixture.Register<DesignerDbContext>(() =>
            {
                var inMemoryDbContext = Create.InMemoryDbContext();
                var questionnaireChangeRecord = Create.QuestionnaireChangeRecord(questionnaireId: document.PublicKey.FormatGuid(), sequence: 1);
                inMemoryDbContext.QuestionnaireChangeRecords.Add(
                    questionnaireChangeRecord);
                inMemoryDbContext.SaveChanges();
                return inMemoryDbContext;
            });

            dbContext = fixture.Freeze<DesignerDbContext>();
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
           
            // Change questionnaire to invalidate cache
            var questionnaireChangeRecord = Create.QuestionnaireChangeRecord(questionnaireId: document.PublicKey.FormatGuid(), sequence: 2);
            dbContext.QuestionnaireChangeRecords.Add(questionnaireChangeRecord);
            dbContext.SaveChanges();

            assemblyGeneratorMock.Invocations.Clear();

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
                .Throws<Exception>();

            // Act
            Assert.Throws<ComposeException>(() => result = subj.ComposeQuestionnaire(Id.gA));

            // ensure that generate processor were called
            assemblyGeneratorMock.Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Once);
            assemblyGeneratorMock.Invocations.Clear();

            // should not cache error, and throw again and try to generate assembly
            Assert.Throws<ComposeException>(() => result = subj.ComposeQuestionnaire(Id.gA));
            assemblyGeneratorMock.Verify(m => m.GenerateProcessorStateAssembly(document, It.IsAny<int>(), out assembly), Times.Once);
        }
    }
}
