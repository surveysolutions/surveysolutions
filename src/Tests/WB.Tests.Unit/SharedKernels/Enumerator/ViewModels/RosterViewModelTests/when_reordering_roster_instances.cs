using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestFixture]
    internal class when_reordering_roster_instances : RosterViewModelTests
    {
        [Test]
        public async Task should_reorder_roster_instances_in_the_list()
        {
            var interviewId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Id.gA, children: new IComposite[]
            {
                Create.Entity.YesNoQuestion(questionId: Id.g1, answers: new[]{ 1, 2, 3 }, ordered: true),
                Create.Entity.Roster(rosterId: Id.g2, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: Id.g1),
            });

            var interview = SetUp.StatefulInterview(questionnaire);
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: Id.g1,
                answeredOptions: new[]
                {
                    Create.Entity.AnsweredYesNoOption(1, true),
                    Create.Entity.AnsweredYesNoOption(2, true),
                    Create.Entity.AnsweredYesNoOption(3, true),
                }));

            var statefulInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            IViewModelEventRegistry registry = Create.Service.LiteEventRegistry();

            var viewModel = this.CreateViewModel(statefulInterviewRepository, eventRegistry: registry);

            var navigationState = Create.Other.NavigationState(statefulInterviewRepository);

            await navigationState.NavigateTo(Create.Entity.NavigationIdentity(Identity.Create(Id.gA, RosterVector.Empty)));

            viewModel.Init(interviewId.ToString("N"), Create.Identity(Id.g2), navigationState);

            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: Id.g1,
                answeredOptions: new[]
                {
                    Create.Entity.AnsweredYesNoOption(1, false),
                    Create.Entity.AnsweredYesNoOption(2, false),
                    Create.Entity.AnsweredYesNoOption(3, false),
                }));
            Abc.SetUp.ApplyInterviewEventsToViewModels(interview, registry, interviewId);

            // act
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(
                questionId: Id.g1,
                answeredOptions: new[]
                {
                    Create.Entity.AnsweredYesNoOption(value: 2, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: 3, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: 1, answer: true),
                }));
            Abc.SetUp.ApplyInterviewEventsToViewModels(interview, registry, interviewId);

            //assert
            var rosters = viewModel.RosterInstances.Select(x => x.Identity).ToArray();
            Assert.That(rosters[0].RosterVector.Last(), Is.EqualTo(2));
            Assert.That(rosters[1].RosterVector.Last(), Is.EqualTo(3));
            Assert.That(rosters[2].RosterVector.Last(), Is.EqualTo(1));
        }
    }
}
