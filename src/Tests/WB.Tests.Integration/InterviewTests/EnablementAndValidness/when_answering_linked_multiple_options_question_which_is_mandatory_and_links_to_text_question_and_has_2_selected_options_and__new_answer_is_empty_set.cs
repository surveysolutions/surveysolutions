using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_linked_multiple_options_question_which_is_mandatory_and_links_to_text_question_and_has_2_selected_options_and__new_answer_is_empty_set : InterviewTestsContext
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

                var questionnaireId = Guid.Parse("00000000000000000000000000000000");
                var categoricalMultiOptionMandatoryQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var rosterTextQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var rosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var rosterSizeQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                var rosterInstanceId = (decimal)22.5;
                var rosterVector = Empty.RosterVector.Concat(new[] { rosterInstanceId }).ToArray();

                var linkedOption1Vector = new decimal[] { 0 };
                var linkedOption2Vector = new decimal[] { 1 };
                var linkedOption3Vector = new decimal[] { 2 };

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(rosterSizeQuestionId),
                    Create.Roster(rosterId, rosterSizeQuestionId: rosterSizeQuestionId,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterTitleQuestionId: rosterTextQuestionId,
                        children: new[]
                        {
                            Create.Question(rosterTextQuestionId),
                            Create.MultyOptionsQuestion(categoricalMultiOptionMandatoryQuestionId, isMandatory: true,
                                linkedToQuestionId: rosterTextQuestionId)
                        })
                    );

                var interview = SetupInterview(questionnaireDocument, new object[]
                {
                    Create.Event.RosterInstancesAdded(new[]
                    {
                        Create.AddedRosterInstance(rosterId, rosterInstanceId: linkedOption1Vector[0]),
                        Create.AddedRosterInstance(rosterId, rosterInstanceId: linkedOption2Vector[0]),
                        Create.AddedRosterInstance(rosterId, rosterInstanceId: linkedOption3Vector[0]),
                    }),
                    Create.Event.TextQuestionAnswered(rosterTextQuestionId, propagationVector: linkedOption1Vector, answer: "linked option 1"),
                    Create.Event.TextQuestionAnswered(rosterTextQuestionId, propagationVector: linkedOption2Vector, answer: "linked option 2"),
                    Create.Event.TextQuestionAnswered(rosterTextQuestionId, propagationVector: linkedOption3Vector, answer: "linked option 3"),
                    Create.Event.RosterInstancesAdded(new[]
                    {
                        Create.AddedRosterInstance(rosterId, rosterInstanceId: rosterInstanceId),
                        Create.AddedRosterInstance(rosterId, rosterInstanceId: rosterInstanceId),
                    }),
                    Create.Event.MultipleOptionsLinkedQuestionAnswered(categoricalMultiOptionMandatoryQuestionId,
                        propagationVector: rosterVector,
                        selectedValues: new[] {linkedOption3Vector, linkedOption2Vector}),
                });
                

                using (var eventContext = new EventContext())
                {
                    interview.AnswerMultipleOptionsLinkedQuestion(userId, categoricalMultiOptionMandatoryQuestionId, rosterVector, DateTime.Now, new decimal[][] { });

                    return new InvokeResults()
                    {
                        MandatoryCatagoricalMultiLinkedQuestionAnswered =
                            HasEvent<MultipleOptionsLinkedQuestionAnswered>(eventContext.Events),
                        MandtoryQuestionDeclaredAsInvalidOnce =
                            EventsByType<AnswersDeclaredInvalid>(eventContext.Events).Count() == 1
                    };
                }
            });

        It should_raise_MultipleOptionsLinkedQuestionAnswered_event = () =>
            results.MandatoryCatagoricalMultiLinkedQuestionAnswered.ShouldBeTrue();

        It should_raise_1_AnswersDeclaredInvalid_event = () =>
           results.MandtoryQuestionDeclaredAsInvalidOnce.ShouldBeTrue();

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
            public bool MandatoryCatagoricalMultiLinkedQuestionAnswered { get; set; }
            public bool MandtoryQuestionDeclaredAsInvalidOnce { get; set; }
        }
    }
}