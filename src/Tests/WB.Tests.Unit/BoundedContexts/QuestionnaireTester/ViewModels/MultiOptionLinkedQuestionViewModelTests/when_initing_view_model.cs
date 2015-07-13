using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Questions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    public class when_initing_view_model : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewId = "interview";
            questionId = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
            Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var interview = Mock.Of<IStatefulInterview>(x =>
                x.FindAnswersOfReferencedQuestionForLinkedQuestion(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Identity>()) == new[]
                {
                    Create.TextAnswer("answer1", linkedToQuestionId, new []{1m}),
                    Create.TextAnswer("answer2", linkedToQuestionId, new []{2m})
                } &&
                x.Answers == new Dictionary<string, BaseInterviewAnswer>()
                );

            var questionnaire = Create.QuestionnaireModel();
            questionnaire.Questions = new Dictionary<Guid, BaseQuestionModel>();
            questionnaire.Questions.Add(questionId.Id, new LinkedMultiOptionQuestionModel
            {
                LinkedToQuestionId = linkedToQuestionId
            });
            questionnaire.Questions.Add(linkedToQuestionId, new TextQuestionModel());


            var interviews = new Mock<IStatefulInterviewRepository>();
            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();

            interviews.SetReturnsDefault(interview);
            questionnaires.SetReturnsDefault(questionnaire);

            viewModel = CreateViewModel(interviewRepository: interviews.Object, questionnaireStorage: questionnaires.Object);
        };

        Because of = () => viewModel.Init(interviewId, questionId, Create.NavigationState());

        It should_fill_options_from_linked_question = () => viewModel.Options.Count.ShouldEqual(2);

        It should_add_linked_question_roster_vectors_as_values_for_answers = () => viewModel.Options.First().Value.ShouldContainOnly(1m);

        It should_use_question_answer_as_title = () => viewModel.Options.Second().Title.ShouldEqual("answer2");

        static MultiOptionLinkedQuestionViewModel viewModel;
        static string interviewId;
        static Identity questionId;
    }
}

