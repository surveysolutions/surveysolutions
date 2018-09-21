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
    internal class when_creating_interview_export_view_by_interview_with_1_answered_datetime_question : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            dateTimeQuestionId = Guid.Parse("10000000000000000000000000000000");

            interviewData =
                Create.Entity.InterviewData(Create.Entity.InterviewQuestion(questionId: dateTimeQuestionId,
                    answer: date));

            questionnaireDocument =
                Create.Entity.QuestionnaireDocument(children: Create.Entity.DateTimeQuestion(questionId: dateTimeQuestionId, variable: "dateTime"));

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

        [NUnit.Framework.Test] public void should_create_record__with_one_datetime_question_which_contains_composite_answer () =>
          result.Levels[0].Records[0].Answers.First().Should().BeEquivalentTo(new[] { "1984-04-18"  });

        private static ExportViewFactory exportViewFactory;
        private static InterviewDataExportView result;
        private static Guid dateTimeQuestionId;
        private static IQuestionnaire questionnaire;
        private static QuestionnaireDocument questionnaireDocument;
        private static InterviewData interviewData;
        private static DateTime date = new DateTime(1984, 4, 18, 18, 4, 19);
    }
}
