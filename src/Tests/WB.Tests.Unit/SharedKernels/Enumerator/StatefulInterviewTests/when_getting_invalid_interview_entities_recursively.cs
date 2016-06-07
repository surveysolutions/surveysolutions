﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
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
                    Create.Entity.TextQuestion(questionId)
                })));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(Create.Entity.Identity(staticText1Id)));
            interview.Apply(Create.Event.StaticTextsDeclaredValid(Create.Entity.Identity(staticText2Id)));
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(Create.Entity.Identity(staticText2Id)));
            interview.Apply(Create.Event.AnswersDeclaredInvalid(new[] {Create.Entity.Identity(questionId)}));
        };

        Because of = () =>
            invalidEntitiesInInterview = interview.GetInvalidEntitiesInInterview();

        It should_contain_first_invalid_static_text = () =>
            invalidEntitiesInInterview.ShouldContain(Create.Entity.Identity(staticText1Id));

        It should_contain_second_invalid_static_text = () =>
            invalidEntitiesInInterview.ShouldContain(Create.Entity.Identity(staticText2Id));

        It should_contain_invalid_question = () =>
            invalidEntitiesInInterview.ShouldContain(Create.Entity.Identity(questionId));

        private static StatefulInterview interview;
        private static readonly Guid staticText1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid staticText2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid questionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static IEnumerable<Identity> invalidEntitiesInInterview;
        private static Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        static readonly Identity group = new Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);
    }
}