using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

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
                Guid staticTextId = Guid.Parse("22222222222222222222222222222222");
                Identity staticTextIdentity = Abc.Create.Identity(staticTextId);

                QuestionnaireDocument questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Guid.NewGuid(),
                    Create.Entity.NumericIntegerQuestion(questionId, variable: "num", validationConditions: new List <ValidationCondition>
                    {
                        Create.Entity.ValidationCondition(expression: "self < 125", message: "validation 1"),
                        Create.Entity.ValidationCondition(expression: "self >= 0", message: "validation 2")
                    }),
                    Create.Entity.StaticText(staticTextId, validationConditions: new List<ValidationCondition>
                    {
                        Create.Entity.ValidationCondition(expression: "num < 125", message: "static text validation 1"),
                        Create.Entity.ValidationCondition(expression: "num >= 0", message: "static text validation 2")
                    }));

                var interview = SetupInterview(questionnaireDocument);
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: questionId, answer: -5));
                
                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: questionId, answer: 126));

                    return new InvokeResults
                    {
                        AnswerDeclaredInvalid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionId)),
                        StaticTextDeclaredInvalid = eventContext.AnyEvent<StaticTextsDeclaredInvalid>(x => x.GetFailedValidationConditionsDictionary().ContainsKey(staticTextIdentity)),
                    };
                }
            });

        It should_mark_question_as_invalid_with_new_failed_condition_index = () =>  results.AnswerDeclaredInvalid.ShouldBeTrue();
        
        It should_mark_static_text_as_invalid_with_new_failed_condition_index = () => results.StaticTextDeclaredInvalid.ShouldBeTrue();

        static InvokeResults results;
        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool AnswerDeclaredInvalid { get; set; }
            public bool StaticTextDeclaredInvalid { get; set; }
        }
    }
}