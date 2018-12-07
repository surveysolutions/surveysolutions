using System;
using System.Linq;
using AppDomainToolkit;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Hubs;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;
using WB.Tests.Integration.InterviewTests;
using WB.UI.Headquarters.API.WebInterview;

namespace WB.Tests.Integration.WebInterviewTests
{
    [TestOf(typeof(WebInterviewHub))]

    public class WebInterview_PlainMode_Tests : InterviewTestsContext
    {
        [Test]
        public void GetCompleteInfo_for_entities_with_errors_placed_in_plain_roster_shoud_return_correct_parent_link()
        {
            var sectionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var appDomainContext = AppDomainContext.Create();

            var completeInfo = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
                {
                    Create.Entity.Group(groupId, children: new IComposite[]
                    {
                        Create.Entity.FixedRoster(isPlainMode: true, fixedTitles: new FixedRosterTitle[]
                        {
                            Create.Entity.FixedTitle(1, "1"),
                            Create.Entity.FixedTitle(2, "2"),
                        }, children: new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(intQuestionId, 
                                validationConditions: new []{ Create.Entity.ValidationCondition("self > 100") }
                            )
                        })
                    })
                });

                var statefulInterview = SetupStatefullInterview(questionnaireDocument);

                statefulInterview.AnswerNumericIntegerQuestion(currentUserId, intQuestionId, Create.RosterVector(1), DateTimeOffset.UtcNow, 5);
                var webInterview = CreateWebInterview(statefulInterview, questionnaireDocument);

                return webInterview.GetCompleteInfo();
            });


            Assert.That(completeInfo.EntitiesWithError.Length, Is.EqualTo(1));
            Assert.That(completeInfo.EntitiesWithError.Single().Id, Is.EqualTo(Create.Identity(intQuestionId, Create.RosterVector(1)).ToString()));
            Assert.That(completeInfo.EntitiesWithError.Single().ParentId, Is.EqualTo(groupId.FormatGuid()));
        }

        private WebInterviewHub CreateWebInterview(IStatefulInterview statefulInterview, QuestionnaireDocument questionnaireDocument)
        {
            var statefulInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocument);
            var webInterviewInterviewEntityFactory = Create.Service.WebInterviewInterviewEntityFactory();

            var webInterviewHub = new WebInterviewHub(statefulInterviewRepository,
                Mock.Of<ICommandService>(),
                questionnaireStorage,
                Mock.Of<IWebInterviewNotificationService>(),
                webInterviewInterviewEntityFactory,
                Mock.Of<IImageFileStorage>(),
                Mock.Of<IInterviewBrokenPackagesService>(),
                Mock.Of<IAudioFileStorage>(),
                Mock.Of<IAuthorizedUser>(),
                Mock.Of<IChangeStatusFactory>(),
                Mock.Of<IInterviewFactory>(),
                Mock.Of<IStatefullInterviewSearcher>(),
                Mock.Of<IInterviewOverviewService>(),
                Mock.Of<IUnitOfWork>());

            webInterviewHub.Context = Mock.Of<HubCallerContext>(h =>
                h.QueryString == Mock.Of<INameValueCollection>(p =>
                    p["interviewId"] == statefulInterview.Id.FormatGuid()
                )
            );

            return webInterviewHub;
        }
    }
}
