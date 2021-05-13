using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Providers
{
    public class InterviewExpressionStorageProvider : IInterviewExpressionStorageProvider
    {
        private readonly IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor;
        private readonly ILogger logger;

        public InterviewExpressionStorageProvider(
            IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor, 
            ILoggerProvider loggerProvider)
        {
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.logger = loggerProvider.GetForType(GetType());
        }

        public IInterviewExpressionStorage GetExpressionStorage(QuestionnaireIdentity questionnaireIdentity)
        {
            Type interviewExpressionStateType = GetExpressionStorageType(questionnaireIdentity);

            try
            {
                var initialExpressionState = Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionStorage;
                return initialExpressionState;
            }
            catch (Exception e)
            {
                logger.Error(
                    $"Error on activating interview expression state. Cannot cast to created object to {nameof(IInterviewExpressionStorage)}",
                    e);
                return null;
            }
        }

        public Type GetExpressionStorageType(QuestionnaireIdentity questionnaireIdentity)
        {
            var assemblyExists = this.questionnaireAssemblyFileAccessor.IsQuestionnaireAssemblyExists(
                questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            if (!assemblyExists)
            {
                logger.Error($"Assembly was not found. Questionnaire={questionnaireIdentity}");
                return null;
            }

            try
            {
                var compiledAssembly = this.questionnaireAssemblyFileAccessor.LoadAssembly(
                    questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

                TypeInfo interviewExpressionStateTypeInfo = compiledAssembly.DefinedTypes.ToList().
                    SingleOrDefault(x => !(x.IsAbstract || x.IsGenericTypeDefinition || x.IsInterface) && x.ImplementedInterfaces.Contains(typeof(IInterviewExpressionStorage)) && x.IsPublic);

                if (interviewExpressionStateTypeInfo == null)
                    throw new Exception($"Type implementing {nameof(IInterviewExpressionStorage)} was not found");

                Type interviewExpressionStateType = interviewExpressionStateTypeInfo.AsType();
                return interviewExpressionStateType;
                
            }
            catch (Exception exception)
            {
                logger.Error($"Error on assembly loading for id={questionnaireIdentity}", exception);

                var exceptions = new List<Exception> { exception };

                if (exception is ReflectionTypeLoadException)
                {
                    var typeLoadException = exception as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    foreach (var loaderException in loaderExceptions)
                    {
                        exceptions.Add(loaderException);
                        logger.Error("LoaderException found", loaderException);
                    }
                }

                if (exception.InnerException != null)
                {
                    exceptions.Add(exception.InnerException);
                    logger.Error("Error on assembly loading (inner)", exception.InnerException);
                }

                //hide original one
                throw new InterviewException("Interview loading error. Code EC0001", new AggregateException(exceptions));
            }
        }
    }
}
