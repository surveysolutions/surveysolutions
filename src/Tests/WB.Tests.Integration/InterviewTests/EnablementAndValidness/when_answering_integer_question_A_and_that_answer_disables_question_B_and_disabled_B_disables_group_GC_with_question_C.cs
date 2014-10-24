﻿using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_integer_question_A_and_that_answer_disables_question_B_and_disabled_B_disables_group_GC_with_question_C_and_B_was_answered : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var groupGCId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(questionAId, "a"),
                    Create.NumericIntegerQuestion(questionBId, "b", "a > 0"),
                    Create.Group(groupGCId, enablementCondition: "b > 0", children: new IComposite[] {
                        Create.NumericIntegerQuestion(questionCId)
                    })
                );

                var interview = SetupInterview(questionnaireDocument, new List<object>
                {
                    Create.Event.QuestionsEnabled(new []
                    {
                        Create.Identity(questionAId),
                        Create.Identity(questionBId),
                        Create.Identity(questionCId)
                    }),
                    Create.Event.GroupsEnabled(Create.Identity(groupGCId)),
                    Create.Event.NumericIntegerQuestionAnswered(questionBId, 0)
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionAId, Empty.RosterVector, DateTime.Now, 0);

                    return new InvokeResults()
                    {
                        GroupGCDisabled = GetFirstEventByType<GroupsDisabled>(eventContext.Events).Groups.FirstOrDefault(g => g.Id == groupGCId) != null,
                        QuestionBDisabled = GetFirstEventByType<QuestionsDisabled>(eventContext.Events).Questions.FirstOrDefault(q => q.Id == questionBId) != null
                    };
                }
            });

        It should_disable_question_B = () =>
            results.GroupGCDisabled.ShouldBeTrue();

        It should_disable_question_C = () =>
            results.QuestionBDisabled.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool GroupGCDisabled { get; set; }
            public bool QuestionBDisabled { get; set; }
        }
    }
}