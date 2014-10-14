using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Integration.LanguageTests
{
    internal class CodeGenerationTestsContext
    {
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