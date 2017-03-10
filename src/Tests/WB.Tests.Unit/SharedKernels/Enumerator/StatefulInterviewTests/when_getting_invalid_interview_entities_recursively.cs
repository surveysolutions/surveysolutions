﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_invalid_interview_entities_recursively : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(questionnaireId,
                Create.Entity.Group(groupId: group.Id, children: new List<IComposite>()
                {
                    Create.Entity.StaticText(staticText1Id),
                    Create.Entity.StaticText(staticText2Id),
                    Create.Entity.Group(children: new[]
                    {
                        Create.Entity.TextQuestion(questionId),
                        Create.Entity.TextQuestion(disabledQuestionId)
                    })
                })));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(Create.Entity.Identity(staticText1Id)));
            interview.Apply(Create.Event.StaticTextsDeclaredValid(Create.Entity.Identity(staticText2Id)));
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(Create.Entity.Identity(staticText2Id)));
            interview.Apply(Create.Event.AnswersDeclaredInvalid(new[] {Create.Entity.Identity(questionId), Create.Entity.Identity(disabledQuestionId) }));
            interview.Apply(Create.Event.QuestionsDisabled(new[] { Create.Entity.Identity(disabledQuestionId) }));
        };

        Because of = () =>
            invalidEntitiesInInterview = interview.GetInvalidEntitiesInInterview();

        It shouldreturn_3_entities_with_error = () =>
          invalidEntitiesInInterview.Count().ShouldEqual(3);

        It should_contain_only_invalid_enabled_elements = () =>
            invalidEntitiesInInterview.ShouldContainOnly(
                Create.Entity.Identity(staticText1Id),
                Create.Entity.Identity(staticText2Id),
                Create.Entity.Identity(questionId));

        private static StatefulInterview interview;
        private static readonly Guid staticText1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid staticText2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid questionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid disabledQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static IEnumerable<Identity> invalidEntitiesInInterview;
        private static Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        static readonly Identity group = new Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);
    }
}