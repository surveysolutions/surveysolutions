using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Hubs;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.API.WebInterview;

namespace WB.Tests.Integration.WebInterviewTests
{
    public class InterviewTestsContext
    {
    }

    [TestOf(typeof(WebInterviewHub))]
    public class WebInterview_FlatMode_Tests : InterviewTestsContext
    {
        [Test]
        public void GetCompleteInfo_for_entities_with_errors_placed_in_plain_roster_should_return_correct_parent_link()
        {
            var sectionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();


            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId,
                new IComposite[]
                {
                    Abc.Create.Entity.Group(groupId, children: new IComposite[]
                    {
                        Abc.Create.Entity.FixedRoster(displayMode: RosterDisplayMode.Flat,
                            fixedTitles: new FixedRosterTitle[]
                            {
                                Abc.Create.Entity.FixedTitle(1, "1"),
                                Abc.Create.Entity.FixedTitle(2, "2"),
                            }, children: new IComposite[]
                            {
                                Abc.Create.Entity.NumericIntegerQuestion(intQuestionId,
                                    validationConditions: new[] {Abc.Create.Entity.ValidationCondition("self > 100")}
                                )
                            })
                    })
                });

            StatefulInterview statefulInterview = Abc.Create.AggregateRoot.StatefulInterview(questionnaire: questionnaireDocument);

            statefulInterview.AnswerNumericIntegerQuestion(
                Abc.Create.Command.AnswerNumericIntegerQuestionCommand(
                    interviewId: statefulInterview.Id,
                    userId: currentUserId,
                    questionId: intQuestionId,
                    answer: 5,
                    Abc.Create.RosterVector(1)));
            var webInterview = CreateWebInterview(statefulInterview, questionnaireDocument);

            var completeInfo = webInterview.GetCompleteInfo();


            Assert.That(completeInfo.EntitiesWithError.Length, Is.EqualTo(1));
            Assert.That(completeInfo.EntitiesWithError.Single().Id,
                Is.EqualTo(Abc.Create.Identity(intQuestionId, Abc.Create.RosterVector(1)).ToString()));
            Assert.That(completeInfo.EntitiesWithError.Single().ParentId, Is.EqualTo(groupId.FormatGuid()));
        }

        private WebInterviewHub CreateWebInterview(IStatefulInterview statefulInterview,
            QuestionnaireDocument questionnaireDocument)
        {
            var statefulInterviewRepository = Abc.Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireStorage = Abc.Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocument);
            var webInterviewInterviewEntityFactory = Web.Create.Service.WebInterviewInterviewEntityFactory();

            var serviceLocator = Mock.Of<IServiceLocator>(sl =>
                sl.GetInstance<IStatefulInterviewRepository>() == statefulInterviewRepository
                && sl.GetInstance<IQuestionnaireStorage>() == questionnaireStorage
                && sl.GetInstance<IWebInterviewInterviewEntityFactory>() == webInterviewInterviewEntityFactory
                && sl.GetInstance<IAuthorizedUser>() == Mock.Of<IAuthorizedUser>());

            var webInterviewHub = new WebInterviewHub();
            webInterviewHub.SetServiceLocator(serviceLocator);

            webInterviewHub.Context = Mock.Of<HubCallerContext>(h =>
                h.QueryString == Mock.Of<INameValueCollection>(p =>
                    p["interviewId"] == statefulInterview.Id.FormatGuid()
                )
            );

            return webInterviewHub;
        }
    }
}
