using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class UserPreloadingService : IUserPreloadingService
    {
        private readonly IPlainStorageAccessor<UserPreloadingProcess> userPreloadingProcessStorage;
        private readonly IRecordsAccessorFactory recordsAccessorFactory;
        private readonly UserPreloadingSettings userPreloadingSettings;

        readonly Dictionary<string, Action<UserPreloadingDataRecord, string>> dataColumnNamesMappedOnRecordSetter;

        public UserPreloadingService(IPlainStorageAccessor<UserPreloadingProcess> userPreloadingProcessStorage,
            IRecordsAccessorFactory recordsAccessorFactory, UserPreloadingSettings userPreloadingSettings)
        {
            this.userPreloadingProcessStorage = userPreloadingProcessStorage;
            this.recordsAccessorFactory = recordsAccessorFactory;
            this.userPreloadingSettings = userPreloadingSettings;

            this.dataColumnNamesMappedOnRecordSetter = new Dictionary<string, Action<UserPreloadingDataRecord, string>>
            {
                {"Login", (r, v) => r.Login = v},
                {"Password", (r, v) => r.Password = v},
                {"Email", (r, v) => r.Email = v},
                {"FullName", (r, v) => r.FullName = v},
                {"PhoneNumber", (r, v) => r.PhoneNumber = v},
                {"Role", (r, v) => r.Role = v},
                {"Supervisor", (r, v) => r.Supervisor = v}
            };
        }

        public string CreateUserPreloadingProcess(Stream data, string fileName)
        {
            var preloadingProcessId = Guid.NewGuid().FormatGuid();
            var preloadingProcess = new UserPreloadingProcess()
            {
                UserPreloadingProcessId = preloadingProcessId,
                FileName = fileName,
                FileSize = data.Length,
                State = UserPrelodingState.Uploaded,
                UploadDate = DateTime.Now
            };

            preloadingProcess.LastUpdateDate = DateTime.Now;

            var records = new List<string[]>();
            try
            {
                records = recordsAccessorFactory.CreateRecordsAccessor(data,
                    ExportFileSettings.SeparatorOfExportedDataFile.ToString()).Records.ToList();
            }
            catch (Exception e)
            {
                throw new UserPreloadingException(e.Message, e);
            }

            if (records.Count - 1 > userPreloadingSettings.MaxAllowedRecordNumber)
                throw new UserPreloadingException(
                    String.Format(UserPreloadingServiceMessages.TheDatasetMaxRecordNumberReachedFormat,
                        records.Count - 1, userPreloadingSettings.MaxAllowedRecordNumber));

            string[] header = null;
            foreach (var record in records)
            {
                if (header == null)
                {
                    header = record.Select(r => r.ToLower()).ToArray();
                    ThrowIfFileStructureIsInvalid(header, fileName);
                    continue;
                }
                var dataRecord = new UserPreloadingDataRecord();

                foreach (var dataColumnNameMappedOnRecordSetter in this.dataColumnNamesMappedOnRecordSetter)
                {
                    var indexOfColumn = Array.IndexOf(header, dataColumnNameMappedOnRecordSetter.Key.ToLower());
                    if (indexOfColumn < 0)
                        continue;

                    var cellValue = (record[indexOfColumn] ?? "").Trim();

                    var propertySetter = dataColumnNameMappedOnRecordSetter.Value;
                    propertySetter(dataRecord, cellValue);
                }

                preloadingProcess.UserPrelodingData.Add(dataRecord);
            }

            preloadingProcess.RecordsCount = preloadingProcess.UserPrelodingData.Count;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);

            return preloadingProcessId;
        }

        void ThrowIfFileStructureIsInvalid(string[] header, string fileName)
        {
            var dataColumnNamesLowerCase = this.dataColumnNamesMappedOnRecordSetter.Keys.Select(r => r.ToLower()).ToArray();
            var invalidColumnNames = new List<string>();
            foreach (var columnName in header)
            {
                if (!dataColumnNamesLowerCase.Contains(columnName))
                    invalidColumnNames.Add(columnName);
            }
            if (invalidColumnNames.Any())
                throw new UserPreloadingException(String.Format(UserPreloadingServiceMessages.FileColumnsCantBeMappedFormat,
                    fileName, string.Join(",", invalidColumnNames)));
        }

        public void FinishValidationProcess(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Validating);

            preloadingProcess.State = UserPrelodingState.Validated;
            preloadingProcess.LastUpdateDate = DateTime.Now;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void FinishValidationProcessWithError(string preloadingProcessId, string errorMessage)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Validating);

            preloadingProcess.State = UserPrelodingState.ValidationFinishedWithError;
            preloadingProcess.ErrorMessage = errorMessage;
            preloadingProcess.LastUpdateDate = DateTime.Now;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void FinishPreloadingProcess(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.CreatingUsers);

            if (preloadingProcess.CreatedUsersCount != preloadingProcess.RecordsCount)
                throw new UserPreloadingException(
                    String.Format(
                        UserPreloadingServiceMessages.userPreloadingProcessCantBeFinishedFormat,
                        preloadingProcess.UserPreloadingProcessId, preloadingProcess.CreatedUsersCount, preloadingProcess.RecordsCount));

            preloadingProcess.State = UserPrelodingState.Finished;
            preloadingProcess.LastUpdateDate = DateTime.Now;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void FinishPreloadingProcessWithError(string preloadingProcessId, string errorMessage)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.CreatingUsers);

            preloadingProcess.State = UserPrelodingState.FinishedWithError;
            preloadingProcess.ErrorMessage = errorMessage;
            preloadingProcess.LastUpdateDate = DateTime.Now;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void EnqueueForValidation(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Uploaded);

            preloadingProcess.State = UserPrelodingState.ReadyForValidation;
            preloadingProcess.LastUpdateDate = DateTime.Now;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void EnqueueForUserCreation(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Validated);

            if (preloadingProcess.VerificationErrors.Any())
                throw new UserPreloadingException(
                    String.Format(
                        UserPreloadingServiceMessages.UserPreloadingProcessWithIdHasErrorsFormat,
                        preloadingProcess.UserPreloadingProcessId, preloadingProcess.VerificationErrors.Count));

            preloadingProcess.State = UserPrelodingState.ReadyForUserCreation;
            preloadingProcess.LastUpdateDate = DateTime.Now;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void DeletePreloadingProcess(string preloadingProcessId)
        {
            userPreloadingProcessStorage.Remove(preloadingProcessId);
        }

        public string DeQueuePreloadingProcessIdReadyToBeValidated()
        {
            var process = GetOldestPreloadingProcessInState(UserPrelodingState.ReadyForValidation);
            if (process == null)
                return null;

            process.State = UserPrelodingState.Validating;
            process.LastUpdateDate = DateTime.Now;
            process.ValidationStartDate = DateTime.Now;

            userPreloadingProcessStorage.Store(process, process.UserPreloadingProcessId);
            return process.UserPreloadingProcessId;
        }

        public string DeQueuePreloadingProcessIdReadyToCreateUsers()
        {
            var process = GetOldestPreloadingProcessInState(UserPrelodingState.ReadyForUserCreation);
            if (process == null)
                return null;

            process.State = UserPrelodingState.CreatingUsers;
            process.LastUpdateDate = DateTime.Now;
            process.CreationStartDate = DateTime.Now;

            userPreloadingProcessStorage.Store(process, process.UserPreloadingProcessId);
            return process.UserPreloadingProcessId;
        }

        public UserPreloadingProcess[] GetPreloadingProcesses()
        {
            return
                userPreloadingProcessStorage.Query(
                    _ => _.OrderBy(p => p.LastUpdateDate).ToArray());
        }

        public UserPreloadingProcess GetPreloadingProcesseDetails(string preloadingProcessId)
        {
            return userPreloadingProcessStorage.GetById(preloadingProcessId);
        }

        public void PushVerificationError(string preloadingProcessId, string code, int rowNumber, string columnName,
            string cellValue)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Validating);

            if (preloadingProcess.VerificationErrors.Count >
                userPreloadingSettings.NumberOfValidationErrorsBeforeStopValidation)
            {
                throw new UserPreloadingException(
                    string.Format(UserPreloadingServiceMessages.MaxNumberOfValidationErrorsHaveBeenReachedFormat,
                        userPreloadingSettings.NumberOfValidationErrorsBeforeStopValidation));
            }

            preloadingProcess.VerificationErrors.Add(new UserPreloadingVerificationError()
            {
                CellValue = cellValue,
                Code = code,
                ColumnName = columnName,
                RowNumber = rowNumber
            });
            preloadingProcess.LastUpdateDate = DateTime.Now;
            
            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void UpdateVerificationProgressInPercents(string preloadingProcessId, int percents)
        {
            if(percents<0 ||percents>100)
                throw new UserPreloadingException(String.Format(UserPreloadingServiceMessages.validationProgressInPercentsCantBeNegativeOrGreaterThen100Format, percents));

            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Validating);

            preloadingProcess.LastUpdateDate=DateTime.Now;
            preloadingProcess.VerificationProgressInPercents = percents;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void IncrementCreatedUsersCount(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.CreatingUsers);

            if (preloadingProcess.RecordsCount == preloadingProcess.CreatedUsersCount)
                throw new UserPreloadingException(
                    String.Format(
                        "user preloading process with id '{0}' can't create more users then {1}",
                        preloadingProcessId, preloadingProcess.RecordsCount));

            preloadingProcess.CreatedUsersCount++;
            preloadingProcess.LastUpdateDate = DateTime.Now;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public string[] GetAvaliableDataColumnNames()
        {
            return this.dataColumnNamesMappedOnRecordSetter.Keys.ToArray();
        }

        public UserRoles GetUserRoleFromDataRecord(UserPreloadingDataRecord dataRecord)
        {
            if (dataRecord.Role.ToLower() == "supervisor")
                return UserRoles.Supervisor;
            if (dataRecord.Role.ToLower() == "interviewer")
                return UserRoles.Operator;

            return UserRoles.Undefined;
        }

        private UserPreloadingProcess GetUserPreloadingProcessAndThrowIfMissing(string preloadingProcessId)
        {
            var preloadingProcess = this.userPreloadingProcessStorage.GetById(preloadingProcessId);
            if (preloadingProcess == null)
                throw new UserPreloadingException(String.Format("user preloading process with id '{0}' is missing",
                    preloadingProcessId));
            return preloadingProcess;
        }

        private void ThrowIfStateDoesntMatch(UserPreloadingProcess preloadingProcess, UserPrelodingState state)
        {
            if (preloadingProcess.State != state)
                throw new UserPreloadingException(
                    String.Format(
                        "user preloading process with id '{0}' is in state '{1}', but must be in state '{2}'",
                        preloadingProcess.UserPreloadingProcessId, preloadingProcess.State, state));
        }

        private UserPreloadingProcess GetOldestPreloadingProcessInState(UserPrelodingState state)
        {
            return
                userPreloadingProcessStorage.Query(
                    _ =>
                        _.Where(p => p.State == state)
                            .OrderBy(p => p.LastUpdateDate)
                            .FirstOrDefault());
        }
    }
}