using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Providers
{
    internal class InterviewExpressionStatePrototypeProvider : IInterviewExpressionStatePrototypeProvider
    {
        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public InterviewExpressionStatePrototypeProvider(IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor, IFileSystemAccessor fileSystemAccessor)
        {
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public IInterviewExpressionStateV2 GetExpressionState(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFile = this.questionnareAssemblyFileAccessor.GetFullPathToAssembly(questionnaireId, questionnaireVersion);

            if (!fileSystemAccessor.IsFileExists(assemblyFile))
            {
                Logger.Fatal(String.Format("Assembly was not found. Questionnaire={0}, version={1}, search={2}", 
                    questionnaireId, questionnaireVersion, assemblyFile));
                throw new InterviewException("Interview loading error. Code EC0003");
            }

            try
            {
                //path is cached
                //if assembly was loaded from this path it won't be loaded again 
                var compiledAssembly = fileSystemAccessor.LoadAssembly(assemblyFile);

                TypeInfo interviewExpressionStateTypeInfo = compiledAssembly.DefinedTypes.
                    SingleOrDefault(x => !(x.IsAbstract || x.IsGenericTypeDefinition || x.IsInterface) && x.ImplementedInterfaces.Contains(typeof (IInterviewExpressionState)));

                if (interviewExpressionStateTypeInfo == null)
                    throw new Exception("Type implementing IInterviewExpressionState was not found");

                Type interviewExpressionStateType = interviewExpressionStateTypeInfo.AsType();
                try
                {
                    var interviewExpressionState = Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState;

                    var v2 = interviewExpressionState as IInterviewExpressionStateV2;
                    if (v2 != null)
                        return v2;

                    return Adapt(interviewExpressionState);
                }
                catch (Exception e)
                {
                    Logger.Fatal("Error on activating interview expression state. Cannot cast to created object to IInterviewExpressionState", e);
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal(String.Format("Error on assembly loading for id={0} version={1}", questionnaireId, questionnaireVersion), exception);
                if (exception.InnerException != null)
                    Logger.Fatal("Error on assembly loading (inner)", exception.InnerException);

                //hide original one
                throw new InterviewException("Interview loading error. Code EC0001");
            }
        }

        protected IInterviewExpressionStateV2 Adapt(IInterviewExpressionState interviewExpressionState)
        {
            return new InterviewExpressionStateAdapter(interviewExpressionState);
        }
    }

    internal class InterviewExpressionStateAdapter : IInterviewExpressionState, IInterviewExpressionStateV2
    {
        private readonly IInterviewExpressionState interviewExpressionState;

        public InterviewExpressionStateAdapter(IInterviewExpressionState interviewExpressionState)
        {
            this.interviewExpressionState = interviewExpressionState;
        }

        public void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long? answer)
        {
            interviewExpressionState.UpdateNumericIntegerAnswer(questionId, rosterVector, answer);
        }

        public void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double? answer)
        {
            interviewExpressionState.UpdateNumericRealAnswer(questionId, rosterVector, answer);
        }

        public void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime? answer)
        {
            interviewExpressionState.UpdateDateAnswer(questionId, rosterVector, answer);
        }

        public void UpdateMediaAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            interviewExpressionState.UpdateMediaAnswer(questionId, rosterVector, answer);
        }

        public void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            interviewExpressionState.UpdateTextAnswer(questionId, rosterVector, answer);
        }

        public void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            interviewExpressionState.UpdateQrBarcodeAnswer(questionId, rosterVector, answer);
        }

        public void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal? answer)
        {
            interviewExpressionState.UpdateSingleOptionAnswer(questionId, rosterVector, answer);
        }

        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            interviewExpressionState.UpdateMultiOptionAnswer(questionId, rosterVector, answer);
        }

        public void UpdateGeoLocationAnswer(Guid questionId, decimal[] propagationVector, double latitude, double longitude,
            double accuracy, double altitude)
        {
            interviewExpressionState.UpdateGeoLocationAnswer(questionId, propagationVector, latitude, longitude, accuracy, altitude);
        }

        public void UpdateTextListAnswer(Guid questionId, decimal[] propagationVector, Tuple<decimal, string>[] answers)
        {
            interviewExpressionState.UpdateTextListAnswer(questionId, propagationVector, answers); 
        }

        public void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[] selectedPropagationVector)
        {
            interviewExpressionState.UpdateLinkedSingleOptionAnswer(questionId, propagationVector, selectedPropagationVector); 
        }

        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[][] selectedPropagationVectors)
        {
            interviewExpressionState.UpdateLinkedMultiOptionAnswer(questionId, propagationVector, selectedPropagationVectors); 
        }

        public void DeclareAnswersInvalid(IEnumerable<Identity> invalidQuestions)
        {
            interviewExpressionState.DeclareAnswersInvalid(invalidQuestions); 
        }

        public void DeclareAnswersValid(IEnumerable<Identity> validQuestions)
        {
            interviewExpressionState.DeclareAnswersValid(validQuestions); 
        }

        public void DisableGroups(IEnumerable<Identity> groupsToDisable)
        {
            interviewExpressionState.DisableGroups(groupsToDisable); 
        }

        public void EnableGroups(IEnumerable<Identity> groupsToEnable)
        {
            interviewExpressionState.EnableGroups(groupsToEnable); 
        }

        public void DisableQuestions(IEnumerable<Identity> questionsToDisable)
        {
            interviewExpressionState.DisableQuestions(questionsToDisable); 
        }

        public void EnableQuestions(IEnumerable<Identity> questionsToEnable)
        {
            interviewExpressionState.EnableQuestions(questionsToEnable); 
        }

        public void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            interviewExpressionState.AddRoster(rosterId, outerRosterVector, rosterInstanceId, sortIndex); 
        }

        public void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId)
        {
            interviewExpressionState.RemoveRoster(rosterId, rosterVector, rosterInstanceId); 
        }

        public ValidityChanges ProcessValidationExpressions()
        {
            return interviewExpressionState.ProcessValidationExpressions(); 
        }

        public EnablementChanges ProcessEnablementConditions()
        {
            return interviewExpressionState.ProcessEnablementConditions(); 
        }

        public void SaveAllCurrentStatesAsPrevious()
        {
            interviewExpressionState.SaveAllCurrentStatesAsPrevious(); 
        }

        public IInterviewExpressionState Clone()
        {
            return interviewExpressionState.Clone(); 
        }

        public void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle)
        {
        }

        public IInterviewExpressionStateV2 CloneV2()
        {
            return new InterviewExpressionStateAdapter(interviewExpressionState);
        }
    }
}
