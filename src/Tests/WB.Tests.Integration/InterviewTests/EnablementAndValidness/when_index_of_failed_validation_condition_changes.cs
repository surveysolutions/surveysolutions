using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    [TestFixture]
    [TestOf(typeof(Interview))]
    internal class when_index_of_failed_validation_condition_changes : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void Setup()
        {
           var appDomainContext = AppDomainContext.Create();
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

        }

        [Test]
        public void should_mark_question_as_invalid_with_new_failed_condition_index()
        {
            Assert.That(results.AnswerDeclaredInvalid, Is.True);
        }

        private static InvokeResults results;
        [Serializable]
        internal class InvokeResults
        {
            public bool AnswerDeclaredInvalid { get; set; }
        }
    }
}