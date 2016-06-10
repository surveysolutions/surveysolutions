﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Substitutions
{
    internal class when_group_title_has_substitution : InterviewTestsContext
    {
        Establish context = () =>
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
                }
            });

            Guid questionnaireId = Guid.NewGuid();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            events = new EventContext();
        };

        Because of = () => interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), rosterSizeQuestionId, Empty.RosterVector, DateTime.Now, 2);

        It should_raise_title_changed_event_for_group_after_answer = () =>
            events.ShouldContainEvent<SubstitutionTitlesChanged>(x => x.Groups.Length == 3 && 
                x.Groups[0].Id == group1Id && 
                x.Groups[1].Equals(Create.Entity.Identity(group2Id, Create.Entity.RosterVector(0))) &&
                x.Groups[2].Equals(Create.Entity.Identity(group2Id, Create.Entity.RosterVector(1))) 
            );

        static EventContext events;
        static Interview interview;
        static Guid rosterSizeQuestionId;
        static Guid group1Id;
        static Guid group2Id;
    }
}