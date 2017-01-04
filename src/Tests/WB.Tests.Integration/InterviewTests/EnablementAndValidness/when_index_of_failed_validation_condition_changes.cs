using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_index_of_failed_validation_condition_changes : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        public Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                AssemblyContext.SetupServiceLocator();
                Guid questionId = Guid.Parse("11111111111111111111111111111111");

                QuestionnaireDocument questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                    children: Create.NumericIntegerQuestion(questionId, "num", new List<ValidationCondition>
                    {
                        Create.ValidationCondition("self < 125", "validation 1"),
                        Create.ValidationCondition("self >= 0", "validation 2")
                    }));

                var interview = SetupInterview(questionnaireDocument);
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestion(questionId, answer: -5));
                
                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestion(questionId, answer: 126));

                    return new InvokeResults
                    {
                        AnswerDeclaredInvalid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionId)),
                    };
                }
            });

        It should_mark_question_as_invalid_with_new_failed_condition_index = () =>  results.AnswerDeclaredInvalid.ShouldBeTrue();
        
        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool AnswerDeclaredInvalid { get; set; }
        }
    }
}