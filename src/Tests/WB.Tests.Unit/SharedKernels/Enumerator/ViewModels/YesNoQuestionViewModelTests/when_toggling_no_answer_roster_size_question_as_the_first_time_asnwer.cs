using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_toggling_no_answer_roster_size_question_as_the_first_time_asnwer : YesNoQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewIdAsString = "hello";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = Create.QuestionnaireModel(new BaseQuestionModel[] {
                Create.YesNoQuestionModel(id : questionId.Id, options: new List<OptionModel>
                {
                    Create.OptionModel("item1", 1),
                    Create.OptionModel( "item2", 2),
                    Create.OptionModel("item3",  3),
                    Create.OptionModel("item4", 4),
                    Create.OptionModel("item5", 5),
                })
            });

            ((YesNoQuestionModel)questionnaire.Questions.First().Value).IsRosterSizeQuestion = true;

            var yesNoAnswer = Create.YesNoAnswer(questionGuid, Empty.RosterVector);
           
            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoAnswer(questionId) == yesNoAnswer);

            var questionnaireStorage = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.Setup(x => x.Get(interviewIdAsString)).Returns(interview);

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object, interviewRepository: interviewRepository.Object);

            viewModel.Init(interviewIdAsString, questionId, Create.NavigationState());
            viewModel.Options.First().Selected = false;
        };

        Because of = () => exception = Catch.Exception(
               () => viewModel.ToggleAnswerAsync(viewModel.Options.First()).WaitAndUnwrapException());

        It should_get_interview_by_given_string_key_in_Init_and_ToggleAnswerAsync_methods = () =>
            interviewRepository.Verify(x => x.Get(interviewIdAsString), Times.AtLeast(2));

        It should_check_no_option = () =>
            viewModel.Options.First().NoSelected.ShouldBeTrue();

        private static Exception exception;
        static YesNoQuestionViewModel viewModel;
        static Identity questionId;
        private static Guid questionGuid;
        private static Mock<IStatefulInterviewRepository> interviewRepository;
        private static string interviewIdAsString;
    }
}