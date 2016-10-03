using System;
using System.Globalization;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    [TestFixture]
    internal class when_handling_DateTimeQuestionAnswered_event
    {
        [OneTimeSetUp]
        public void context ()
        {
            interviewId = Guid.Parse("22222222222222222222222222222222");

            dateTimeQuestionIdentity = Create.Entity.Identity("11111111111111111111111111111111", RosterVector.Empty);
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("33333333333333333333333333333333"), 1);
            
            @event = Create.Event.DateTimeQuestionAnswered(interviewId, dateTimeQuestionIdentity, answer).ToPublishedEvent(eventSourceId: interviewId);

            interviewViewStorage = new SqliteInmemoryStorage<InterviewView>();
            interviewViewStorage.Store(Create.Entity.InterviewView(interviewId: interviewId,
                questionnaireId: questionnaireIdentity.ToString()));

            IQuestionnaireStorage plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version,
                questionnaire: Create.Entity.PlainQuestionnaire(
                    Create.Entity.QuestionnaireDocument(questionnaireIdentity.QuestionnaireId, new IComposite[]
                    {
                        Create.Entity.DateTimeQuestion(questionId: dateTimeQuestionIdentity.Id, preFilled: true)
                    })));

            prefilledQuestions = new InMemoryPlainStorage<PrefilledQuestionView>();
            denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage, 
                questionnaireStorage: plainQuestionnaireRepository,
                prefilledQuestions: prefilledQuestions
                );
        }

        [SetUp]
        public void of() => denormalizer.Handle(@event);

        [Test]
        public void should_set_formatted_date_to_specified_answer_view() =>
            prefilledQuestions.Where(x => x.QuestionId == dateTimeQuestionIdentity.Id)
                .First()
                .Answer.ShouldEqual(answer.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern));

        static InterviewerDashboardEventHandler denormalizer;
        static IPublishedEvent<DateTimeQuestionAnswered> @event;
        static SqliteInmemoryStorage<InterviewView> interviewViewStorage;
        static DateTime answer = new DateTime(2016, 06, 08, 12, 49, 0);
        static Guid interviewId;
        static Identity dateTimeQuestionIdentity;
        static InMemoryPlainStorage<PrefilledQuestionView> prefilledQuestions;
    }
}