using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using System.Threading;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_interview_export_view_by_interview_with_1_answerd_double_question_in_russian_culture : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            dateTimeQuestionId = Guid.Parse("10000000000000000000000000000000");

            interviewData =
                Create.Entity.InterviewData(Create.Entity.InterviewQuestion(questionId: dateTimeQuestionId,
                    answer: value));

            questionnaireDocument =
                Create.Entity.QuestionnaireDocument(children: Create.Entity.NumericRealQuestion(id: dateTimeQuestionId, variable: "real"));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(questionnaire);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
            BecauseOf();
        }

        public void BecauseOf()
        {
            var originalCulture = new
            {
                Culture = Thread.CurrentThread.CurrentCulture,
                UICulture = Thread.CurrentThread.CurrentUICulture
            };

            try
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                result = exportViewFactory.CreateInterviewDataExportView(
                            exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaireDocument.PublicKey, 1)),
                            interviewData, questionnaire);
            }
            finally 
            {
                Thread.CurrentThread.CurrentCulture = originalCulture.Culture;
                Thread.CurrentThread.CurrentUICulture = originalCulture.UICulture;
            }
            
        }


        [NUnit.Framework.Test] public void should_create_record__with_one_datetime_question_which_contains_composite_answer () =>
          result.Levels[0].Records[0].Answers.First().Should().BeEquivalentTo(new[] { value.ToString(CultureInfo.InvariantCulture)  });

        private static ExportViewFactory exportViewFactory;
        private static InterviewDataExportView result;
        private static Guid dateTimeQuestionId;
        private static IQuestionnaire questionnaire;
        private static QuestionnaireDocument questionnaireDocument;
        private static InterviewData interviewData;
        private static double value = 5.55;

        private static CultureInfo culture = CultureInfo.GetCultureInfo("ru-ru");
    }
}
