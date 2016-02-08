using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_roster_row_title_changed_and_question_is_linked_on_it : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionId = new Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), new decimal[] { 1 });
            rosterId = new Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), new decimal[] { 1 });

            var questionnaire = Create.QuestionnaireModel(new BaseQuestionModel[] {
                    new LinkedToRosterMultiOptionQuestionModel()
                    {
                        Id = questionId.Id,
                        LinkedToRosterId = rosterId.Id
                    }
                });

            interview = new Mock<IStatefulInterview>();
            interview.Setup(x => x.FindReferencedRostersForLinkedQuestion(rosterId.Id, Moq.It.IsAny<Identity>()))
                 .Returns(new [] { Create.InterviewRoster(rosterId.Id, new decimal[] { 1 }, "title") });

            viewModel = CreateMultiOptionRosterLinkedQuestionViewModel(questionnaire, interview.Object);
            viewModel.Init("interview", questionId, Create.NavigationState());
        };

        Because of = () =>
        {
          
            viewModel.Handle(Create.RosterInstancesTitleChanged(rosterId: rosterId.Id, rosterTitle: "title",
                outerRosterVector: new decimal[0], instanceId: 1));
        };

        It should_insert_new_option = () => viewModel.Options.Count.ShouldEqual(1);

        static MultiOptionRosterLinkedQuestionViewModel viewModel;
        static Identity questionId;
        static Identity rosterId;
        static Mock<IStatefulInterview> interview;
    }
}

