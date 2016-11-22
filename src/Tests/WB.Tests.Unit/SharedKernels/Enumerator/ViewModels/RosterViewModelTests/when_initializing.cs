using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestFixture]
    internal class when_initializing : RosterViewModelTests
    {
        [Test]
        public void should_read_roster_instances_from_interview()
        {
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.GetRosterInstances(It.IsAny<Identity>(), rosterId))
                .Returns(new ReadOnlyCollection<Identity>(new List<Identity>
                {
                    Create.Entity.Identity(rosterId, Create.Entity.RosterVector(1)),
                    Create.Entity.Identity(rosterId, Create.Entity.RosterVector(2))
                }));

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview.Object);
            var viewModel = this.CreateViewModel(interviewRepository: interviewRepository);

            viewModel.Init("interviewId", Create.Entity.Identity(rosterId), Create.Other.NavigationState());

            Assert.That(viewModel.RosterInstances.Count(), Is.EqualTo(2));
        }

        [Test]
        public void should_read_roster_instances_ordered_like_options_in_multi_option_question()
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: question1Id, textAnswers: new []
                {
                    Create.Entity.Option("1", "Multi 1"),
                    Create.Entity.Option("2", "Multi 2"),
                    Create.Entity.Option("3", "Multi 3"),
                    Create.Entity.Option("4", "Multi 4")
                }),
                Create.Entity.Roster(rosterId: roster1Id, rosterSizeQuestionId: question1Id)
            });

            var plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, 0);

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: plainQuestionnaire);
            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), question1Id, RosterVector.Empty, DateTime.Now, new []{ 3, 1, 4, 2 });

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var viewModel = this.CreateViewModel(interviewRepository: interviewRepository);

            var navigationState = Create.Other.NavigationState(interviewRepository);
            navigationState.NavigateTo(NavigationIdentity.CreateForGroup(Create.Entity.Identity(chapterId)));

            viewModel.Init(interview.Id.FormatGuid(), Create.Entity.Identity(roster1Id), navigationState);

            Assert.That(viewModel.RosterInstances.Count(), Is.EqualTo(4));
        }

        static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid question1Id = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}