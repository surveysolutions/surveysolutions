using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Integration.LanguageTests
{
    internal class CodeGenerationTestsContext
    {
        protected static Interview SetupInterview(Guid actorId, QuestionnaireDocument questionnaireDocument, Guid questionnaireId, List<object> evnts)
        {
            var questionnaire = Create.Questionnaire(actorId, questionnaireDocument);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionnaire.GetQuestionnaire()
                    && repository.GetHistoricalQuestionnaire(questionnaireId, questionnaire.GetQuestionnaire().Version) == questionnaire.GetQuestionnaire()
                    && repository.GetHistoricalQuestionnaire(questionnaireId, 1) == questionnaire.GetQuestionnaire());

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            SetupRoslyn(questionnaireDocument);

            var interview = Create.Interview(questionnaireId: questionnaireId);

            ApplyAllEvents(interview, evnts);

            return interview;
        }

        public static void ApplyAllEvents(Interview interview, List<object> evnts)
        {
            foreach (var evnt in evnts)
            {
                interview.Apply((dynamic)evnt);
            }
        }

        public static T GetFirstEventByType<T>(IEnumerable<UncommittedEvent> events)
            where T : class
        {
            return ((T)events.First(b => b.Payload is T).Payload);
        }

        public static void SetupRoslyn(QuestionnaireDocument questionnaireDocument)
        {
            IInterviewExpressionState state = GetInterviewExpressionState(questionnaireDocument);

            var statePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(a => a.GetExpressionState(It.IsAny<Guid>(), It.IsAny<long>()) == state);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IInterviewExpressionStatePrototypeProvider>())
                .Returns(statePrototypeProvider);
        }

        public static IInterviewExpressionState GetInterviewExpressionState(QuestionnaireDocument questionnaireDocument)
        {
            var expressionProcessorGenerator = new QuestionnireExpressionProcessorGenerator();

            string resultAssembly;
            var emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument, out resultAssembly);

            var filePath = Path.GetTempFileName();

            if (emitResult.Success && !string.IsNullOrEmpty(resultAssembly))
            {
                File.WriteAllBytes(filePath, Convert.FromBase64String(resultAssembly));

                var compiledAssembly = Assembly.LoadFrom(filePath);

                Type interviewExpressionStateType =
                    compiledAssembly.GetTypes()
                        .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IInterviewExpressionState)));

                if (interviewExpressionStateType == null)
                    throw new Exception("Type InterviewExpressionState was not found");

                var interviewExpressionState = Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState;

                if (interviewExpressionState == null)
                    throw new Exception("Error on IInterviewExpressionState generation");

                return interviewExpressionState;
            }

            throw new Exception("Error on IInterviewExpressionState generation");
        }
    }
}