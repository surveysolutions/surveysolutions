﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Providers
{
    internal class InterviewExpressionStatePrototypeProvider : IInterviewExpressionStatePrototypeProvider
    {
        private static ILogger Logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<InterviewExpressionStatePrototypeProvider>();

        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IInterviewExpressionStateUpgrader interviewExpressionStateUpgrader;

        public InterviewExpressionStatePrototypeProvider(
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor, 
            IFileSystemAccessor fileSystemAccessor, 
            IInterviewExpressionStateUpgrader interviewExpressionStateUpgrader)
        {
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewExpressionStateUpgrader = interviewExpressionStateUpgrader;
        }

        public ILatestInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion)
        {
            var assemblyExists = this.questionnaireAssemblyFileAccessor.IsQuestionnaireAssemblyExists(questionnaireId, questionnaireVersion);

            if (!assemblyExists)
            {
                Logger.Error($"Assembly was not found. Questionnaire={questionnaireId}, version={questionnaireVersion}");
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
                    Logger.Error("Error on activating interview expression state. Cannot cast to created object to IInterviewExpressionState", e);
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.Error($"Error on assembly loading for id={questionnaireId} version={questionnaireVersion}", exception);
                
                var exceptions = new List<Exception> { exception };

                if (exception is ReflectionTypeLoadException)
                {
                    var typeLoadException = exception as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    foreach (var loaderException in loaderExceptions)
                    {
                        exceptions.Add(loaderException);
                        Logger.Error("LoaderEcxeption found", loaderException);
                    }
                }

                if (exception.InnerException != null)
                {
                    exceptions.Add(exception.InnerException);
                    Logger.Error("Error on assembly loading (inner)", exception.InnerException);
                }

                //hide original one
                throw new InterviewException("Interview loading error. Code EC0001", new AggregateException(exceptions));
            }
        }
    }
}
