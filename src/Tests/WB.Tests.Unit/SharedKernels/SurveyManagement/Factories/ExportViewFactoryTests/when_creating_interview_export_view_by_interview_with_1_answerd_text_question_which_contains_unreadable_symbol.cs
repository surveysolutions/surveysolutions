using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
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
    internal class when_creating_interview_export_view_by_interview_with_1_answerd_text_question_which_contains_unreadable_symbol : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            textQuestionId = Guid.Parse("10000000000000000000000000000000");

            interviewData =
                Create.Entity.InterviewData(Create.Entity.InterviewQuestion(questionId: textQuestionId,
                    answer: text));

            questionnaireDocument =
                Create.Entity.QuestionnaireDocument(children: Create.Entity.DateTimeQuestion(questionId: textQuestionId, variable: "txt"));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(questionnaire);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument.PublicKey, 1),
                interviewData, questionnaire);

        [NUnit.Framework.Test] public void should_create_record_with_one_text_question () =>
            result.Levels[0].Records[0].GetPlainAnswers().First().Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_create_record_with_one_text_question_which_answered_and_doesnt_contain_the_unreadable_symbol () =>
          result.Levels[0].Records[0].GetPlainAnswers().First().Should().BeEquivalentTo(new[] { "231 Pietermaritz StPietermaritzburg3201" });

        private static ExportViewFactory exportViewFactory;
        private static InterviewDataExportView result;
        private static Guid textQuestionId;
        private static QuestionnaireDocument questionnaireDocument;
        private static IQuestionnaire questionnaire;
        private static InterviewData interviewData;
        private static string text = "231 Pietermaritz St\u263APietermaritzburg\u263A3201";
    }
}
