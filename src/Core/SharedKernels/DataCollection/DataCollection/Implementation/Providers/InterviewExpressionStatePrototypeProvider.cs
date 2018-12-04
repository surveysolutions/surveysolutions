using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Providers
{
    public class InterviewExpressionStatePrototypeProvider : IInterviewExpressionStatePrototypeProvider
    {
        private readonly ILogger logger;

        private readonly IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor;
        private readonly IInterviewExpressionStateUpgrader interviewExpressionStateUpgrader;

        public InterviewExpressionStatePrototypeProvider(
            IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor, 
            IInterviewExpressionStateUpgrader interviewExpressionStateUpgrader,
            ILoggerProvider loggerProvider)
        {
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.interviewExpressionStateUpgrader = interviewExpressionStateUpgrader;
            logger = loggerProvider.GetFor<InterviewExpressionStatePrototypeProvider>();
        }

        public ILatestInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion)
        {
            var assemblyExists = this.questionnaireAssemblyFileAccessor.IsQuestionnaireAssemblyExists(questionnaireId, questionnaireVersion);

            if (!assemblyExists)
            {
                logger.Error($"Assembly was not found. Questionnaire={questionnaireId}, version={questionnaireVersion}");
                throw new InterviewException("Interview loading error. Code EC0003");
            }

            try
            {
                var compiledAssembly = this.questionnaireAssemblyFileAccessor.LoadAssembly(questionnaireId, questionnaireVersion);

                TypeInfo interviewExpressionStateTypeInfo = compiledAssembly.DefinedTypes.ToList().
                    SingleOrDefault(x => !(x.IsAbstract || x.IsGenericTypeDefinition || x.IsInterface) && x.ImplementedInterfaces.Contains(typeof (IInterviewExpressionState)) && x.IsPublic);

                if (interviewExpressionStateTypeInfo == null)
                    throw new Exception("Type implementing IInterviewExpressionState was not found");

                Type interviewExpressionStateType = interviewExpressionStateTypeInfo.AsType();
                try
                {
                    var initialExpressionState =
                        Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState;

                    ILatestInterviewExpressionState upgradedExpressionState =
                        this.interviewExpressionStateUpgrader.UpgradeToLatestVersionIfNeeded(initialExpressionState);

                    return upgradedExpressionState;
                }
                catch (Exception e)
                {
                    logger.Error("Error on activating interview expression state. Cannot cast to created object to IInterviewExpressionState", e);
                    return null;
                }
            }
            catch (Exception exception)
            {
                logger.Error($"Error on assembly loading for id={questionnaireId} version={questionnaireVersion}", exception);
                
                var exceptions = new List<Exception> { exception };

                if (exception is ReflectionTypeLoadException)
                {
                    var typeLoadException = exception as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    foreach (var loaderException in loaderExceptions)
                    {
                        exceptions.Add(loaderException);
                        logger.Error("LoaderEcxeption found", loaderException);
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

        public IInterviewExpressionStorage GetExpressionStorage(QuestionnaireIdentity questionnaireIdentity)
        {
            var assemblyExists = this.questionnaireAssemblyFileAccessor.IsQuestionnaireAssemblyExists(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            if (!assemblyExists)
            {
                logger.Error($"Assembly was not found. Questionnaire={questionnaireIdentity}");
                throw new InterviewException("Interview loading error. Code EC0003");
            }

            try
            {
                var compiledAssembly = this.questionnaireAssemblyFileAccessor.LoadAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

                TypeInfo interviewExpressionStateTypeInfo = compiledAssembly.DefinedTypes.ToList().
                    SingleOrDefault(x => !(x.IsAbstract || x.IsGenericTypeDefinition || x.IsInterface) && x.ImplementedInterfaces.Contains(typeof(IInterviewExpressionStorage)) && x.IsPublic);

                if (interviewExpressionStateTypeInfo == null)
                    throw new Exception($"Type implementing {nameof(IInterviewExpressionState)} was not found");

                Type interviewExpressionStateType = interviewExpressionStateTypeInfo.AsType();
                try
                {
                    var initialExpressionState = Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionStorage;
                    return initialExpressionState;
                }
                catch (Exception e)
                {
                    logger.Error($"Error on activating interview expression state. Cannot cast to created object to {nameof(IInterviewExpressionState)}", e);
                    return null;
                }
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
                        logger.Error("LoaderEcxeption found", loaderException);
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
