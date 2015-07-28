﻿using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_moving_group_to_illegal_depth : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId1 = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            groupId2 = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionnaire = CreateQuestionnaireWithNesingAndLastGroup(9, parentGroupId, responsibleId);

            AddGroup(questionnaire, groupId1, null, "", responsibleId, null);
            AddGroup(questionnaire, groupId2, groupId1, "", responsibleId, null);
        };

        Because of = () =>
            exception = Catch.Exception(
                () =>
                    questionnaire.MoveGroup(groupId1,
                responsibleId: responsibleId, targetGroupId: parentGroupId, targetIndex:0));

                    

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message = () =>
            new[] { "sub-section", "roster", "depth", "higher", "10"}.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));
        

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId1;
        private static Guid groupId2;
        private static Guid parentGroupId;
        
        private static Exception exception;
    }
}