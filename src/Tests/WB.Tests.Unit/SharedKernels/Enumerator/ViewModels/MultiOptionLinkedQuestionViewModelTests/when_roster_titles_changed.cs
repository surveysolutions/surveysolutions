﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Nito.AsyncEx.Synchronous;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_roster_titles_changed : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var level1TriggerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var level2triggerId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var linkToQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var linkedQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            topRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var questionIdentity = Create.Other.Identity(linkedQuestionId, RosterVector.Empty);

            var questionnaire = Create.Other.QuestionnaireDocumentWithOneChapter(
                Create.Other.TextListQuestion(questionId: level1TriggerId),
                Create.Other.Roster(rosterSizeQuestionId: level1TriggerId, rosterId: topRosterId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                {
                    Create.Other.TextListQuestion(questionId: level2triggerId),
                    Create.Other.Roster(rosterSizeQuestionId: level2triggerId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.Other.TextQuestion(questionId: linkToQuestionId)
                    })
                }),
                Create.Other.MultipleOptionsQuestion(questionId: linkedQuestionId, linkedToQuestionId: linkToQuestionId)
                );

            var interview = Substitute.For<IStatefulInterview>();
            interview.GetParentRosterTitlesWithoutLast(linkToQuestionId, Create.Other.RosterVector(1, 1))
                .Returns(new List<string> { "title" });
            interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkToQuestionId, questionIdentity)
                .Returns(new[] { Create.Other.TextAnswer("subtitle", linkToQuestionId, Create.Other.RosterVector(1, 1)) });

            viewModel = CreateViewModel(Create.Other.PlainQuestionnaire(questionnaire), interview);
            viewModel.Init(interview.Id.FormatGuid(), questionIdentity, Create.Other.NavigationState());
        };

        Because of = () => viewModel.Handle(Create.Other.RosterInstancesTitleChanged(rosterId: topRosterId));

        It should_refresh_list_of_options = () => viewModel.Options.Count.ShouldEqual(1);

        It should_prefix_option_with_parent_title = () => viewModel.Options.First().Title.ShouldEqual("title: subtitle");

        static MultiOptionLinkedToQuestionQuestionViewModel viewModel;
        static Guid topRosterId;
    }
}