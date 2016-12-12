using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_removing_numeric_roster_instance_with_nested_multi_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("11111111111110000111111111111111");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");

                var rosterSizeIntQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var numericRosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                var rosterSizeMultiQuestionId = Guid.Parse("11111111111111111111111111111111");
                var nestedMultiRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

                var textQuestionId = Guid.Parse("33333333333333333333333333333333");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.NumericIntegerQuestion(id: rosterSizeIntQuestionId, variable:"numeric"),
                    Create.Roster(
                        id: numericRosterId, 
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: rosterSizeIntQuestionId,
                        variable: "numRoster",
                        children: new IComposite[]
                        {
                            Create.MultyOptionsQuestion(rosterSizeMultiQuestionId, variable: "multi", options: new List<Answer>
                            {
                                Create.Option(value: "1", text: "Hello"),
                                Create.Option(value: "2", text: "World")
                            }),
                            Create.Roster(
                                id: nestedMultiRosterId,
                                rosterSizeSourceType: RosterSizeSourceType.Question,
                                rosterSizeQuestionId: rosterSizeMultiQuestionId,
                                variable: "multiRoster",
                                children: new IComposite[]
                                {
                                    Create.TextQuestion(textQuestionId, variable: "text")
                                })
                        })
                );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument, new List<object>() { });

                interview.AnswerNumericIntegerQuestion(userId, rosterSizeIntQuestionId, Empty.RosterVector, DateTime.Now, 2);

                interview.AnswerMultipleOptionsQuestion(userId, rosterSizeMultiQuestionId, new [] { 0m }, DateTime.Now, new [] { 1, 2 });
                interview.AnswerTextQuestion(userId, textQuestionId, new[] { 0m, 1m }, DateTime.Now, "aaa");
                interview.AnswerTextQuestion(userId, textQuestionId, new[] { 0m, 2m }, DateTime.Now, "bbb");

                interview.AnswerMultipleOptionsQuestion(userId, rosterSizeMultiQuestionId, new [] { 1m }, DateTime.Now, new [] { 1, 2 });
                interview.AnswerTextQuestion(userId, textQuestionId, new[] { 1m, 1m }, DateTime.Now, "ccc");
                interview.AnswerTextQuestion(userId, textQuestionId, new[] { 1m, 2m }, DateTime.Now, "ddd");

                using (var eventContext = new EventContext())
                {                    
                    interview.AnswerNumericIntegerQuestion(userId, rosterSizeIntQuestionId, Empty.RosterVector, DateTime.Now, 1);

                    result.HasRemoveAnswerInPosition_0_1 = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == textQuestionId && q.RosterVector.SequenceEqual(new[] { 0m, 1m })));
                    result.HasRemoveAnswerInPosition_0_2 = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == textQuestionId && q.RosterVector.SequenceEqual(new[] { 0m, 2m })));
                    result.HasRemoveAnswerInPosition_1_1 = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == textQuestionId && q.RosterVector.SequenceEqual(new[] { 1m, 1m })));
                    result.HasRemoveAnswerInPosition_1_2 = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == textQuestionId && q.RosterVector.SequenceEqual(new[] { 1m, 2m })));
                }

                return result;
            });

        It should_not_remove_answer_in_first_numeric_and_first_muti_roster = () =>
            results.HasRemoveAnswerInPosition_0_1.ShouldBeFalse();

        It should_not_remove_answer_in_first_numeric_and_second_muti_roster = () =>
            results.HasRemoveAnswerInPosition_0_2.ShouldBeFalse();

        It should_remove_answer_in_second_numeric_and_first_muti_roster = () =>
            results.HasRemoveAnswerInPosition_1_1.ShouldBeTrue();

        It should_remove_answer_in_second_numeric_and_second_muti_roster = () =>
            results.HasRemoveAnswerInPosition_1_2.ShouldBeTrue();

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
            public bool HasRemoveAnswerInPosition_0_1 { get; set; }
            public bool HasRemoveAnswerInPosition_0_2 { get; set; }
            public bool HasRemoveAnswerInPosition_1_1 { get; set; }
            public bool HasRemoveAnswerInPosition_1_2 { get; set; }
        }          
    }
}