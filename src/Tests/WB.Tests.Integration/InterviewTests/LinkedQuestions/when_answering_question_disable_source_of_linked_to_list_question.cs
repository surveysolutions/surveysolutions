using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_question_disable_source_of_linked_to_list_question : in_standalone_app_domain
    {
        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(Guid.Parse("11111111111111111111111111111111"),
                    children: new IComposite[]
                    {
                        Abc.Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "num"),
                        Create.ListQuestion(id: listQuestionId, variable: "list", enablementCondition: "num == 1"),
                        Create.SingleOptionQuestion(questionId: linkedSingleOptionQuestion, variable: "lnkSgl", linkedToQuestionId: listQuestionId),
                        Create.MultyOptionsQuestion(id: linkedMultioptionQuestion, variable: "lnkMul", linkedToQuestionId: listQuestionId),
                    });

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, intQuestionId, RosterVector.Empty, DateTime.Now, 1);
                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.Now, new[] { Tuple.Create(0m, "one") });
                interview.AnswerSingleOptionQuestion(userId, linkedSingleOptionQuestion, RosterVector.Empty, DateTime.Now, 0m);
                interview.AnswerMultipleOptionsQuestion(userId, linkedMultioptionQuestion, RosterVector.Empty, DateTime.Now, new[] { 0 });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, intQuestionId, RosterVector.Empty, DateTime.Now, 2);

                    return new InvokeResults
                    {
                        MultiQuestionOptionsChanged = eventContext.AnyEvent<LinkedToListOptionsChanged>(x => x.ChangedLinkedQuestions.Any(y => y.QuestionId.Id == linkedMultioptionQuestion)),
                        SingleQuestionOptionsChanged = eventContext.AnyEvent<LinkedToListOptionsChanged>(x => x.ChangedLinkedQuestions.Any(y => y.QuestionId.Id == linkedSingleOptionQuestion)),
                        SingleQuestionAnswerRemoved = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(y => y.Id == linkedSingleOptionQuestion)),
                        MultiQuestionAnswerRemoved = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(y => y.Id == linkedMultioptionQuestion))
                    };
                }
            });

        It should_remove_answer_for_single_linked_to_list = () => results.SingleQuestionAnswerRemoved.ShouldBeTrue();
        It should_remove_answer_for_multi_linked_to_list = () => results.MultiQuestionAnswerRemoved.ShouldBeTrue();
        It should_change_options_for_single_linked_to_list = () => results.SingleQuestionOptionsChanged.ShouldBeTrue();
        It should_change_options_for_multi_linked_to_list = () => results.MultiQuestionOptionsChanged.ShouldBeTrue();

        static InvokeResults results;
        static Guid userId = Guid.Parse("22222222222222222222222222222222");
        static Guid listQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid linkedSingleOptionQuestion = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Guid linkedMultioptionQuestion = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Guid intQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

        [Serializable]
        public class InvokeResults
        {
            public bool SingleQuestionAnswerRemoved { get; set; }
            public bool MultiQuestionAnswerRemoved { get; set; }
            public bool SingleQuestionOptionsChanged { get; set; }
            public bool MultiQuestionOptionsChanged { get; set; }
        }
    }
}