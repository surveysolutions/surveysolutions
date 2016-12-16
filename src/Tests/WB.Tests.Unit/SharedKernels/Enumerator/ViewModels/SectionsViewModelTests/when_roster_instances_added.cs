using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    internal class when_roster_instances_added : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            interviewerId = Guid.Parse("22222222222222222222222222222222");
            textListQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var chapterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.Group(group1Id),
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.Roster(rosterId, rosterSizeQuestionId: textListQuestionId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion()
                }),
                Create.Entity.Group(group2Id));

            interview = Setup.StatefulInterview(questionnaire);
            interview.AnswerTextListQuestion(interviewerId, textListQuestionId, RosterVector.Empty, DateTime.Now,
                new[] {Tuple.Create(0m, "zero"), Tuple.Create(1m, "one")});

            viewModel = CreateSectionsViewModel(Create.Entity.PlainQuestionnaire(questionnaire), interview);

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var navigationState = Create.Other.NavigationState();
            navigationState.Init(interview.Id.FormatGuid(), questionnaireIdentity.ToString());
            
            viewModel.Init(interview.Id.FormatGuid(), questionnaireIdentity, navigationState);
            viewModel.Sections[0].SectionIdentity = new Identity(chapterId, Empty.RosterVector);
            viewModel.Sections[0].Expanded = true;
        };

        Because of = () =>
        {
            interview.AnswerTextListQuestion(interviewerId, textListQuestionId, RosterVector.Empty, DateTime.Now,
               new[] { Tuple.Create(0m, "zero"), Tuple.Create(1m, "one"), Tuple.Create(2m, "two")});
            viewModel.Handle(Create.Event.RosterInstancesAdded(rosterId, Create.Entity.RosterVector(2)));
        };

        It should_add_roster_into_a_tree = () => 
            viewModel.Sections[0].Children.Count.ShouldEqual(4);

        static SideBarSectionsViewModel viewModel;
        static StatefulInterview interview;
        private static Guid rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid group1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid group2Id = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewerId;
        private static Guid textListQuestionId;
    }
}

