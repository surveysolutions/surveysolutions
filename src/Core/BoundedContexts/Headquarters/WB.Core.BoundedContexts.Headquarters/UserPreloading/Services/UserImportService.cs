using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class UserImportService : IUserImportService
    {
        private readonly UserPreloadingSettings userPreloadingSettings;
        private readonly ICsvReader csvReader;
        private readonly IPlainStorageAccessor<UsersImportProcess> importUsersProcessRepository;
        private readonly IPlainStorageAccessor<UserToImport> importUsersRepository;
        private readonly IUserRepository userStorage;
        private readonly IUserImportVerifier userImportVerifier;
        private readonly IAuthorizedUser authorizedUser;
        
        private readonly Guid supervisorRoleId = UserRoles.Supervisor.ToUserId();
        private readonly Guid interviewerRoleId = UserRoles.Interviewer.ToUserId();

        public UserImportService(
            UserPreloadingSettings userPreloadingSettings, 
            ICsvReader csvReader,
            IPlainStorageAccessor<UsersImportProcess> importUsersProcessRepository,
            IPlainStorageAccessor<UserToImport> importUsersRepository,
            IUserRepository userStorage,
            IUserImportVerifier userImportVerifier,
            IAuthorizedUser authorizedUser)
        {
            this.userPreloadingSettings = userPreloadingSettings;
            this.csvReader = csvReader;
            this.importUsersProcessRepository = importUsersProcessRepository;
            this.importUsersRepository = importUsersRepository;
            this.userStorage = userStorage;
            this.userImportVerifier = userImportVerifier;
            this.authorizedUser = authorizedUser;
        }

        public IEnumerable<UserImportVerificationError> VerifyAndSaveIfNoErrors(byte[] data, string fileName)
        {
            var csvDelimiter = ExportFileSettings.DataFileSeparator.ToString();

            var requiredColumns = this.GetRequiredUserProperties();

            var columns = this.csvReader.ReadHeader(new MemoryStream(data), csvDelimiter)
                .Select(x => x.ToLower()).ToArray();

            var missingColumns = requiredColumns.Where(x => !columns.Contains(x));

            if (missingColumns.Any())
                throw new UserPreloadingException(string.Format(UserPreloadingServiceMessages.FileColumnsMissingFormat,
                        fileName, string.Join(", ", missingColumns)));

            var usersToImport = new List<UserToImport>();

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

            foreach (var userToImport in this.csvReader.ReadAll<UserToImport>(new MemoryStream(data), csvDelimiter))
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
            nameof(UserToImport.Login), nameof(UserToImport.Password),
            nameof(UserToImport.Role), nameof(UserToImport.Supervisor),
            nameof(UserToImport.FullName), nameof(UserToImport.Email),
            nameof(UserToImport.PhoneNumber)
        }.Select(x => x.ToLower()).ToArray();

        private void Save(string fileName, IList<UserToImport> usersToImport)
        {
            var process = this.importUsersProcessRepository.Query(x => x.FirstOrDefault()) ?? new UsersImportProcess();
            process.FileName = fileName;
            process.InterviewersCount = usersToImport.Count(x => x.Role == "interviewer");
            process.SupervisorsCount = usersToImport.Count(x => x.Role == "supervisor");
            process.Responsible = this.authorizedUser.Id;
            process.StartedDate = DateTime.UtcNow;

            this.importUsersProcessRepository.Store(process, process.Id);

            this.importUsersRepository.Store(usersToImport.OrderByDescending(x => x.Role)
                .Select(x => new Tuple<UserToImport, object>(x, null)));
        }

        public UsersImportStatus GetImportStatus()
        {
            var process = this.importUsersProcessRepository.Query(x => x.FirstOrDefault());
            var usersInQueue = this.importUsersRepository.Query(x => x.Count());

            return new UsersImportStatus
            {
                IsInProgress = usersInQueue > 0,
                TotalUsersToImport = process?.InterviewersCount + process?.SupervisorsCount ?? 0,
                UsersInQueue = usersInQueue,
                FileName = process?.FileName
            };
        }

        public UsersImportCompleteStatus GetImportCompleteStatus()
        {
            var process = this.importUsersProcessRepository.Query(x => x.FirstOrDefault());

            return new UsersImportCompleteStatus
            {
                SupervisorsCount = process?.SupervisorsCount ?? 0,
                InterviewersCount = process?.InterviewersCount ?? 0
            };
        }

        public void RemoveAllUsersToImport()
        {
            var allUsersToImport = this.importUsersRepository.Query(x => x.ToList());

            this.importUsersRepository.Remove(allUsersToImport);

            var usersImportProcess = this.importUsersProcessRepository.Query(x => x.FirstOrDefault());
            if (usersImportProcess != null)
                this.importUsersProcessRepository.Remove(usersImportProcess.Id);
        }

        public UserToImport GetUserToImport() => this.importUsersRepository.Query(x => x.FirstOrDefault());

        public void RemoveImportedUser(UserToImport importedUser)
            => this.importUsersRepository.Remove(new[] {importedUser});

        private static UserImportVerificationError ToVerificationError(PreloadedDataValidator validator,
            UserToImport userToImport, int userIndex)
            => new UserImportVerificationError
            {
                CellValue = validator.ValueSelector.Compile()(userToImport),
                Code = validator.Code,
                ColumnName = ((MemberExpression)validator.ValueSelector.Body).Member.Name,
                RowNumber = userIndex + 1 /*header item*/
            };
    }
}