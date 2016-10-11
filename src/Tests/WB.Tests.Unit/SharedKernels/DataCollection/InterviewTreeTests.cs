using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(InterviewTree))]
    [TestFixture]
    public class InterviewTreeTests
    {
        [Test]
        public void When_Compare_and_changed_tree_has_2_nodes_which_dont_have_source_tree_Then_should_return_2_diff_nodes()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var sectionIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"));
            var addedQuestionIdentity = Create.Entity.Identity(Guid.Parse("33333333333333333333333333333333"));
            var addedRosterIdentity = Create.Entity.Identity(Guid.Parse("44444444444444444444444444444444"));
            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(sectionIdentity);
            var changedTreeMainSection = Create.Entity.InterviewTreeSection(sectionIdentity);

            changedTreeMainSection.AddChildren(Create.Entity.InterviewTreeQuestion(addedQuestionIdentity));
            changedTreeMainSection.AddChildren(Create.Entity.InterviewTreeRoster(addedRosterIdentity));

            var sourceTree = Create.Entity.InterviewTree(interviewId, sourceTreeMainSection);
            var changedTree = Create.Entity.InterviewTree(interviewId, changedTreeMainSection);
            //act
            var diff = sourceTree.Compare(changedTree).ToList();
            //assert
            Assert.That(diff.Count, Is.EqualTo(2));
            Assert.That(diff[0].SourceNode, Is.EqualTo(null));
            Assert.That(diff[1].SourceNode, Is.EqualTo(null));

            Assert.IsAssignableFrom<InterviewTreeQuestion>(diff[0].ChangedNode);
            Assert.IsAssignableFrom<InterviewTreeRoster>(diff[1].ChangedNode);
            Assert.That(diff[0].ChangedNode.Identity, Is.EqualTo(addedQuestionIdentity));
            Assert.That(diff[1].ChangedNode.Identity, Is.EqualTo(addedRosterIdentity));
        }

        [Test]
        public void When_Compare_and_source_tree_has_2_nodes_which_dont_have_changed_tree_Then_should_return_2_diff_nodes()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var sectionIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"));
            var addedQuestionIdentity = Create.Entity.Identity(Guid.Parse("33333333333333333333333333333333"));
            var addedRosterIdentity = Create.Entity.Identity(Guid.Parse("44444444444444444444444444444444"));
            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(sectionIdentity);
            var changedTreeMainSection = Create.Entity.InterviewTreeSection(sectionIdentity);

            sourceTreeMainSection.AddChildren(Create.Entity.InterviewTreeQuestion(addedQuestionIdentity));
            sourceTreeMainSection.AddChildren(Create.Entity.InterviewTreeRoster(addedRosterIdentity));

            var sourceTree = Create.Entity.InterviewTree(interviewId, sourceTreeMainSection);
            var changedTree = Create.Entity.InterviewTree(interviewId, changedTreeMainSection);
            //act
            var diff = sourceTree.Compare(changedTree).ToList();
            //assert
            Assert.That(diff.Count, Is.EqualTo(2));
            Assert.That(diff[0].ChangedNode, Is.EqualTo(null));
            Assert.That(diff[1].ChangedNode, Is.EqualTo(null));

            Assert.IsAssignableFrom<InterviewTreeQuestion>(diff[0].SourceNode);
            Assert.IsAssignableFrom<InterviewTreeRoster>(diff[1].SourceNode);
            Assert.That(diff[0].SourceNode.Identity, Is.EqualTo(addedQuestionIdentity));
            Assert.That(diff[1].SourceNode.Identity, Is.EqualTo(addedRosterIdentity));
        }

        [Test]
        public void When_Compare_on_same_trees_Then_should_return_no_diffs()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var sectionIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"), RosterVector.Empty);
            var questionIdentity = Create.Entity.Identity(Guid.Parse("33333333333333333333333333333333"), RosterVector.Empty);

            var sourceTree = CreateSimpleTree(interviewId, sectionIdentity, questionIdentity);
            var targetTree = CreateSimpleTree(interviewId, sectionIdentity, questionIdentity);

            //act
            var diff = sourceTree.Compare(targetTree).ToList();

            //assert
            Assert.That(diff.Count, Is.EqualTo(0));
        }

        [Test]
        public void When_Compare_and_answer_changed_Then_should_return_1_diff_node_with_that_question()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var sectionIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"), RosterVector.Empty);
            var questionIdentity = Create.Entity.Identity(Guid.Parse("33333333333333333333333333333333"), RosterVector.Empty);

            var question1 = Create.Entity.InterviewTreeQuestion_SingleOption(questionIdentity, answer: 1);
            var question2 = Create.Entity.InterviewTreeQuestion_SingleOption(questionIdentity, answer: 2);

            var sourceTree = CreateSimpleTree(interviewId, sectionIdentity, question1);
            var targetTree = CreateSimpleTree(interviewId, sectionIdentity, question2);

            //act
            var diff = sourceTree.Compare(targetTree).ToList();

            //assert
            Assert.That(diff.Count, Is.EqualTo(1));
            Assert.That(diff[0].SourceNode, Is.EqualTo(question1));
            Assert.That(diff[0].ChangedNode, Is.EqualTo(question2));
        }

        [Test]
        public void When_Compare_and_section_disabled_Then_should_return_1_diff_node_with_that_section()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var sectionIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"), RosterVector.Empty);

            var section1 = Create.Entity.InterviewTreeSection(sectionIdentity, isDisabled: false);
            var section2 = Create.Entity.InterviewTreeSection(sectionIdentity, isDisabled: true);

            var sourceTree = CreateSimpleTree(interviewId, section1);
            var targetTree = CreateSimpleTree(interviewId, section2);

            //act
            var diff = sourceTree.Compare(targetTree).ToList();

            //assert
            Assert.That(diff.Count, Is.EqualTo(1));
            Assert.That(diff[0].SourceNode, Is.EqualTo(section1));
            Assert.That(diff[0].ChangedNode, Is.EqualTo(section2));
        }

        private static InterviewTree CreateSimpleTree(Guid interviewId, Identity sectionIdentity, Identity questionIdentity)
            => CreateSimpleTree(interviewId, sectionIdentity, Create.Entity.InterviewTreeQuestion(questionIdentity));

        private static InterviewTree CreateSimpleTree(Guid interviewId, Identity sectionIdentity, InterviewTreeQuestion question)
            => CreateSimpleTree(interviewId, Create.Entity.InterviewTreeSection(sectionIdentity, children: question));

        private static InterviewTree CreateSimpleTree(Guid interviewId, InterviewTreeSection section)
            => new InterviewTree(interviewId, new[] { section });
    }
}