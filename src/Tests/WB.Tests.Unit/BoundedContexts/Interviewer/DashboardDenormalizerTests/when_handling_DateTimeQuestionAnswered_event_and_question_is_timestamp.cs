using System;
using System.Globalization;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Eventing.ServiceModel.Bus;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_DateTimeQuestionAnswered_event_and_question_is_timestamp
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("22222222222222222222222222222222");

            dateTimeQuestionIdentity = Create.Entity.Identity("11111111111111111111111111111111", RosterVector.Empty);
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("33333333333333333333333333333333"), 1);
            
            @event = Create.Event.DateTimeQuestionAnswered(interviewId, dateTimeQuestionIdentity, answer).ToPublishedEvent();

            interviewViewStorage = new SqliteInmemoryStorage<InterviewView>();
            interviewViewStorage.Store(Create.Entity.InterviewView(interviewId: interviewId,
                questionnaireId: questionnaireIdentity.ToString()));

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version,
                questionnaire: Create.Entity.PlainQuestionnaire(
                    Create.Entity.QuestionnaireDocument(questionnaireIdentity.QuestionnaireId, new IComposite[]
                    {
                        Create.Entity.DateTimeQuestion(questionId: dateTimeQuestionIdentity.Id, preFilled: true, isTimestamp: true)
                    })));

            denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage, questionnaireStorage: plainQuestionnaireRepository);
        };

        Because of = () => denormalizer.Handle(@event);

        It should_set_formatted_date_to_specified_answer_view = () =>
            interviewViewStorage.GetById(interviewId.FormatGuid())?.AnswersOnPrefilledQuestions?.FirstOrDefault(question => question.QuestionId == dateTimeQuestionIdentity.Id)?
                .Answer.ShouldEqual(answer.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern));

        private static InterviewerDashboardEventHandler denormalizer;
        private static IPublishedEvent<DateTimeQuestionAnswered> @event;
        private static SqliteInmemoryStorage<InterviewView> interviewViewStorage;
        private static DateTime answer = new DateTime(2016, 06, 08, 12, 49, 0);
        private static Guid interviewId;
        private static Identity dateTimeQuestionIdentity;
    }
}