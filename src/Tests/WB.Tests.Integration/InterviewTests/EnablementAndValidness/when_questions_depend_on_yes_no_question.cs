using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NHibernate.Util;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    public class when_questions_depend_on_yes_no_question : InterviewTestsContext
    {
        [Test(Description = "KP-11929")]
        public void should_enable_disable_needed()
        {
            var appDomainContext = AppDomainContext.Create();
            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var userId = Guid.NewGuid();
                var q1Id = Id.g1;
                var q2Id = Id.g2;
                var q3Id = Id.g3;

                var questionnaireId = Id.gA;

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(q1Id, variable: "q1"),
                    Create.Entity.MultipleOptionsQuestion(q2Id, variable: "q2", answers: new[] {1, 9}, isYesNo: true,
                        enablementCondition: "q1 > 2"),
                    Create.Entity.TextListQuestion(q3Id, variable: "q3", enablementCondition: "q2.Yes.Contains(9)")
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new object[] { });

                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: q1Id, answer: 10));
                interview.AnswerYesNoQuestion(
                    Create.Command.AnswerYesNoQuestion(userId: userId, questionId: q2Id, answeredOptions: new []
                    {
                        Create.Entity.AnsweredYesNoOption(9, true)
                    }));

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(
                        Create.Command.AnswerNumericIntegerQuestionCommand(questionId: q1Id, answer: 0));

                    return new InvokeResults
                    {
                        WasTextListQuestionDisabled = HasEvent<QuestionsDisabled>(eventContext.Events, e => e.Questions.Any(q => q.Id == q3Id))
                    };
                }
            });

            Assert.That(results.WasTextListQuestionDisabled);
        }

        
        [Serializable]
        internal class InvokeResults
        {
            public bool WasTextListQuestionDisabled { get; set; }
        }
    }
}
