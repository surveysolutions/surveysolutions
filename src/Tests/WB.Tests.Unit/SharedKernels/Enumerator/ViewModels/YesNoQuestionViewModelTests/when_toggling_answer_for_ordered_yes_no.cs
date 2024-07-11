using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_toggling_answer_for_ordered_yes_no : YesNoQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public void context () {
            questionId = Create.Entity.Identity(Id.g1, Empty.RosterVector);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g8, Id.g9, Create.Entity.YesNoQuestion(Id.g1, new []{ 1, 2, 3, 4}, true));

            var interview = Create.AggregateRoot.StatefulInterview(Id.gA, Id.g9, Id.gB, Id.gC, questionnaire);
            
            var questionnaireStorage = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var interviewRepository = SetUp.StatefulInterviewRepository(interview);
          
            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
                Create.Entity.CategoricalQuestionOption(3, "item3"),
                Create.Entity.CategoricalQuestionOption(4, "item4"),
            });
            
            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            viewModel.Init(Id.gA.FormatGuid(), questionId, Create.Other.NavigationState());

            CheckYesOption(0);
            CheckYesOption(1);
            CheckYesOption(2);
            CheckYesOption(3);

            CheckNoOption(0);
            CheckNoOption(1);
            CheckYesOption(0);
            CheckYesOption(1);
        }

        private static void CheckYesOption(int index)
        {
            viewModel.Options[index].Checked = true;
            viewModel.Options[index].CheckAnswerCommand.Execute();
        }

        private static void CheckNoOption(int index)
        {
            (viewModel.Options[index] as CategoricalYesNoOptionViewModel).NoSelected = true;
            (viewModel.Options[index] as CategoricalYesNoOptionViewModel).SetNoAnswerCommand.Execute();
        }

        [Test] public void should_send_answers_to_command_service ()
        {
            int[] orders = viewModel.Options.OrderBy(x => x.Value).Select(x => x.CheckedOrder ?? 0).ToArray();
            CollectionAssert.AreEqual(new int []{ 3, 4, 1, 2}, orders);
        }

        static CategoricalYesNoViewModel viewModel;
        static Identity questionId;
    }
}
