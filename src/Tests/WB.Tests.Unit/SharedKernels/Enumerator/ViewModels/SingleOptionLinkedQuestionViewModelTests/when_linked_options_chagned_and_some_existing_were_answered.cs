﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_linked_options_chagned_and_some_existing_were_answered : SingleOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            linkSourceQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            linkedQuestionId = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC").FormatGuid();
            interviewerId = Guid.Parse("77777777777777777777777777777777");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(linkedQuestionId.Id, linkedToQuestionId: linkSourceQuestionId),
                Create.Entity.FixedRoster(fixedTitles: new[] { Create.Entity.FixedTitle(1), Create.Entity.FixedTitle(2), Create.Entity.FixedTitle(3) }, children: new []
                {
                    Create.Entity.TextQuestion(linkSourceQuestionId)
                }));

            interview = Setup.StatefulInterview(questionnaire);

            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(2), DateTime.UtcNow, "two");

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(interview: interview, questionnaire: Create.Entity.PlainQuestionnaire(questionnaire));
            viewModel.Init(interviewId, linkedQuestionId, Create.Other.NavigationState());
            viewModel.Options.First().Selected = true;

            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "one");
            interview.AnswerTextQuestion(interviewerId, linkSourceQuestionId, Create.Entity.RosterVector(3), DateTime.UtcNow, "three");
            interview.AnswerSingleOptionLinkedQuestion(interviewerId, linkedQuestionId.Id, linkedQuestionId.RosterVector, DateTime.UtcNow, Create.Entity.RosterVector(2));
        };

        Because of = () =>
        {
            viewModel.Handle(Create.Event.LinkedOptionsChanged(new[]
            {
                new ChangedLinkedOptions(linkedQuestionId, new[]
                {
                    Create.Entity.RosterVector(1), Create.Entity.RosterVector(2), Create.Entity.RosterVector(3)
                }),
            }));
        };

        It should_not_remove_existing_selected_option = () => viewModel.Options.Second().Selected.ShouldBeTrue();

        It should_add_new_options_as_non_selected = () => viewModel.Options.First().Selected.ShouldBeFalse();
        
        It should_add_all_options_from_event = () => viewModel.Options.Count.ShouldEqual(3);

        static SingleOptionLinkedQuestionViewModel viewModel;
        static Guid linkSourceQuestionId;
        static Identity linkedQuestionId;
        static string interviewId;
        static StatefulInterview interview;
        static Guid interviewerId;
    }
}