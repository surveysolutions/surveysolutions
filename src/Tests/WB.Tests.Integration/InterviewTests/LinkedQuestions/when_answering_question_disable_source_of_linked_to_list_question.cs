using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_question_disable_source_of_linked_to_list_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(Guid.Parse("11111111111111111111111111111111"),
                    children: new IComposite[]
                    {
                        Abc.Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "num"),
                        Abc.Create.Entity.TextListQuestion(questionId: listQuestionId, variable: "list", enablementCondition: "num == 1"),
                        Abc.Create.Entity.SingleOptionQuestion(questionId: linkedSingleOptionQuestion, variable: "lnkSgl", linkedToQuestionId: listQuestionId),
                        Abc.Create.Entity.MultyOptionsQuestion(id: linkedMultioptionQuestion, variable: "lnkMul", linkedToQuestionId: listQuestionId),
                    });

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, intQuestionId, RosterVector.Empty, DateTime.Now, 1);
                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.Now, new[] { Tuple.Create(0m, "one") });
                interview.AnswerSingleOptionQuestion(userId, linkedSingleOptionQuestion, RosterVector.Empty, DateTime.Now, 0);
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

        [NUnit.Framework.Test] public void should_remove_answer_for_single_linked_to_list () => results.SingleQuestionAnswerRemoved.Should().BeTrue();
        [NUnit.Framework.Test] public void should_remove_answer_for_multi_linked_to_list () => results.MultiQuestionAnswerRemoved.Should().BeTrue();
        [NUnit.Framework.Test] public void should_change_options_for_single_linked_to_list () => results.SingleQuestionOptionsChanged.Should().BeTrue();
        [NUnit.Framework.Test] public void should_change_options_for_multi_linked_to_list () => results.MultiQuestionOptionsChanged.Should().BeTrue();

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
