using System;
using System.Linq;
using System.Reflection;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;

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

            changedTreeMainSection.AddChild(Create.Entity.InterviewTreeQuestion(addedQuestionIdentity));
            changedTreeMainSection.AddChild(Create.Entity.InterviewTreeRoster(addedRosterIdentity));

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

            sourceTreeMainSection.AddChild(Create.Entity.InterviewTreeQuestion(addedQuestionIdentity));
            sourceTreeMainSection.AddChild(Create.Entity.InterviewTreeRoster(addedRosterIdentity));

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


        [Test]
        public void When_Clone_tree_has_alot_of_nodes_Then_should_return_copy_of_source_tree_with_dif_references()
        {
            //arrange
            var sourceTree = CreateTreeForClone();

            //act
            var clonedTree = sourceTree.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedTree, sourceTree), Is.False);
            Assert.That(ReferenceEquals(clonedTree.Sections, sourceTree.Sections), Is.False);
            Assert.That(ReferenceEquals(clonedTree.Sections.First(), sourceTree.Sections.First()), Is.False);
            Assert.That(ReferenceEquals(clonedTree.Sections.First().Children.First(), sourceTree.Sections.First().Children.First()), Is.False);
            Assert.That(ReferenceEquals(clonedTree.Sections.First().Children.Second(), sourceTree.Sections.First().Children.Second()), Is.False);

            var sourceInterviewTreeNodes = sourceTree.Sections.Cast<IInterviewTreeNode>().TreeToEnumerable(s => s.Children).ToList();
            var clonedInterviewTreeNodes = clonedTree.Sections.Cast<IInterviewTreeNode>().TreeToEnumerable(s => s.Children).ToList();
            foreach (var pair in Enumerable.Zip(sourceInterviewTreeNodes, clonedInterviewTreeNodes, (s, c) => new { SourceNode = s, ClonedNode = c }))
            {
                Assert.That(ReferenceEquals(pair.SourceNode, pair.ClonedNode), Is.False);
            }
        }

        [Test]
        public void When_Clone_tree_has_alot_of_nodes_Then_should_return_copy_of_source_tree_with_correct_identity()
        {
            //arrange
            var sourceTree = CreateTreeForClone();

            //act
            var clonedTree = sourceTree.Clone();

            //assert
            Assert.AreEqual(clonedTree.InterviewId, sourceTree.InterviewId);
            Assert.AreEqual(clonedTree.Sections.First().Identity, sourceTree.Sections.First().Identity);
            Assert.AreEqual(clonedTree.Sections.First().Children.First().Identity, sourceTree.Sections.First().Children.First().Identity);
            Assert.AreEqual(clonedTree.Sections.First().Children.Second().Identity, sourceTree.Sections.First().Children.Second().Identity);

            var sourceInterviewTreeNodes = sourceTree.Sections.Cast<IInterviewTreeNode>().TreeToEnumerable(s => s.Children).ToList();
            var clonedInterviewTreeNodes = clonedTree.Sections.Cast<IInterviewTreeNode>().TreeToEnumerable(s => s.Children).ToList();
            foreach (var pair in Enumerable.Zip(sourceInterviewTreeNodes, clonedInterviewTreeNodes, (s, c) => new { SourceNode = s, ClonedNode = c }))
            {
                Assert.AreEqual(pair.SourceNode.Identity, pair.ClonedNode.Identity);
            }
        }

        [Test]
        public void When_Clone_tree_has_alot_of_nodes_Then_should_return_copy_of_source_tree_with_parent_property_to_new_nodes()
        {
            //arrange
            var sourceTree = CreateTreeForClone();

            //act
            var clonedTree = sourceTree.Clone();

            //assert
            clonedTree.Sections.Cast<IInterviewTreeNode>().Single().ForEachTreeElement(s => s.Children,
                (parent, children) => Assert.AreEqual(children.Parent, parent));
        }

        [Test]
        public void When_Clone_tree_has_alot_of_nodes_Then_should_return_copy_of_source_tree_with_correct_tree_ref_to_new_tree()
        {
            //arrange
            var sourceTree = CreateTreeForClone();

            //act
            var clonedTree = sourceTree.Clone();

            //assert
            var clonedInterviewTreeNodes = clonedTree.Sections.Cast<IInterviewTreeNode>().Single().TreeToEnumerable(s => s.Children).ToList();
            clonedInterviewTreeNodes.ForEach(n =>
            {
                if (n is InterviewTreeQuestion)
                    Assert.AreEqual(((InterviewTreeQuestion)n).Tree, clonedTree);
                if (n is InterviewTreeGroup)
                    Assert.AreEqual(((InterviewTreeGroup)n).Tree, clonedTree);
            });
        }

        [Test]
        public void When_Clone_text_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true, 
                title: "title",
                variableName: "variable",
                questionType: QuestionType.Text, 
                answer: "answer");

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsText);
            Assert.AreEqual(clonedQuestion.AsText.GetAnswer(), question.AsText.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsText, question.AsText), Is.False);
        }

        [Test]
        public void When_Clone_integer_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true, 
                title: "title",
                variableName: "variable",
                questionType: QuestionType.Numeric, 
                answer: 10);

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsInteger);
            Assert.AreEqual(clonedQuestion.AsInteger.GetAnswer(), question.AsInteger.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsInteger, question.AsInteger), Is.False);
        }

        [Test]
        public void When_Clone_double_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true, 
                title: "title",
                variableName: "variable",
                questionType: QuestionType.Numeric, 
                isDecimal: true,
                answer: 10.1);

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsDouble);
            Assert.AreEqual(clonedQuestion.AsDouble.GetAnswer(), question.AsDouble.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsDouble, question.AsDouble), Is.False);
        }

        [Test]
        public void When_Clone_textlist_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true, 
                title: "title",
                variableName: "variable",
                questionType: QuestionType.TextList, 
                answer: new[] { new Tuple<decimal, string>(1, "1") });

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsTextList);
            Assert.AreEqual(clonedQuestion.AsTextList.GetAnswer(), question.AsTextList.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsTextList, question.AsTextList), Is.False);
        }

        [Test]
        public void When_Clone_DateTime_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true, 
                title: "title",
                variableName: "variable",
                questionType: QuestionType.DateTime, 
                answer: DateTime.UtcNow);

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsDateTime);
            Assert.AreEqual(clonedQuestion.AsDateTime.GetAnswer(), question.AsDateTime.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsDateTime, question.AsDateTime), Is.False);
        }

        [Test]
        public void When_Clone_GpsCoordinates_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true, 
                title: "title",
                variableName: "variable",
                questionType: QuestionType.GpsCoordinates, 
                answer: new GeoPosition(1, 2, 3, 4, DateTimeOffset.Now));

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsGps);
            Assert.AreEqual(clonedQuestion.AsGps.GetAnswer(), question.AsGps.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsGps, question.AsGps), Is.False);
        }

        [Test]
        public void When_Clone_Multimedia_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true, 
                title: "title",
                variableName: "variable",
                questionType: QuestionType.Multimedia, 
                answer: "pic.jpg");

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsMultimedia);
            Assert.AreEqual(clonedQuestion.AsMultimedia.GetAnswer(), question.AsMultimedia.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsMultimedia, question.AsMultimedia), Is.False);
        }

        [Test]
        public void When_Clone_MultiOption_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true, 
                title: "title",
                variableName: "variable",
                questionType: QuestionType.MultyOption, 
                answer: new decimal[] { 5, 7 });

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsMultiFixedOption);
            Assert.AreEqual(clonedQuestion.AsMultiFixedOption.GetAnswer(), question.AsMultiFixedOption.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsMultiFixedOption, question.AsMultiFixedOption), Is.False);
        }

        [Test]
        public void When_Clone_yes_no_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true,
                title: "title",
                variableName: "variable",
                questionType: QuestionType.MultyOption,
                answer: new AnsweredYesNoOption[] { new AnsweredYesNoOption(1, true), new AnsweredYesNoOption(4, false) },
                isYesNo: true);

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsYesNo);
            Assert.AreEqual(clonedQuestion.AsYesNo.GetAnswer(), question.AsYesNo.GetAnswer());
            Assert.AreEqual(clonedQuestion.AsYesNo.GetAnswer().CheckedOptions.First(), question.AsYesNo.GetAnswer().CheckedOptions.First());
            Assert.AreEqual(clonedQuestion.AsYesNo.GetAnswer().CheckedOptions.Second(), question.AsYesNo.GetAnswer().CheckedOptions.Second());
            Assert.That(ReferenceEquals(clonedQuestion.AsYesNo, question.AsYesNo), Is.False);
        }

        [Test]
        public void When_Clone_linked_MultiOption_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true,
                title: "title",
                variableName: "variable",
                questionType: QuestionType.MultyOption,
                answer: new decimal[][]{ new decimal[]{ 1.1m, 1.2m}, new decimal[]{2.1m, 2.2m} },
                linkedSourceId: Guid.NewGuid(),
                linkedOptions: new []{ Create.Entity.RosterVector(1, 1), Create.Entity.RosterVector(1, 2), });

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsMultiLinkedOption);
            Assert.AreEqual(clonedQuestion.AsMultiLinkedOption.GetAnswer(), question.AsMultiLinkedOption.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsMultiLinkedOption, question.AsMultiLinkedOption), Is.False);
            Assert.That(ReferenceEquals(clonedQuestion.AsMultiLinkedOption.Options, question.AsMultiLinkedOption.Options), Is.False);
        }

        [Test]
        public void When_Clone_SingleOption_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true, 
                title: "title",
                variableName: "variable",
                questionType: QuestionType.SingleOption, 
                answer: 5.7);

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsSingleFixedOption);
            Assert.AreEqual(clonedQuestion.AsSingleFixedOption.GetAnswer(), question.AsSingleFixedOption.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsSingleFixedOption, question.AsSingleFixedOption), Is.False);
        }

        [Test]
        public void When_Clone_linked_SingleOption_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true, 
                title: "title",
                variableName: "variable",
                questionType: QuestionType.SingleOption, 
                answer: Create.Entity.RosterVector(1, 1),
                linkedSourceId: Guid.NewGuid(),
                linkedOptions: new[] { Create.Entity.RosterVector(1, 1), Create.Entity.RosterVector(1, 2), });

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsSingleLinkedOption);
            Assert.AreEqual(clonedQuestion.AsSingleLinkedOption.GetAnswer(), question.AsSingleLinkedOption.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsSingleLinkedOption, question.AsSingleLinkedOption), Is.False);
            Assert.That(ReferenceEquals(clonedQuestion.AsSingleLinkedOption.Options, question.AsSingleLinkedOption.Options), Is.False);
        }
        [Test]
        public void When_Clone_QRBarcode_question_Then_should_return_copy_of_question_with_dif_references()
        {
            //arrange
            var question = Create.Entity.InterviewTreeQuestion(
                questionIdentity: Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111")),
                isDisabled: true, 
                title: "title",
                variableName: "variable",
                questionType: QuestionType.QRBarcode, 
                answer: "bar code");

            //act
            var clonedQuestion = (InterviewTreeQuestion)question.Clone();

            //assert
            Assert.That(ReferenceEquals(clonedQuestion, question), Is.False);
            Assert.AreEqual(clonedQuestion.Identity, question.Identity);
            Assert.NotNull(clonedQuestion.Title);
            Assert.AreEqual(clonedQuestion.Title.Text, question.Title.Text);
            Assert.AreEqual(clonedQuestion.VariableName, question.VariableName);
            Assert.AreEqual(clonedQuestion.IsDisabled(), question.IsDisabled());
            Assert.NotNull(clonedQuestion.AsQRBarcode);
            Assert.AreEqual(clonedQuestion.AsQRBarcode.GetAnswer(), question.AsQRBarcode.GetAnswer());
            Assert.That(ReferenceEquals(clonedQuestion.AsQRBarcode, question.AsQRBarcode), Is.False);
        }

        [Test]
        public void When_Clone_tree_has_alot_of_nodes_Then_should_return_copy_of_source_tree_with_dif_references_for_all_tree()
        {
            //arrange
            var sourceTree = CreateTreeForClone();

            //act
            var clonedTree = sourceTree.Clone();

            //assert
            Type[] ignoreTypes = new[] {typeof(Identity), typeof(RosterVector), typeof(string), typeof(ISubstitutionService), typeof(IVariableToUIStringService) };
            var sourceInterviewTreeNodes = sourceTree.Sections.Cast<IInterviewTreeNode>().TreeToEnumerable(s => s.Children).ToList();
            var clonedInterviewTreeNodes = clonedTree.Sections.Cast<IInterviewTreeNode>().TreeToEnumerable(s => s.Children).ToList();
            foreach (var pair in Enumerable.Zip(sourceInterviewTreeNodes, clonedInterviewTreeNodes, (s, c) => new { SourceNode = s, ClonedNode = c }))
            {
                Assert.That(ReferenceEquals(pair.SourceNode, pair.ClonedNode), Is.False);

                var nodeType = pair.SourceNode.GetType();

                var properties = nodeType.GetProperties();
                foreach (var property in properties)
                {
                    if (ignoreTypes.Contains(property.PropertyType))
                        continue;

                    var sourceValue = property.GetValue(pair.SourceNode);
                    var clonedValue = property.GetValue(pair.ClonedNode);
                    if (sourceValue == null && clonedValue == null)
                        continue;

                    Assert.That(ReferenceEquals(sourceValue, clonedValue), Is.False);
                }

                var fields = nodeType.GetFields();
                foreach (var field in fields)
                {
                    if (ignoreTypes.Contains(field.FieldType))
                        continue;

                    var sourceValue = field.GetValue(pair.SourceNode);
                    var clonedValue = field.GetValue(pair.ClonedNode);
                    if (sourceValue == null && clonedValue == null)
                        continue;

                    Assert.That(ReferenceEquals(sourceValue, clonedValue), Is.False);
                }
            }
        }



        private static InterviewTree CreateSimpleTree(Guid interviewId, Identity sectionIdentity, Identity questionIdentity)
            => CreateSimpleTree(interviewId, sectionIdentity, Create.Entity.InterviewTreeQuestion(questionIdentity));

        private static InterviewTree CreateSimpleTree(Guid interviewId, Identity sectionIdentity, InterviewTreeQuestion question)
            => CreateSimpleTree(interviewId, Create.Entity.InterviewTreeSection(sectionIdentity, children: question));

        private static InterviewTree CreateSimpleTree(Guid interviewId, InterviewTreeSection section)
            => Create.Entity.InterviewTree(interviewId, new[] { section });

        private static InterviewTree CreateTreeForClone()
        {
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var sectionIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"));
            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(sectionIdentity, children: new IInterviewTreeNode[]
            {
                Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("33333333333333333333333333333333"))),
                Create.Entity.InterviewTreeRoster(Create.Entity.Identity(Guid.Parse("44444444444444444444444444444444")), children: new IInterviewTreeNode[]
                {
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("55555555555555555555555555555555"))),
                    Create.Entity.InterviewTreeStaticText(Create.Entity.Identity(Guid.Parse("66666666666666666666666666666666"))),
                    Create.Entity.InterviewTreeVariable(Create.Entity.Identity(Guid.Parse("77777777777777777777777777777777"))),
                    Create.Entity.InterviewTreeSubSection(Create.Entity.Identity(Guid.Parse("88888888888888888888888888888888")), children: new IInterviewTreeNode[]
                    {
                        Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("99999999999999999999999999999999"))),
                        Create.Entity.InterviewTreeVariable(Create.Entity.Identity(Guid.Parse("10000000000000000000000000000000"))),
                        Create.Entity.InterviewTreeStaticText(Create.Entity.Identity(Guid.Parse("10000000000000000000111111111111"))),
                    }),
                }),
                Create.Entity.InterviewTreeRoster(Create.Entity.Identity(Guid.Parse("20111111111111111111111111111111")), children: new IInterviewTreeNode[]
                {
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("20222222222222222222222222222222")), questionType: QuestionType.Numeric, answer: 5),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("20333333333333333333333333333333")), questionType: QuestionType.Text, answer: "text"),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("20444444444444444444444444444444")), questionType: QuestionType.DateTime, answer: DateTime.Now),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("20555555555555555555555555555555")), questionType: QuestionType.GpsCoordinates, answer: new GeoPosition(1, 2, 3, 4, DateTimeOffset.Now)),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("20666666666666666666666666666666")), questionType: QuestionType.Multimedia, answer: "filename"),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("20777777777777777777777777777777")), questionType: QuestionType.MultyOption, answer: new decimal[] { 1.2m, 1.7m }),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("20888888888888888888888888888888")), questionType: QuestionType.MultyOption, linkedSourceId: Guid.NewGuid(), answer: new decimal[][] { new decimal[] { 1.2m}, new decimal[] {16m, 2.3m} }),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("20999999999999999999999999999999")), questionType: QuestionType.QRBarcode, answer: "bar code"),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("30111111111111111111111111111111")), questionType: QuestionType.TextList, answer: new string[] { "line1", "line2" }),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("30222222222222222222222222222222")), questionType: QuestionType.SingleOption, answer: 4),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("30333333333333333333333333333333")), questionType: QuestionType.SingleOption, linkedSourceId: Guid.NewGuid(), answer: new RosterVector(new decimal[] {5, 7 })),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("30444444444444444444444444444444")), questionType: QuestionType.MultyOption, isYesNo: true, answer: new AnsweredYesNoOption[] { new AnsweredYesNoOption(5, true), new AnsweredYesNoOption(2, false), }),
                    Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(Guid.Parse("30555555555555555555555555555555")), questionType: QuestionType.Numeric, isDecimal: true, answer: 5.5),
                }),
                Create.Entity.InterviewTreeSubSection(Create.Entity.Identity(Guid.Parse("40111111111111111111111111111111")), children: new IInterviewTreeNode[]
                {
                    Create.Entity.InterviewTreeRoster(Create.Entity.Identity(Guid.Parse("40222222222222222222222222222222")), rosterType: RosterType.Fixed),
                    Create.Entity.InterviewTreeRoster(Create.Entity.Identity(Guid.Parse("40333333333333333333333333333333")), rosterType: RosterType.List, rosterSizeQuestion: Guid.NewGuid()),
                    Create.Entity.InterviewTreeRoster(Create.Entity.Identity(Guid.Parse("40444444444444444444444444444444")), rosterType: RosterType.Multi, rosterSizeQuestion: Guid.NewGuid()),
                    Create.Entity.InterviewTreeRoster(Create.Entity.Identity(Guid.Parse("40555555555555555555555555555555")), rosterType: RosterType.Numeric, rosterSizeQuestion: Guid.NewGuid()),
                    Create.Entity.InterviewTreeRoster(Create.Entity.Identity(Guid.Parse("40666666666666666666666666666666")), rosterType: RosterType.YesNo, rosterSizeQuestion: Guid.NewGuid()),
                })
            });
            var sourceTree = Create.Entity.InterviewTree(interviewId, sourceTreeMainSection);
            return sourceTree;
        }
    }
}