using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_removing_answer_from_gps_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();
                var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Chapter(children: new IComposite[]
                    {
                        Create.GpsCoordinateQuestion(variable: "gps", 
                        validationExpression: "false",
                        id: questionId),
                    }),
                });

                var interview = SetupInterview(questionnaireDocument,
                    events: new object[]
                    {
                        Create.Event.GeoLocationQuestionAnswered(Create.Identity(questionId), 1, 1),
                        Create.Event.AnswersDeclaredInvalid(new[]
                        {
                            Create.Identity(questionId)
                        }),
                    });

                using (var eventContext = new EventContext())
                {
                    interview.RemoveAnswer(questionId, RosterVector.Empty, Guid.Empty, DateTime.Now);

                    return new InvokeResult
                    {
                        AnswersDeclaredValidEventCount = eventContext.Count<AnswersDeclaredValid>(),
                    };
                }
            });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };


        It should_mark_gps_question_as_valid = () => result.AnswersDeclaredValidEventCount.ShouldEqual(1);

        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public int AnswersDeclaredValidEventCount { get; set; }
        }
    }
}