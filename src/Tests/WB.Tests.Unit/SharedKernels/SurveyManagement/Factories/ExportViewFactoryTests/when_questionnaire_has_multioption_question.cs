using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_questionnaire_has_multioption_question : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(id: questionId, 
                variable:"mult",
                options: new List<Answer> {
                    Create.Entity.Answer("foo", 28), Create.Entity.Answer("bar", 42)
                }));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(Create.Entity.PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() => questionnaaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaire.PublicKey, 1);

        [NUnit.Framework.Test] public void should_fill_multioption_question_header_title () 
        {
            HeaderStructureForLevel headerStructureForLevel = questionnaaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            ExportedQuestionHeaderItem exportedQuestionHeaderItem = headerStructureForLevel.HeaderItems[questionId] as ExportedQuestionHeaderItem;

            exportedQuestionHeaderItem.ColumnHeaders.Count.Should().Be(2);
            exportedQuestionHeaderItem.ColumnHeaders.Select(x=> x.Name).SequenceEqual(new[] { "mult__28", "mult__42" }).Should().BeTrue();
            exportedQuestionHeaderItem.ColumnValues.Length.Should().Be(2);
            exportedQuestionHeaderItem.ColumnValues.SequenceEqual(new[] {28, 42}).Should().BeTrue();
        }

        static ExportViewFactory exportViewFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static QuestionnaireDocument questionnaire;
        static Guid questionId;
    }
}
