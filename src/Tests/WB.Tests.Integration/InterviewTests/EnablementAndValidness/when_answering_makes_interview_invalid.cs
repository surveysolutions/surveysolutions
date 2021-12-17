using System;

using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_makes_interview_invalid : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext appDomainContext;

        private InvokeResults results;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            appDomainContext = AppDomainContext.Create();

            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var q1 = Id.g1;

                var interview = SetupInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(q1, "q1", validationExpression: "self > 10"),
                }));

                InvokeResults result = new InvokeResults();
                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: q1, answer: 1));

                    result.InterviewWasDeclaredInvalid =
                        eventContext.GetSingleEventOrNull<InterviewDeclaredInvalid>() != null;
                }

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: q1, answer: 2));
                    result.DuplidateInvalidDeclarationOccured = eventContext.GetSingleEventOrNull<InterviewDeclaredInvalid>() != null;
                }

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: q1, answer: 11));
                    result.InterviewDeclaredValid = eventContext.GetSingleEventOrNull<InterviewDeclaredValid>() != null;
                }

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: q1, answer: 12));
                    result.DuplicateInterviewDeclaredValid = eventContext.GetSingleEventOrNull<InterviewDeclaredValid>() != null;
                }

                return result;
            });
        }

        [Test]
        public void should_raise_interview_marked_invalid()
        {
            Assert.That(results, Has.Property(nameof(results.InterviewWasDeclaredInvalid)).True);
        }

        [Test]
        public void should_not_raise_duplicate_interview_marked_invalid()
        {
            Assert.That(results, Has.Property(nameof(results.DuplidateInvalidDeclarationOccured)).False);
        }

        [Test]
        public void should_raise_interview_marked_valid()
        {
            Assert.That(results, Has.Property(nameof(results.InterviewDeclaredValid)).True);
        }

        [Test]
        public void should_not_raise_duplicate_interview_marked_valid()
        {
            Assert.That(results, Has.Property(nameof(results.DuplicateInterviewDeclaredValid)).False);
        }

        [Serializable]
        internal class InvokeResults
        {
            public bool InterviewWasDeclaredInvalid { get; set; }
            public bool DuplidateInvalidDeclarationOccured { get; set; }
            public bool InterviewDeclaredValid { get; set; }
            public bool DuplicateInterviewDeclaredValid { get; set; }
        }

    }
}
