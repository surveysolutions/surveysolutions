using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestFixture]
    internal class when_reordering_roster_instances : RosterViewModelTests
    {
        [Test]
        public async Task should_reorder_roster_instances_in_the_list()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Id.gA, children: new IComposite[]
            {
                Create.Entity.YesNoQuestion(questionId: Id.g1, answers: new[]{ 1, 2, 3 }, ordered: true),
                Create.Entity.Roster(rosterId: Id.g2, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: Id.g1),
            });

            var interview = Setup.StatefulInterview(questionnaire);
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: Id.g1,
                answeredOptions: new[]
                {
                    Create.Entity.AnsweredYesNoOption(1, true),
                    Create.Entity.AnsweredYesNoOption(2, true),
                    Create.Entity.AnsweredYesNoOption(3, true),
                }));

            var statefulInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);
            var questionnaireStorage = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var viewModel = this.CreateViewModel(
                statefulInterviewRepository, 
                questionnaireRepository: questionnaireStorage);

            var navigationState = Create.Other.NavigationState(statefulInterviewRepository);

            await navigationState.NavigateTo(Create.Entity.NavigationIdentity(Identity.Create(Id.gA, RosterVector.Empty)));

            viewModel.Init(null, Create.Identity(Id.g2), navigationState);
          
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(
                questionId: Id.g1,
                answeredOptions: new[]
                {
                    Create.Entity.AnsweredYesNoOption(value: 2, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: 3, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: 1, answer: true),
                }));

            viewModel.Handle(Create.Event.YesNoQuestionAnswered(Id.g1, new []
            {
                Create.Entity.AnsweredYesNoOption(2, true),
                Create.Entity.AnsweredYesNoOption(3, true),
                Create.Entity.AnsweredYesNoOption(1, true),
            }));

            var rosters = viewModel.RosterInstances.Select(x => x.Identity).ToArray();
            Assert.That(rosters[0].RosterVector.Last(), Is.EqualTo(2));
            Assert.That(rosters[1].RosterVector.Last(), Is.EqualTo(3));
            Assert.That(rosters[2].RosterVector.Last(), Is.EqualTo(1));
        }
    }
}
