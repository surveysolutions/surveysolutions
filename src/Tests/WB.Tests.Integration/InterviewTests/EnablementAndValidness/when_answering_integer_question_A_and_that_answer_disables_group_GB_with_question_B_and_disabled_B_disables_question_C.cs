using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_integer_question_A_and_that_answer_disables_group_GB_with_question_B_and_disabled_B_disables_question_C_and_B_was_answered : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var emptyRosterVector = new decimal[] { };
                var  userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var groupGBId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                Setup.MockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.NumericIntegerQuestion(questionAId, "a"),
                    Create.Group(groupGBId, enablementCondition: "a > 0", children: new IComposite[] {
                        Create.NumericIntegerQuestion(questionBId, "b")
                    }),
                    Create.NumericIntegerQuestion(questionCId, "c", enablementCondition: "b > 0")
                );

                var interview = SetupInterview(questionnaireDocument, new List<object>
                {
                    Create.Event.QuestionsEnabled(new []
                    {
                        Create.Identity(questionAId),
                        Create.Identity(questionBId),
                        Create.Identity(questionCId)
                    }),
                    Create.Event.GroupsEnabled(Create.Identity(groupGBId)),
                    Create.Event.NumericIntegerQuestionAnswered(questionBId, 1)
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionAId, emptyRosterVector, DateTime.Now, 0);

                    return new InvokeResults()
                    {
                        GroupGBDisabled = GetFirstEventByType<GroupsDisabled>(eventContext.Events).Groups.FirstOrDefault(g => g.Id == groupGBId) != null,
                        QuestionCDisabled = GetFirstEventByType<QuestionsDisabled>(eventContext.Events).Questions.FirstOrDefault(g => g.Id == questionCId) != null
                    };
                }
            });

        It should_disable_question_B = () =>
            results.GroupGBDisabled.ShouldBeTrue();

        It should_disable_question_C = () =>
            results.QuestionCDisabled.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool GroupGBDisabled { get; set; }
            public bool QuestionCDisabled { get; set; }
        }
    }
}