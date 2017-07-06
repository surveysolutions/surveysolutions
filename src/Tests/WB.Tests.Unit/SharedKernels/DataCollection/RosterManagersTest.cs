using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(RosterManager))]
    [TestFixture]
    public class RosterManagerTests
    {
        [Test]
        public void when_YesNoRosterManager_Calcuates_Expected_Identities_with_tree_not_having_size_question_Then_should_return_empty_list()
        {
            var parentEntityId = Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111"),
                Create.Entity.RosterVector(1));

            var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            
            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(sectionIdentity: parentEntityId);

            //doesn't contain roster size questoin
            var tree = Create.Entity.InterviewTree(sections: sourceTreeMainSection);
            
            var questionnaire = new Mock<IQuestionnaire>();
            questionnaire.Setup(x => x.GetRosterSizeQuestion(rosterId)).Returns(rosterSizeQuestionId);
            
            var textFactoryMock = new Mock<ISubstitutionTextFactory> { DefaultValue = DefaultValue.Mock };
            var roster = new YesNoRosterManager(tree, questionnaire.Object, rosterId, textFactoryMock.Object);

            //act
            var entities = roster.CalcuateExpectedIdentities(parentEntityId);

            //assert
            Assert.That(entities, Is.Not.Null);
            Assert.That(entities.Count, Is.EqualTo(0));
        }

        [Test]
        public void when_NumericRosterManager_Update_Roster_with_answered_roster_title_question_Then_should_roster_title_has_specified_value()
        {
            //arrange
            var parentEntityId = Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111"),
                Create.Entity.RosterVector());

            var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterTitleQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterIdentity = Identity.Create(rosterId, Create.Entity.RosterVector(1, 2));
            var rosterTitleQuestionIdentity = Identity.Create(rosterTitleQuestionId, Create.RosterVector(1, 2));

            var rosterTitleQuestion = Create.Entity.InterviewTreeQuestion(rosterTitleQuestionIdentity,
                questionType: QuestionType.MultyOption, answer: new decimal[] {1, 3});

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(parentEntityId.Id,
                    children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(rosterSizeQuestionId),
                        Create.Entity.NumericRoster(rosterId, rosterSizeQuestionId: rosterSizeQuestionId,
                            rosterTitleQuestionId: rosterTitleQuestionId, children: new IComposite[]
                            {
                                Create.Entity.MultyOptionsQuestion(rosterTitleQuestionId, options: Create.Entity.Options(1, 3))
                            })
                    }));

            var numericRoster = Create.Entity.InterviewTreeRoster(rosterIdentity, rosterType: RosterType.Numeric,
                rosterSizeQuestion: rosterSizeQuestionId, rosterTitleQuestionIdentity: rosterTitleQuestionIdentity,
                children: rosterTitleQuestion);

            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(sectionIdentity: parentEntityId,
                children: numericRoster);

            var tree = Create.Entity.InterviewTree(sections: sourceTreeMainSection, questionnaire: questionnaire);

            var roster = new NumericRosterManager(tree, questionnaire, rosterId, Create.Service.SubstitutionTextFactory());

            //act
            roster.UpdateRoster(numericRoster, parentEntityId, rosterIdentity, 2);

            //assert
            Assert.That(numericRoster.RosterTitle, Is.EqualTo("Option 1, Option 3"));
        }
    }
}
