using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
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
                var question1Id = Guid.Parse("11111111111111111111111111111111");
                var question2Id = Guid.Parse("22222222222222222222222222222222");
                var actorId = Guid.Parse("99999999999999999999999999999999");

                var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
                ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

                var questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

                questionnaireDocument.Add(new NumericQuestion
                {
                    QuestionType = QuestionType.Numeric,
                    PublicKey = question1Id,
                    StataExportCaption = "q1",
                    IsInteger = true,
                }, questionnaireId, null);

                questionnaireDocument.Add(new NumericQuestion
                {
                    QuestionType = QuestionType.Numeric,
                    PublicKey = question2Id,
                    StataExportCaption = "q2",
                    IsInteger = true,
                    ConditionExpression = "q1 > 3"
                }, questionnaireId, null);

                var questionnaire = new Questionnaire(actorId, questionnaireDocument, false, string.Empty);

                IQuestionnaire questionaire = questionnaire.GetQuestionnaire();

                var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                    => repository.GetQuestionnaire(questionnaireId) == questionaire
                        && repository.GetHistoricalQuestionnaire(questionnaireId, questionaire.Version) == questionaire
                        && repository.GetHistoricalQuestionnaire(questionnaireId, 1) == questionaire);

                Mock.Get(ServiceLocator.Current)
                    .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                    .Returns(questionnaireRepository);

                IInterviewExpressionState state = GetInterviewExpressionState(questionnaireDocument);

                var st = Mock.Of<IInterviewExpressionStatePrototypeProvider>(a => a.GetExpressionState(questionnaireId, questionaire.Version) == state);

                Mock.Get(ServiceLocator.Current)
                    .Setup(locator => locator.GetInstance<IInterviewExpressionStatePrototypeProvider>())
                    .Returns(st);

                var interview = new Interview(
                    new Guid("A0A0A0A0B0B0B0B0A0A0A0A0B0B0B0B0"),
                    actorId,
                    questionnaireId,
                    1,
                    new Dictionary<Guid, object>(),
                    new DateTime(2012, 12, 20),
                    new Guid("D222D222D222D222D222D222D222D222"));

                var eventContext = new EventContext();

                interview.AnswerNumericIntegerQuestion(actorId, question1Id, new decimal[0], DateTime.Now, 4);

                EnablementChanges enablementChanges = state.ProcessEnablementConditions();

                return new InvokeResults
                {
                    QuestionsToBeDisabledCount = enablementChanges.QuestionsToBeDisabled.Count,
                    QuestionsToBeEnabledCount = enablementChanges.QuestionsToBeEnabled.Count
                };
            });

        It should_disabled_question_count_equal_0 = () =>
            results.QuestionsToBeDisabledCount.ShouldEqual(0);

        It should_enabled_question_count_equal_1 = () =>
            results.QuestionsToBeEnabledCount.ShouldEqual(1);

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
            public int QuestionsToBeDisabledCount { get; set; }
            public int QuestionsToBeEnabledCount { get; set; }
        }
    }
}