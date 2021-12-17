using System;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class MultiOptionLinkedQuestionViewModelTests : MultiOptionLinkedQuestionViewModelTestsContext
    {
        [Test]
        public async Task when_roster_row_title_changed_and_question_is_linked_on_it()
        {

            Identity questionId = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);
            Identity rosterId = Create.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(rosterId.Id, fixedTitles: new[] { new FixedRosterTitle(1, "title") }),
                Create.Entity.MultyOptionsQuestion(questionId.Id, linkedToRosterId: rosterId.Id)
            );

            StatefulInterview interview = Abc.SetUp.StatefulInterview(questionnaire);

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            CategoricalMultiLinkedToRosterTitleViewModel viewModel = CreateMultiOptionRosterLinkedQuestionViewModel(interviewRepository: interviewRepository, questionnaireStorage: questionnaireRepository);
            viewModel.Init(interview.Id.FormatGuid(), questionId, Create.Other.NavigationState());

            await viewModel.HandleAsync(
                Create.Event.RosterInstancesTitleChanged(rosterId: rosterId.Id, rosterTitle: "title", outerRosterVector: rosterId.RosterVector, instanceId: 1));

            Assert.That(viewModel.Options.Count, Is.EqualTo(1));
        }
    }
}

