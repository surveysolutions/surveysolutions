using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Tests.CodeGenerationTests
{
    
    public class CodeGenerationTestsContext
    {
        public static QuestionnaireDocument CreateQuestionnaireForGeneration(Guid? questionnaireId = null)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId ?? Guid.NewGuid() };

            Guid chapterId = Guid.Parse("23232323232323232323232323232323");
            Guid questionId = Guid.Parse("23232323232323232323232323232311");

            questionnaireDocument.AddChapter(chapterId);

            questionnaireDocument.Add(new NumericQuestion()
            {
                PublicKey = questionId,
                StataExportCaption = "persons_n",
                IsInteger = true

            }, chapterId, null);

            var rosterId = Guid.Parse("23232323232323232323232323232322");
            questionnaireDocument.Add(new Group()
            {
                PublicKey = rosterId,
                IsRoster = true,
                RosterSizeQuestionId = questionId

            }, chapterId, null);

            var rosterId1 = Guid.Parse("23232323232323232323232323232388");
            questionnaireDocument.Add(new Group()
            {
                PublicKey = rosterId1,
                IsRoster = true,
                RosterSizeQuestionId = questionId

            }, chapterId, null);

            questionnaireDocument.Add(new NumericQuestion()
            {
                PublicKey = Guid.Parse("23232323232323232323232323232399"),
                StataExportCaption = "q_22",
                IsInteger = true

            }, rosterId1, null);


            Guid pets_questionId = Guid.Parse("23232323232323232323232323232317");
            questionnaireDocument.Add(new NumericQuestion()
            {
                PublicKey = pets_questionId,
                StataExportCaption = "pets_n",
                IsInteger = true

            }, rosterId, null);

            var groupId = Guid.Parse("12345678912345678912345678912345");
            questionnaireDocument.Add(new Group()
            {
                PublicKey = groupId,
                IsRoster = false,
                ConditionExpression = "pets_n > 0"

            }, rosterId, null);

            questionnaireDocument.Add(new TextQuestion()
            {
                PublicKey = Guid.Parse("12345678912345678912345678912340"),
                StataExportCaption = "pets_text",
                ConditionExpression = "pets_n > 0",
                ValidationExpression = "pets_n == 0"

            }, groupId, null);

            questionnaireDocument.Add(new Group()
            {
                PublicKey = Guid.Parse("23232323232323232323232323232324"),
                IsRoster = true,
                RosterSizeQuestionId = pets_questionId,
                ConditionExpression = "pets_text.Length > 0"

            }, rosterId, null);

            return questionnaireDocument;
        }


        public static QuestionnaireDocument CreateQuestionnaireWithOneNumericIntegerQuestionWithValidation(Guid questionnaireId, Guid questionId)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId};

            Guid chapterId = Guid.Parse("23232323232323232323232323232323");
            
            questionnaireDocument.AddChapter(chapterId);

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = questionId,
                StataExportCaption = "test",
                IsInteger = true,
                ValidationExpression = "test > 3"

            }, chapterId, null);


            return questionnaireDocument;
        }

        public static IInterviewExpressionStateProvider GetInterviewExpressionStateProvider(QuestionnaireDocument questionnaireDocument)
        {
            return new InterviewExpressionStateTestingProvider(questionnaireDocument);
        }
        
        private class InterviewExpressionStateTestingProvider : IInterviewExpressionStateProvider
        {
            private IInterviewExpressionState interviewExpressionState;

            public InterviewExpressionStateTestingProvider(QuestionnaireDocument questionnaireDocument)
            {
                var expressionProcessorGenerator = new QuestionnireExpressionProcessorGenerator();

                string resultAssembly;
                var emitResult = expressionProcessorGenerator.GenerateProcessor(questionnaireDocument, out resultAssembly);

                var filePath = Path.GetTempFileName();

                if (emitResult.Success == true && !string.IsNullOrEmpty(resultAssembly))
                {
                    File.WriteAllBytes(filePath, Convert.FromBase64String(resultAssembly));

                    var compiledAssembly = Assembly.LoadFrom(filePath);

                    Type interviewExpressionStateType = compiledAssembly.GetTypes().FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IInterviewExpressionState)));

                    if (interviewExpressionStateType == null)
                        throw new Exception("Type InterviewExpressionState was not found");

                    interviewExpressionState = Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState;
                }
                else
                {
                    throw new Exception("Error on IInterviewExpressionState generation");
                }
               
            }


            public IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion)
            {
                return interviewExpressionState.Clone();
            }

            public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assembly)
            {
                
            }
        }
    }

    
}
