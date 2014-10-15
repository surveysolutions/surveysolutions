using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.LanguageTests
{
    internal class when_answering_on_question_which_is_part_of_condition_expression_in_another_question : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = RemoteFunc.Invoke(appDomainContext.Domain, () =>
            {                
                var questionnaireId = Guid.Parse("00000000000000000000000000000000");
                var actorId = Guid.Parse("99999999999999999999999999999999");
                var question1Id = Guid.Parse("11111111111111111111111111111111");
                var question2Id = Guid.Parse("22222222222222222222222222222222");

                var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
                ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(question1Id, "q1"),
                    Create.NumericIntegerQuestion(question2Id, "q2", "q1 > 3")
               );

                var interview = SetupInterview(actorId, questionnaireDocument, questionnaireId);

                var eventContext = new EventContext();

                interview.AnswerNumericIntegerQuestion(actorId, question1Id, new decimal[0], DateTime.Now, 4);

                var result = GetQuestionEnabledEvent(eventContext.Events).Questions.FirstOrDefault(q => q.Id == question2Id);

                return new InvokeResults { Questions2Enabled = result != null};
            });

        It should_enable_second_question = () =>
            results.Questions2Enabled.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };
        
        private static InvokeResults results;
        private static AppDomainContext appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool Questions2Enabled { get; set; }
        }
    }
}