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
                Identity staticTextIdentity = IntegrationCreate.Identity(staticTextId);

                QuestionnaireDocument questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Guid.NewGuid(),
                    Abc.Create.Entity.NumericIntegerQuestion(questionId, variable: "num", validationConditions: new List <ValidationCondition>
                    {
                        IntegrationCreate.ValidationCondition("self < 125", "validation 1"),
                        IntegrationCreate.ValidationCondition("self >= 0", "validation 2")
                    }),
                    Abc.Create.Entity.StaticText(staticTextId, validationConditions: new List<ValidationCondition>
                    {
                        IntegrationCreate.ValidationCondition("num < 125", "static text validation 1"),
                        IntegrationCreate.ValidationCondition("num >= 0", "static text validation 2")
                    }));

                var interview = SetupInterview(questionnaireDocument);
                interview.AnswerNumericIntegerQuestion(Abc.Create.Command.AnswerNumericIntegerQuestionCommand(questionId, answer: -5));
                
                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Abc.Create.Command.AnswerNumericIntegerQuestionCommand(questionId, answer: 126));

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