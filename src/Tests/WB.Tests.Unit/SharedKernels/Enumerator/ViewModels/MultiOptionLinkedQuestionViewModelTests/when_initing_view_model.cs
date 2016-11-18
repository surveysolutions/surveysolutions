using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    [Ignore("KP-8159")]
    internal class when_initing_view_model : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewId = "interview";
            questionId = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
            Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var interview = Mock.Of<IStatefulInterview>();
            //x =>
            //    x.FindAnswersOfReferencedQuestionForLinkedQuestion(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Identity>()) == new[]
            //    {
            //        Create.Entity.InterviewTreeTextQuestion("answer1", linkedToQuestionId, new []{1m}),
            //        Create.Entity.InterviewTreeTextQuestion("answer2", linkedToQuestionId, new []{2m})
            //    } &&
            //    x.Answers == new Dictionary<string, BaseInterviewAnswer>()
            //    );

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.GetQuestionReferencedByLinkedQuestion(questionId.Id) == linkedToQuestionId
                && _.ShouldQuestionRecordAnswersOrder(questionId.Id) == false);

            var interviews = new Mock<IStatefulInterviewRepository>();
            var questionnaires = new Mock<IQuestionnaireStorage>();

            interviews.SetReturnsDefault(interview);
            questionnaires.SetReturnsDefault(questionnaire);

            questionViewModel = CreateViewModel(interviewRepository: interviews.Object, questionnaireStorage: questionnaires.Object);
        };

        Because of = () => questionViewModel.Init(interviewId, questionId, Create.Other.NavigationState());

        It should_fill_options_from_linked_question = () => questionViewModel.Options.Count.ShouldEqual(2);

        It should_add_linked_question_roster_vectors_as_values_for_answers = () => questionViewModel.Options.First().Value.ShouldContainOnly(1m);

        It should_use_question_answer_as_title = () => questionViewModel.Options.Second().Title.ShouldEqual("answer2");

        static MultiOptionLinkedToRosterQuestionQuestionViewModel questionViewModel;
        static string interviewId;
        static Identity questionId;
    }
}

