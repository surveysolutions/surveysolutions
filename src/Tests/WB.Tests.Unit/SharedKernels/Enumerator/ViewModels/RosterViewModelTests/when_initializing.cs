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
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

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
        [Ignore("KP-8236")]
        public void should_read_roster_instances_ordered_like_options_in_multi_option_question_if_trigger_is_not_ordered()
        {
            // arrange
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: question1Id, areAnswersOrdered: false, textAnswers: new []
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
            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), question1Id, RosterVector.Empty, DateTime.Now, new[] { 3, 4 });
            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), question1Id, RosterVector.Empty, DateTime.Now, new []{ 3, 1, 4, 2 });

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var viewModel = this.CreateViewModel(interviewRepository: interviewRepository);

            var navigationState = Create.Other.NavigationState(interviewRepository);
            navigationState.NavigateTo(NavigationIdentity.CreateForGroup(Create.Entity.Identity(chapterId)));

            // act
            viewModel.Init(interview.Id.FormatGuid(), Create.Entity.Identity(roster1Id), navigationState);

            var rosterModel = viewModel.RosterInstances.Cast<GroupViewModel>().ToList();
            List<decimal> rosterIds = rosterModel.Select(x => x.Identity.RosterVector.Last()).ToList();

            // assert
            Assert.That(rosterModel.Count, Is.EqualTo(4));
            Assert.AreEqual(new decimal[] {1, 2, 3, 4}, rosterIds);
        }

        [Test]
        [Ignore("KP-8236")]
        public void should_read_roster_instances_ordered_like_options_in_yes_no_question_if_trigger_is_not_ordered()
        {
            // arrange
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: question1Id, isYesNo: true, areAnswersOrdered: false, textAnswers: new []
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
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(Guid.NewGuid(), question1Id, RosterVector.Empty, new []
            {
                Create.Entity.AnsweredYesNoOption(3m, true),
                Create.Entity.AnsweredYesNoOption(4m, true),
            }, DateTime.Now));
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(Guid.NewGuid(), question1Id, RosterVector.Empty, new[]
            {
                Create.Entity.AnsweredYesNoOption(3m, true),
                Create.Entity.AnsweredYesNoOption(1m, true),
                Create.Entity.AnsweredYesNoOption(4m, true),
                Create.Entity.AnsweredYesNoOption(2m, true),
            }, DateTime.Now));

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var viewModel = this.CreateViewModel(interviewRepository: interviewRepository);

            var navigationState = Create.Other.NavigationState(interviewRepository);
            navigationState.NavigateTo(NavigationIdentity.CreateForGroup(Create.Entity.Identity(chapterId)));

            // act
            viewModel.Init(interview.Id.FormatGuid(), Create.Entity.Identity(roster1Id), navigationState);

            var rosterModel = viewModel.RosterInstances.Cast<GroupViewModel>().ToList();
            List<decimal> rosterIds = rosterModel.Select(x => x.Identity.RosterVector.Last()).ToList();

            // assert
            Assert.That(rosterModel.Count, Is.EqualTo(4));
            Assert.AreEqual(new decimal[] { 1, 2, 3, 4 }, rosterIds);
        }

        [Test]
        public void should_read_roster_instances_with_the_same_order_as_user_checked_them_if_multi_size_question_is_ordered()
        {
            // arrange
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: question1Id, areAnswersOrdered: true, textAnswers: new []
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
            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), question1Id, RosterVector.Empty, DateTime.Now, new[] { 3, 4 });
            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), question1Id, RosterVector.Empty, DateTime.Now, new[] { 3, 1, 4, 2 });

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var viewModel = this.CreateViewModel(interviewRepository: interviewRepository);

            var navigationState = Create.Other.NavigationState(interviewRepository);
            navigationState.NavigateTo(NavigationIdentity.CreateForGroup(Create.Entity.Identity(chapterId)));

            // act
            viewModel.Init(interview.Id.FormatGuid(), Create.Entity.Identity(roster1Id), navigationState);

            var rosterModel = viewModel.RosterInstances.Cast<GroupViewModel>().ToList();
            List<decimal> rosterIds = rosterModel.Select(x => x.Identity.RosterVector.Last()).ToList();

            // assert
            Assert.That(rosterModel.Count, Is.EqualTo(4));
            Assert.AreEqual(new decimal[] { 3, 1, 4, 2 }, rosterIds);
        }

        [Test]
        public void should_read_roster_instances_with_the_same_order_as_user_checked_them_if_yes_no_size_question_is_ordered()
        {
            // arrange
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: question1Id, isYesNo: true, areAnswersOrdered: true, textAnswers: new []
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
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(Guid.NewGuid(), question1Id, RosterVector.Empty, new[]
            {
                Create.Entity.AnsweredYesNoOption(3m, true),
                Create.Entity.AnsweredYesNoOption(4m, true),
            }, DateTime.Now));
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(Guid.NewGuid(), question1Id, RosterVector.Empty, new[]
            {
                Create.Entity.AnsweredYesNoOption(3m, true),
                Create.Entity.AnsweredYesNoOption(1m, true),
                Create.Entity.AnsweredYesNoOption(4m, true),
                Create.Entity.AnsweredYesNoOption(2m, true),
            }, DateTime.Now));

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);
            
            var viewModel = this.CreateViewModel(interviewRepository: interviewRepository);

            var navigationState = Create.Other.NavigationState(interviewRepository);
            navigationState.NavigateTo(NavigationIdentity.CreateForGroup(Create.Entity.Identity(chapterId)));

            // act
            viewModel.Init(interview.Id.FormatGuid(), Create.Entity.Identity(roster1Id), navigationState);

            var rosterModel = viewModel.RosterInstances.Cast<GroupViewModel>().ToList();
            List<decimal> rosterIds = rosterModel.Select(x => x.Identity.RosterVector.Last()).ToList();

            // assert
            Assert.That(rosterModel.Count, Is.EqualTo(4));
            Assert.AreEqual(new decimal[] { 3, 1, 4, 2 }, rosterIds);
        }

        static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid question1Id = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}