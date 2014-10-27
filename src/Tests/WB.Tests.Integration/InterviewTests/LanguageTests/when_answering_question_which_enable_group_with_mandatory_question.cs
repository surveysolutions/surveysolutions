using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_question_which_enable_group_with_mandatory_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var mandatoryQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var answeringQuestionId = Guid.Parse("11111111111111111111111111111111");

                var interview = SetupInterview(
                    Create.QuestionnaireDocument(children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(id: answeringQuestionId, variable: "a", isMandatory: true),
                        Create.Group(enablementCondition: "a == 0", children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(id: mandatoryQuestionId, isMandatory: true)
                        })
                    })
                );

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, answeringQuestionId, Empty.RosterVector, DateTime.Now, 0);

                    return new InvokeResult
                    {
                        AnswersDeclaredValid = eventContext.GetSingleEvent<AnswersDeclaredValid>().Questions.Any(identity => identity.Id == mandatoryQuestionId),
                        AnswersDeclaredInvalid = eventContext.GetSingleEvent<AnswersDeclaredInvalid>().Questions.Any(identity => identity.Id == mandatoryQuestionId)
                    };
                }
            });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_not_raise_AnswersDeclaredValid_event = () =>
            result.AnswersDeclaredValid.ShouldBeFalse();

        It should_raise_AnswersDeclaredInvalid_event = () =>
            result.AnswersDeclaredInvalid.ShouldBeTrue();

        private static AppDomainContext appDomainContext;
        private static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public bool AnswersDeclaredValid { get; set; }
            public bool AnswersDeclaredInvalid { get; set; }
        }
    }
}
