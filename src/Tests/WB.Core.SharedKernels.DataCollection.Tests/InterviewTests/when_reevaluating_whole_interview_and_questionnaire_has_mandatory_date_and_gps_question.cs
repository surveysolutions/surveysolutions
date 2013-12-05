using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_reevaluating_whole_interview_and_questionnaire_has_mandatory_date_and_gps_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");

            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            geoQuestionId = Guid.Parse("20000000000000000000000000000000");

            var questionnaireDocument = new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    new Group("Chapter")
                    {
                        Children = new List<IComposite>()
                        {
                            new GpsCoordinateQuestion("Current location")
                            {
                                PublicKey = geoQuestionId,
                                Mandatory = true
                            }
                        }
                    }
                }
            };
           
            var questionaire = new Questionnaire(questionnaireDocument);

            var expressionProcessor = new Mock<IExpressionProcessor>();

            //setup expression processor throw exception
            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>()))
                .Returns(true);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(expressionProcessor.Object);

            eventContext = new EventContext();

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new GeoLocationQuestionAnswered(userId, geoQuestionId, new int[0], DateTime.Now, 0.111,0.222,333,new DateTimeOffset(DateTime.Now)));
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.ReevaluateSynchronizedInterview();

        It should_raise_AnswerDeclaredInvalid_event_with_QuestionId_equal_to_geoQuestionId = () =>
            eventContext.ShouldContainEvent<AnswerDeclaredInvalid>(@event => @event.QuestionId == geoQuestionId);

        It should_raise_AnswerDeclaredValid_event_with_QuestionId_equal_to_geoQuestionId = () =>
            eventContext.ShouldContainEvent<AnswerDeclaredValid>(@event => @event.QuestionId == geoQuestionId);

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static Interview interview;
        private static Guid geoQuestionId;
    }
}