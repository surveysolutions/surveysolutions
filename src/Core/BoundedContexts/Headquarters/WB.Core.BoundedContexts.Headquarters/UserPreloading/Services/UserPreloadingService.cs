using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private readonly UserPrelodingState[] statesInWhichProcessCantBeDelete = new[]
        {UserPrelodingState.Validating, UserPrelodingState.CreatingUsers};

        private readonly string[] dataColumnNames = new[] {"Login", "Password", "Email", "FullName", "PhoneNumber", "Role", "Supervisor"};

        public UserPreloadingService(IPlainStorageAccessor<UserPreloadingProcess> userPreloadingProcessStorage,
            IRecordsAccessorFactory recordsAccessorFactory)
        {
            this.userPreloadingProcessStorage = userPreloadingProcessStorage;
            this.recordsAccessorFactory = recordsAccessorFactory;
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

            var records=new List<string[]>();
            try
            {
                records = recordsAccessorFactory.CreateRecordsAccessor(data,
                    ExportFileSettings.SeparatorOfExportedDataFile.ToString()).Records.ToList();
            }
            catch (Exception e)
            {
                throw new UserPreloadingException(e.Message, e);
            }

            string[] header = null;
            foreach (var record in records)
            {
                if (header == null)
                {
                    header = record.Select(r => r.ToLower()).ToArray();
                    ThrowIfFileStructureIsInvalid(header);
                    continue;
                }
                var dataRecord = new UserPreloadingDataRecord();

                foreach (var dataColumnName in dataColumnNames)
                {
                    SetDataRecordProperty(dataRecord, dataColumnName, record, header);
                }

                preloadingProcess.UserPrelodingData.Add(dataRecord);
            }

            preloadingProcess.RecordsCount = preloadingProcess.UserPrelodingData.Count;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);

            return preloadingProcessId;
        }

        private void ThrowIfFileStructureIsInvalid(string[] header)
        {
            var dataColumnNamesLowerCase = dataColumnNames.Select(r => r.ToLower()).ToArray();
            var invalidColumnNames=new List<string>();
            foreach (var columnName in header)
            {
                if (!dataColumnNamesLowerCase.Contains(columnName))
                    invalidColumnNames.Add(columnName);
            }
            if (invalidColumnNames.Any())
                throw new UserPreloadingException(String.Format("file contains following invalid columns: {0}",
                    string.Join(",", invalidColumnNames)));
        }

        private void SetDataRecordProperty(UserPreloadingDataRecord dataRecord, string dataColumnName, string[] fileRecord, string[] header)
        {
            var dataColumnNameLowerCase = dataColumnName.ToLower();
            if (!header.Contains(dataColumnNameLowerCase))
                return;

            var indexOfColumn = Array.IndexOf(header, dataColumnNameLowerCase);
            var cellValue = (fileRecord[indexOfColumn]??"").Trim();

            PropertyInfo property = typeof (UserPreloadingDataRecord).GetProperty(dataColumnName,
                BindingFlags.Public | BindingFlags.Instance);

            if (property != null && property.CanWrite)
            {
                property.SetValue(dataRecord, cellValue, null);
            }
        }

        public void FinishValidationProcess(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            ThrowIfStateDoesntMatch(preloadingProcess, UserPrelodingState.Validating);

            preloadingProcess.State = UserPrelodingState.Validated;
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
                        "user preloading process with id '{0}' can't be finished because only {1} created out of {2}",
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
                        "user preloading process with id '{0}'has {1} error(s).",
                        preloadingProcess.UserPreloadingProcessId, preloadingProcess.VerificationErrors.Count));

            preloadingProcess.State = UserPrelodingState.ReadyForUserCreation;
            preloadingProcess.LastUpdateDate = DateTime.Now;

            userPreloadingProcessStorage.Store(preloadingProcess, preloadingProcessId);
        }

        public void DeletePreloadingProcess(string preloadingProcessId)
        {
            var preloadingProcess = this.GetUserPreloadingProcessAndThrowIfMissing(preloadingProcessId);

            if(statesInWhichProcessCantBeDelete.Contains(preloadingProcess.State))
                throw new ArgumentException(
                   String.Format(
                       "user preloading process with id '{0}' is in state '{1}' can't be deleted",
                       preloadingProcessId, preloadingProcess.State));

            userPreloadingProcessStorage.Remove(preloadingProcessId);
        }

        public string DeQueuePreloadingProcessIdReadyToBeValidated()
        {
            var process =
                userPreloadingProcessStorage.Query(
                    _ =>
                        _.Where(p => p.State == UserPrelodingState.ReadyForValidation)
                            .OrderBy(p => p.LastUpdateDate)
                            .FirstOrDefault());

            if (process == null)
                return null;

            process.State = UserPrelodingState.Validating;
            process.LastUpdateDate = DateTime.Now;

            userPreloadingProcessStorage.Store(process, process.UserPreloadingProcessId);
            return process.UserPreloadingProcessId;
        }

        public string DeQueuePreloadingProcessIdReadyToCreateUsers()
        {
            var process=
                userPreloadingProcessStorage.Query(
                    _ =>
                        _.Where(p => p.State == UserPrelodingState.ReadyForUserCreation)
                            .OrderBy(p => p.LastUpdateDate)
                            .FirstOrDefault());
            if (process == null)
                return null;

            process.State = UserPrelodingState.CreatingUsers;
            process.LastUpdateDate = DateTime.Now;

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

        public void IncreaseCountCreateUsers(string preloadingProcessId)
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
            return dataColumnNames;
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
    }
}