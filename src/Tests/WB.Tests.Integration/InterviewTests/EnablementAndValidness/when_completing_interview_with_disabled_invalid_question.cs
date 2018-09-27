using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_completing_interview_with_disabled_invalid_question_and_static_text : InterviewTestsContext
    {

        [OneTimeSetUp]
        public void Setup()
        {
            appDomainContext = AppDomainContext.Create();
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Integration.SetUp.MockedServiceLocator();

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.Entity.TextQuestion(Id.gA, variable: "trigger"),
                    Create.Entity.TextQuestion(questionId: Id.g1, variable: "q1", 
                        enablementCondition: "trigger.Contains(\"a\")",
                        validationExpression: "trigger.Contains(\"b\")"),
                    Create.Entity.StaticText(Id.g2, 
                        enablementCondition: "trigger.Contains(\"a\")", 
                        validationConditions: new List<ValidationCondition>
                        {
                            Create.Entity.ValidationCondition("trigger.Contains(\"b\")")
                        })
                });

                var interview = SetupStatefullInterview(questionnaire);

                interview.AnswerTextQuestion(Id.gB, Id.gA, RosterVector.Empty, DateTime.UtcNow, "a");

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextQuestion(Id.gB, Id.gA, RosterVector.Empty, DateTime.UtcNow, "c");
                    interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow);

                    return new InvokeResults
                    {
                        RaisedInterviewDeclaredInvalid = eventContext.GetSingleEventOrNull<InterviewDeclaredInvalid>() != null,
                        RaisedInterviewDeclaredValid = eventContext.GetSingleEventOrNull<InterviewDeclaredValid>() != null
                    };
                }
            });
        }

        [Test]
        public void should_not_declare_interview_invalid()
        {
            Assert.That(results, Has.Property(nameof(results.RaisedInterviewDeclaredInvalid)).False);
        }

        [Test]
        public void should_declare_interview_as_valid()
        {
            Assert.That(results, Has.Property(nameof(results.RaisedInterviewDeclaredValid)).True);
        }

        [Serializable]
        internal class InvokeResults
        {
            public bool RaisedInterviewDeclaredInvalid { get; set; }
            public bool RaisedInterviewDeclaredValid { get; set; }
        }

        [OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        private InvokeResults results;
    }
}
