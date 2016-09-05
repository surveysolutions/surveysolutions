using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

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
                {"login", (r, v) => r.Login = v},
                {"password", (r, v) => r.Password = v},
                {"email", (r, v) => r.Email = v},
                {"fullname", (r, v) => r.FullName = v},
                {"phonenumber", (r, v) => r.PhoneNumber = v},
                {"role", (r, v) => r.Role = v},
                {"supervisor", (r, v) => r.Supervisor = v}
            };
        }

        public string CreateUserPreloadingProcess(Stream data, string fileName)
        {
            var preloadingProcessId = Guid.NewGuid().FormatGuid();
            var preloadingProcess = new UserPreloadingProcess
            {
                UserPreloadingProcessId = preloadingProcessId,
                FileName = fileName,
                FileSize = data.Length,
                State = UserPrelodingState.Uploaded,
                UploadDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow
            };

            var records = new List<string[]>();
            try
            {
                var recordsAccessor = this.recordsAccessorFactory.CreateRecordsAccessor(data, ExportFileSettings.SeparatorOfExportedDataFile.ToString());
                records = recordsAccessor.Records.ToList();
            }
            catch (Exception e)
            {
                throw new UserPreloadingException(e.Message, e);
            }

            if (records.Count - 1 > userPreloadingSettings.MaxAllowedRecordNumber)
            {
                var exceptionMessage = UserPreloadingServiceMessages.TheDatasetMaxRecordNumberReachedFormat
                                                                    .FormatString(records.Count - 1, this.userPreloadingSettings.MaxAllowedRecordNumber);
                throw new UserPreloadingException(exceptionMessage);
            }

            string[] headerRow = null;
            foreach (string[] record in records)
            {
                if (headerRow == null)
                {
                    headerRow = record.Select(r => r.ToLower()).ToArray();
                    ThrowIfFileStructureIsInvalid(headerRow, fileName);
                    continue;
                }

                var dataRecord = new UserPreloadingDataRecord();

                for (int i = 0; i < headerRow.Length; i++)
                {
                    var columnName = headerRow[i];

                    if(!dataColumnNamesMappedOnRecordSetter.ContainsKey(columnName))
                        continue;

                    var recordSetter = dataColumnNamesMappedOnRecordSetter[columnName];

                    var cellValue = (record[i] ?? "").Trim();

                    var propertySetter = recordSetter;

                    propertySetter(dataRecord, cellValue);
                }

                preloadingProcess.UserPrelodingData.Add(dataRecord);
            }

            preloadingProcess.RecordsCount = preloadingProcess.UserPrelodingData.Count;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);

            return preloadingProcessId;
        }

        public void FinishValidationProcess(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Validating);

            preloadingProcess.State = UserPrelodingState.Validated;
            preloadingProcess.LastUpdateDate = DateTime.UtcNow;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void FinishValidationProcessWithError(string preloadingProcessId, string errorMessage)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Validating);

            preloadingProcess.State = UserPrelodingState.ValidationFinishedWithError;
            preloadingProcess.ErrorMessage = errorMessage;
            preloadingProcess.LastUpdateDate = DateTime.UtcNow;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void FinishPreloadingProcess(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.CreatingUsers);

            if (preloadingProcess.CreatedUsersCount != preloadingProcess.RecordsCount)
            {
                var message = String.Format(UserPreloadingServiceMessages.userPreloadingProcessCantBeFinishedFormat,
                    preloadingProcess.UserPreloadingProcessId, 
                    preloadingProcess.CreatedUsersCount, 
                    preloadingProcess.RecordsCount);
                throw new UserPreloadingException(message);
            }

            preloadingProcess.State = UserPrelodingState.Finished;
            preloadingProcess.LastUpdateDate = DateTime.UtcNow;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void FinishPreloadingProcessWithError(string preloadingProcessId, string errorMessage)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.CreatingUsers);

            preloadingProcess.State = UserPrelodingState.FinishedWithError;
            preloadingProcess.ErrorMessage = errorMessage;
            preloadingProcess.LastUpdateDate = DateTime.UtcNow;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void EnqueueForValidation(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Uploaded);

            preloadingProcess.State = UserPrelodingState.ReadyForValidation;
            preloadingProcess.LastUpdateDate = DateTime.UtcNow;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void EnqueueForUserCreation(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Validated);

            if (preloadingProcess.VerificationErrors.Any())
            {
                var message = String.Format(UserPreloadingServiceMessages.UserPreloadingProcessWithIdHasErrorsFormat,
                    preloadingProcess.UserPreloadingProcessId, 
                    preloadingProcess.VerificationErrors.Count);
                throw new UserPreloadingException(message);
            }

            preloadingProcess.State = UserPrelodingState.ReadyForUserCreation;
            preloadingProcess.LastUpdateDate = DateTime.UtcNow;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void DeletePreloadingProcess(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);
            var statesWhichNotAllowingDelete = new[] {UserPrelodingState.Validating, UserPrelodingState.CreatingUsers};
            if (statesWhichNotAllowingDelete.Contains(preloadingProcess.State))
            {
                throw new InvalidOperationException(
                    String.Format(
                        UserPreloadingServiceMessages.UserPreloadingProcessIsInStateButMustBeInStateOneOfTheFollowingStatesFormat,
                        preloadingProcess.FileName, preloadingProcess.State, string.Join(", ", statesWhichNotAllowingDelete.Select(s=>s.ToString()))));
            }

            userPreloadingProcessStorage.Remove(preloadingProcessId);
        }

        public string DeQueuePreloadingProcessIdReadyToBeValidated()
        {
            var process = GetOldestPreloadingProcessInState(UserPrelodingState.ReadyForValidation);
            if (process == null)
                return null;

            process.State = UserPrelodingState.Validating;
            process.LastUpdateDate = DateTime.UtcNow;
            process.ValidationStartDate = DateTime.UtcNow;

            userPreloadingProcessStorage.Store(process, process.UserPreloadingProcessId);
            return process.UserPreloadingProcessId;
        }

        public string DeQueuePreloadingProcessIdReadyToCreateUsers()
        {
            var process = GetOldestPreloadingProcessInState(UserPrelodingState.ReadyForUserCreation);
            if (process == null)
                return null;

            process.State = UserPrelodingState.CreatingUsers;
            process.LastUpdateDate = DateTime.UtcNow;
            process.CreationStartDate = DateTime.UtcNow;

            userPreloadingProcessStorage.Store(process, process.UserPreloadingProcessId);
            return process.UserPreloadingProcessId;
        }

        public UserPreloadingProcess[] GetPreloadingProcesses()
        {
            return userPreloadingProcessStorage.Query(_ => _.OrderBy(p => p.LastUpdateDate).ToArray());
        }

        public UserPreloadingProcess GetPreloadingProcesseDetails(string preloadingProcessId)
        {
            return userPreloadingProcessStorage.GetById(preloadingProcessId);
        }

        public void PushVerificationError(string preloadingProcessId, 
            string code, 
            int rowNumber, 
            string columnName,
            string cellValue)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Validating);

            if (preloadingProcess.VerificationErrors.Count >= userPreloadingSettings.NumberOfValidationErrorsBeforeStopValidation)
            {
                var message = string.Format(UserPreloadingServiceMessages.MaxNumberOfValidationErrorsHaveBeenReachedFormat,
                    this.userPreloadingSettings.NumberOfValidationErrorsBeforeStopValidation);
                throw new UserPreloadingException(message);
            }

            preloadingProcess.VerificationErrors.Add(new UserPreloadingVerificationError()
            {
                CellValue = cellValue,
                Code = code,
                ColumnName = columnName,
                RowNumber = rowNumber
            });
            preloadingProcess.LastUpdateDate = DateTime.UtcNow;
            
            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void UpdateVerificationProgressInPercents(string preloadingProcessId, int percents)
        {
            if (percents < 0 || percents > 100)
            {
                var message = String.Format(UserPreloadingServiceMessages.validationProgressInPercentsCantBeNegativeOrGreaterThen100Format, percents);
                throw new ArgumentOutOfRangeException(message);
            }

            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Validating);

            preloadingProcess.LastUpdateDate = DateTime.UtcNow;
            preloadingProcess.VerificationProgressInPercents = percents;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void IncrementCreatedUsersCount(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.CreatingUsers);

            if (preloadingProcess.RecordsCount == preloadingProcess.CreatedUsersCount)
            {
                var message = String.Format(UserPreloadingServiceMessages.UserPreloadingProcessWithIdCantCreateMoreUsersFormat,
                    preloadingProcessId, 
                    preloadingProcess.RecordsCount);
                throw new UserPreloadingException(message);
            }

            preloadingProcess.CreatedUsersCount++;
            preloadingProcess.LastUpdateDate = DateTime.UtcNow;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public string[] GetAvaliableDataColumnNames()
        {
            return this.dataColumnNamesMappedOnRecordSetter.Keys.ToArray();
        }

        public UserRoles GetUserRoleFromDataRecord(UserPreloadingDataRecord dataRecord)
        {
            if ("supervisor".Equals(dataRecord.Role,StringComparison.InvariantCultureIgnoreCase))
                return UserRoles.Supervisor;
            if ("interviewer".Equals(dataRecord.Role,StringComparison.InvariantCultureIgnoreCase))
                return UserRoles.Interviewer;

            return 0;
        }

        private UserPreloadingProcess GetUserPreloadingProcessAndThrowIfMissing(string preloadingProcessId)
        {
            var preloadingProcess = this.userPreloadingProcessStorage.GetById(preloadingProcessId);
            if (preloadingProcess == null)
            {
                var message = String.Format(UserPreloadingServiceMessages.UserPreloadingProcessWithIdIisMissingFormat, preloadingProcessId);
                throw new KeyNotFoundException(message);
            }
            return preloadingProcess;
        }

        private void ThrowIfStateDoesntMatch(UserPreloadingProcess preloadingProcess, UserPrelodingState state)
        {
            if (preloadingProcess.State != state)
                throw new InvalidOperationException(
                    String.Format(
                        UserPreloadingServiceMessages.UserPreloadingProcessWithIdInInvalidStateFormat,
                        preloadingProcess.UserPreloadingProcessId, preloadingProcess.State, state));
        }

        private UserPreloadingProcess GetOldestPreloadingProcessInState(UserPrelodingState state)
        {
            return userPreloadingProcessStorage.Query(_ => _.Where(p => p.State == state)
                                                            .OrderBy(p => p.LastUpdateDate)
                                                            .FirstOrDefault());
        }

        private void ThrowIfFileStructureIsInvalid(string[] header, string fileName)
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
    }
}