using System;
using AppDomainToolkit;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_removing_answer_from_gps_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();
                var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Abc.Create.Entity.GpsCoordinateQuestion(questionId, "gps", enablementCondition: null, validationExpression: "false"),
                });

                var interview = SetupInterview(questionnaireDocument);
                interview.AnswerGeoLocationQuestion(Guid.NewGuid(), questionId, RosterVector.Empty, DateTime.Now, 1, 1, 1, 1, DateTimeOffset.Now);

                using (var eventContext = new EventContext())
                {
                    interview.RemoveAnswer(questionId, RosterVector.Empty, Guid.Empty, DateTime.Now);

                    return new InvokeResult
                    {
                        AnswersDeclaredValidEventCount = eventContext.Count<AnswersDeclaredValid>(),
                    };
                }
            });

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [NUnit.Framework.Test] public void should_mark_gps_question_as_valid () => 
            result.AnswersDeclaredValidEventCount.Should().Be(1);

        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public int AnswersDeclaredValidEventCount { get; set; }
        }
    }
}
