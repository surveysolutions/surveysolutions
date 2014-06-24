using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.InterviewSynchronizationDtoFactoryTests
{
    internal class when_build_from_interview_created_onclient : InterviewSynchronizationDtoFactoryTestContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("21111111111111111111111111111111");
            createdOnClientFlag = true;
            interviewData = CreateInterviewData();
            interviewData.CreatedOnClient = createdOnClientFlag;

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion("n1")
                {
                    QuestionType = QuestionType.Numeric,
                    PublicKey = questionId
                });

            interviewSynchronizationDtoFactory = CreateInterviewSynchronizationDtoFactory(questionnaireDocument);
        };

        Because of = () =>
            result = interviewSynchronizationDtoFactory.BuildFrom(interviewData);

        It should_result_has_created_onclient_state = () =>
            result.CreatedOnClient.ShouldEqual(createdOnClientFlag);

        private static InterviewSynchronizationDtoFactory interviewSynchronizationDtoFactory;
        private static InterviewData interviewData;
        private static InterviewSynchronizationDto result;
        private static QuestionnaireDocument questionnaireDocument;
        
        private static Guid questionId;

        private static bool createdOnClientFlag;
    }
}
