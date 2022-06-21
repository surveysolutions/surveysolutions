using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    public class LookUpTableTests : InterviewTestsContext
    {
        private AppDomainContext appDomainContext;
        private InvokeResult result;
        
        [SetUp]
        public void SetupTests()
        {
            appDomainContext = AppDomainContext.Create();
        }
        
        [Test]
        public void when_executing_lookup_call_in_russian_locale()
        {
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
                var questionA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                var questionB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
                var variableA = Id.g3;
                var lookupId = Guid.Parse("dddddddddddddddddddddddddddddddd");
                var userId = Guid.NewGuid();

                var lookupTableContent = IntegrationCreate.LookupTableContent(new [] {"min", "max"},
                    IntegrationCreate.LookupTableRow(1, new decimal?[] { 1.1m, 10.1m}),
                    IntegrationCreate.LookupTableRow(2, new decimal?[] { 4.2m, 10.2m}),
                    IntegrationCreate.LookupTableRow(3, new decimal?[] { 6.3m, null})
                );
                Dictionary<Guid, LookupTableContent> lookupTableContents = new() { { lookupId, lookupTableContent } };

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: questionA, variable: "a", validationExpression: "a > price[2].min && a < price[2].max"),
                    Create.Entity.NumericRealQuestion(id: questionB, variable: "b", validationExpression: "b > price[2].min && b < price[2].max", 
                        enablementCondition:"a > price[1].min && a < price[1].max"),
                    Create.Entity.Variable(variableA, VariableType.String, "v1", "b.ToString()")
                });

                questionnaire.LookupTables.Add(lookupId, Create.Entity.LookupTable("price"));
                var package = new QuestionnaireCodeGenerationPackage(questionnaire, lookupTableContents);
                
                var interview = SetupInterview(appDomainContext.AssemblyLoadContext, package);

                var culture = CultureInfo.GetCultureInfo("ru");
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                
                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionA, RosterVector.Empty, DateTime.Now, 5);
                    interview.AnswerNumericRealQuestion(userId, questionB, RosterVector.Empty, DateTime.Now, 0.3);
                    
                    return new InvokeResult
                    {
                        IsQuestionAInValid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionA)),
                        IsQuestionBInValid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionB)),
                        VariableAHasExpectedValue = eventContext.AnyEvent<VariablesChanged>(x=> x.ChangedVariables.Any(v => (string) v.NewValue == "0,3"))
                    };
                }
            });
            
            Assert.That(result.IsQuestionAInValid, Is.False);
            Assert.That(result.IsQuestionBInValid, Is.True);
            Assert.That(result.VariableAHasExpectedValue, Is.True);
        } 

        [TearDown]
        public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }
        
        [Serializable]
        private class InvokeResult
        {
            public bool IsQuestionAInValid { get; set; }
            public bool IsQuestionBInValid { get; set; }
            public bool VariableAHasExpectedValue { get; set; }
        }
    }
}
