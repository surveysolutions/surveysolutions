using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Substitutions
{
    internal class when_group_title_has_substitution : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            group1Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            group2Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group("root")
            {
                Children = new List<IComposite>()
                {
                    Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId, variable: "subst"),
                    Create.Entity.Group(groupId: group1Id,
                        title: "GroupB - %subst%"),
                    Create.Entity.Roster(rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        title: "Roster",
                        children:
                        new List<IComposite>
                        {
                            Create.Entity.Group(groupId: group2Id, title: "GroupC - %subst%")
                        })
                }.ToReadOnlyCollection()
            });

            Guid questionnaireId = Guid.NewGuid();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository);

            events = new EventContext();
            interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), rosterSizeQuestionId, Empty.RosterVector,
                DateTime.Now, 2);
        }

        [Test]
        public void should_raise_title_changed_event_for_3_groups() =>
            events.GetEvent<SubstitutionTitlesChanged>().Groups.Length.Should().Be(3);

        [Test]
        public void should_raise_title_changed_event_for_group_after_answer() =>
            events.GetEvent<SubstitutionTitlesChanged>().Groups.Should().BeEquivalentTo(new []{
                Create.Entity.Identity(group2Id, Create.Entity.RosterVector(0)),
                Create.Entity.Identity(group2Id, Create.Entity.RosterVector(1)),
                Create.Entity.Identity(group1Id, RosterVector.Empty)});

        static EventContext events;
        static Interview interview;
        static Guid rosterSizeQuestionId;
        static Guid group1Id;
        static Guid group2Id;
    }
}
