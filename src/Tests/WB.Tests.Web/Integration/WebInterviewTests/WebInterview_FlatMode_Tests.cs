using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Enumerator.Native.WebInterview;
using WB.Tests.Abc;
using WB.Tests.Web.TestFactories;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Controllers.Api.WebInterview;
using WB.UI.Headquarters.Services;

namespace WB.Tests.Web.Integration.WebInterviewTests
{
    [TestOf(typeof(InterviewDataController))]
    public class WebInterview_FlatMode_Tests : InterviewTestsContext
    {
        private InterviewDataController CreateInterviewDataController(IStatefulInterview statefulInterview,
            QuestionnaireDocument questionnaireDocument)
        {
            var statefulInterviewRepository = Abc.Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireStorage = Abc.Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocument);
            var webInterviewInterviewEntityFactory = Web.Create.Service.WebInterviewInterviewEntityFactory();

            var controller = new InterviewDataController(questionnaireStorage,
                statefulInterviewRepository,
                Mock.Of<IWebInterviewNotificationService>(),
                webInterviewInterviewEntityFactory,
                Mock.Of<IAuthorizedUser>(),
                Mock.Of<IInterviewOverviewService>(),
                Mock.Of<IStatefulInterviewSearcher>(),
                Mock.Of<IInterviewFactory>(),
                Mock.Of<IChangeStatusFactory>(),
                Mock.Of<IInterviewBrokenPackagesService>());

            return controller;
        }

        [Test]
        public void GetCompleteInfo_for_entities_with_errors_placed_in_plain_roster_should_return_correct_parent_link()
        {
            var sectionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId,
                Abc.Create.Entity.Group(groupId, children: new IComposite[]
                {
                    Abc.Create.Entity.FixedRoster(displayMode: RosterDisplayMode.Flat,
                        fixedTitles: new[]
                        {
                            Abc.Create.Entity.FixedTitle(1, "1"),
                            Abc.Create.Entity.FixedTitle(2, "2")
                        }, children: new IComposite[]
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(intQuestionId,
                                validationConditions: new[] {Abc.Create.Entity.ValidationCondition("self > 100")}
                            )
                        })
                }));


            var statefulInterview = Abc.Create.AggregateRoot.StatefulInterview(
                Id.gA,
                questionnaire: questionnaireDocument,
                expressionStorageType: AssemblyCompiler.GetInterviewExpressionStorage(questionnaireDocument).GetType());

            statefulInterview.AnswerNumericIntegerQuestion(
                Abc.Create.Command.AnswerNumericIntegerQuestionCommand(
                    statefulInterview.Id,
                    currentUserId,
                    intQuestionId,
                    5,
                    Abc.Create.RosterVector(1)));

            var webInterview = CreateInterviewDataController(statefulInterview, questionnaireDocument);

            var completeInfo = webInterview.GetCompleteInfo(statefulInterview.Id);

            Assert.That(completeInfo.EntitiesWithError.Length, Is.EqualTo(1));
            Assert.That(completeInfo.EntitiesWithError.Single().Id,
                Is.EqualTo(Abc.Create.Identity(intQuestionId, Abc.Create.RosterVector(1)).ToString()));
            Assert.That(completeInfo.EntitiesWithError.Single().ParentId, Is.EqualTo(groupId.FormatGuid()));
        }
    }
}
