using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class UserPreloadingService : IUserPreloadingService
    {
        private readonly UserPreloadingSettings userPreloadingSettings;
        private readonly ICsvReader csvReader;
        private readonly IPlainStorageAccessor<UserPreloadingProcess> importUsersProcessRepository;
        private readonly IUserRepository userStorage;
        private readonly IUserPreloadingVerifier userImportVerifier;

        private readonly string importUsersProcessId = Guid.Empty.ToString();
        private readonly Guid supervisorRoleId = UserRoles.Supervisor.ToUserId();
        private readonly Guid interviewerRoleId = UserRoles.Interviewer.ToUserId();

        public UserPreloadingService(
            UserPreloadingSettings userPreloadingSettings, 
            ICsvReader csvReader,
            IPlainStorageAccessor<UserPreloadingProcess> importUsersProcessRepository,
            IUserRepository userStorage,
            IUserPreloadingVerifier userImportVerifier)
        {
            this.userPreloadingSettings = userPreloadingSettings;
            this.csvReader = csvReader;
            this.importUsersProcessRepository = importUsersProcessRepository;
            this.userStorage = userStorage;
            this.userImportVerifier = userImportVerifier;
        }

        public IEnumerable<UserPreloadingVerificationError> VerifyAndSaveIfNoErrors(byte[] data, string fileName)
        {
            var csvDelimiter = ExportFileSettings.DataFileSeparator.ToString();

            var requiredColumns = this.GetRequiredUserProperties();

            var columns = this.csvReader.ReadHeader(new MemoryStream(data), csvDelimiter)
                .Select(x => x.ToLower()).ToArray();

            var missingColumns = requiredColumns.Where(x => !columns.Contains(x));

            if (missingColumns.Any())
                throw new UserPreloadingException(string.Format(UserPreloadingServiceMessages.FileColumnsMissingFormat,
                        fileName, string.Join(", ", missingColumns)));

            var usersToImport = new List<UserPreloadingDataRecord>();

            var allInterviewersAndSupervisors = this.userStorage.Users.Select(x => new UserToValidate
            {
                UserId = x.Id,
                UserName = x.UserName,
                IsArchived = x.IsArchived,
                SupervisorId = x.Profile.SupervisorId,
                IsSupervisor = x.Roles.Any(role => role.RoleId == supervisorRoleId),
                IsInterviewer = x.Roles.Any(role => role.RoleId == interviewerRoleId)
            }).ToArray();

            var validations = this.userImportVerifier.GetEachUserValidations(allInterviewersAndSupervisors);

            foreach (var userToImport in this.csvReader.ReadAll<UserPreloadingDataRecord>(new MemoryStream(data), csvDelimiter))
            {
                usersToImport.Add(userToImport);

                foreach (var validator in validations)
                {
                    if (validator.ValidationFunction(userToImport))
                        yield return ToVerificationError(validator, userToImport, usersToImport.Count);
                }

                if (usersToImport.Count > userPreloadingSettings.MaxAllowedRecordNumber)
                    throw new UserPreloadingException(string.Format(UserPreloadingServiceMessages.TheDatasetMaxRecordNumberReachedFormat,
                            this.userPreloadingSettings.MaxAllowedRecordNumber));
            }

            validations = this.userImportVerifier.GetAllUsersValidations(allInterviewersAndSupervisors, usersToImport);

            for (int userIndex = 0; userIndex < usersToImport.Count; userIndex++)
            {
                var userToImport = usersToImport[userIndex];

                foreach (var validator in validations)
                {
                    if (validator.ValidationFunction(userToImport))
                        yield return ToVerificationError(validator, userToImport, userIndex + 1);
                }
            }

            this.Save(fileName, usersToImport);
        }

        private string[] GetRequiredUserProperties() => this.GetUserProperties().Take(4).ToArray();

        public string[] GetUserProperties() => new[]
        {
            nameof(UserPreloadingDataRecord.Login), nameof(UserPreloadingDataRecord.Password),
            nameof(UserPreloadingDataRecord.Role), nameof(UserPreloadingDataRecord.Supervisor),
            nameof(UserPreloadingDataRecord.FullName), nameof(UserPreloadingDataRecord.Email),
            nameof(UserPreloadingDataRecord.PhoneNumber)
        }.Select(x => x.ToLower()).ToArray();

        private void Save(string fileName, IList<UserPreloadingDataRecord> usersToImport)
            => this.importUsersProcessRepository.Store(new UserPreloadingProcess
            {
                UserPreloadingProcessId = importUsersProcessId,
                FileName = fileName,
                RecordsCount = usersToImport.Count,
                InterviewersCount = usersToImport.Count(x => x.Role == "interviewer"),
                SupervisorsCount = usersToImport.Count(x => x.Role == "supervisor"),
                UserPrelodingData = usersToImport.OrderByDescending(x => x.Role).ToList()
            }, null);

        public void Import()
        {
            throw new NotImplementedException();
        }

        public UsersImportStatus GetImportStatus()
        {
            var process = this.importUsersProcessRepository.GetById(importUsersProcessId);

            return new UsersImportStatus
            {
                IsInProgress = process != null,
                TotalUsersToImport = process?.RecordsCount ?? 0,
                UsersInQueue = process?.UserPrelodingData?.Count ?? 0,
                FileName = process?.FileName
            };
        }

        private static UserPreloadingVerificationError ToVerificationError(PreloadedDataValidator validator,
            UserPreloadingDataRecord userToImport, int userIndex)
            => new UserPreloadingVerificationError
            {
                CellValue = validator.ValueSelector.Compile()(userToImport),
                Code = validator.Code,
                ColumnName = ((MemberExpression)validator.ValueSelector.Body).Member.Name,
                RowNumber = userIndex + 1 /*header item*/
            };
    }
}