using System;
using Moq;
using NUnit.Framework;
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
            
            var textFactoryMock = new Mock<ISubstitionTextFactory> { DefaultValue = DefaultValue.Mock };
            var roster = new YesNoRosterManager(tree, questionnaire.Object, rosterId, textFactoryMock.Object);

            //act
            var entities = roster.CalcuateExpectedIdentities(parentEntityId);

            //assert
            Assert.That(entities, Is.Not.Null);
            Assert.That(entities.Count, Is.EqualTo(0));
        }
    }
}
