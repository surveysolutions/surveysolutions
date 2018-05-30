using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_interview_has_yesno_question_with_sorted_answers : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            variableName = "yesno";
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(id: questionId,
                    variable: variableName,
                    options: new List<Answer> {
                        Create.Entity.Answer("foo", 28),
                        Create.Entity.Answer("bar", 42),
                        Create.Entity.Answer("blah", 21),
                        Create.Entity.Answer("bar_null", 15)
                    }, areAnswersOrdered:true,
                    yesNoView: true));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(questionnaire);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);

            questionnaaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument.PublicKey, 1);

            interview = Create.Entity.InterviewData(Create.Entity.InterviewQuestion(questionId, new[]
            {
                Create.Entity.AnsweredYesNoOption(21m, true),
                Create.Entity.AnsweredYesNoOption(42m, false),
                Create.Entity.AnsweredYesNoOption(28m, true),
            }));
            BecauseOf();
        }

        public void BecauseOf() => result = exportViewFactory.CreateInterviewDataExportView(questionnaaireExportStructure, interview, questionnaire);

        [NUnit.Framework.Test] public void should_fill_yesno_question_answer_with_order () 
        {
            InterviewDataExportLevelView first = result.Levels.First();
            var exportedQuestion = first.Records.First().GetPlainAnswers().First();
            exportedQuestion.Length.Should().Be(4);
            exportedQuestion.Should().BeEquivalentTo(new[] { "2", "0", "1", ExportFormatSettings.MissingNumericQuestionValue }); // 1 0 2
        }

        static ExportViewFactory exportViewFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static Guid questionId;
        static InterviewData interview;
        static InterviewDataExportView result;
        static string variableName;
        static IQuestionnaire questionnaire;
    }
}
