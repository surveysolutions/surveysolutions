using System;
using System.IO;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public interface IUserPreloadingService
    {
        string CreateUserPreloadingProcess(Stream data, string fileName);

        void DeletePreloadingProcess(string preloadingProcessId);

        void FinishValidationProcess(string preloadingProcessId);

        void FinishPreloadingProcess(string preloadingProcessId);

        void FinishPreloadingProcessWithError(string preloadingProcessId, string errorMessage);

        void EnqueueForValidation(string preloadingProcessId);
        void EnqueueForUserCreation(string preloadingProcessId);

        string DeQueuePreloadingProcessIdReadyToBeValidated();
        string DeQueuePreloadingProcessIdReadyToCreateUsers();
        
        UserPreloadingProcess[] GetPreloadingProcesses();
        UserPreloadingProcess GetPreloadingProcesseDetails(string preloadingProcessId);

        void PushVerificationError(string preloadingProcessId, string code, int rowNumber, string columnName, string cellValue);
        void IncreaseCountCreateUsers(string preloadingProcessId);
        string[] GetAvaliableDataColumnNames();
    }
}