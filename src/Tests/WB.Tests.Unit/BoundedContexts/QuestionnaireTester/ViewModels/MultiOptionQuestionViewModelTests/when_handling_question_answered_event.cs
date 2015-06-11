using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.MultiOptionQuestionViewModelTests
{
    public class when_handling_question_answered_event : MultiOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = new Identity(questionGuid, Empty.RosterVector);

            var questionnaire = BuildDefaultQuestionnaire(questionId);
            ((MultiOptionQuestionModel) questionnaire.Questions.First().Value).AreAnswersOrdered = true;

            var multiOptionAnswer = new MultiOptionAnswer(questionGuid, Empty.RosterVector);
            multiOptionAnswer.SetAnswers(new[] { 2m });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetMultiOptionAnswer(questionId) == multiOptionAnswer);

            var questionnaireStorage = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object,
                interviewRepository: interviewRepository.Object);

            viewModel.Init("blah", questionId, new NavigationState());
            viewModel.Options.First().Checked = true;
        };

        Because of = () => viewModel.Handle(new MultipleOptionsQuestionAnswered(Guid.NewGuid(), questionGuid, Empty.RosterVector, DateTime.Now, new []{2m, 1m}));

        It should_set_checked_order_to_options = () => viewModel.Options.First().CheckedOrder.ShouldEqual(2);

        static MultiOptionQuestionViewModel viewModel;
        static Identity questionId;
        private static Guid questionGuid;
    }
}

