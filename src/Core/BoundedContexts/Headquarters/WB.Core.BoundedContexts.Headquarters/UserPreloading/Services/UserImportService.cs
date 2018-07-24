using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using Main.Core.Entities.SubEntities;
using Npgsql;
using NpgsqlTypes;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class UserImportService : IUserImportService
    {
        private const string UserToImportTableName = "\"plainstore\".\"usertoimport\"";
        private const string UsersImportProcessTableName = "\"plainstore\".\"usersimportprocess\"";

        private readonly UserPreloadingSettings userPreloadingSettings;
        private readonly ICsvReader csvReader;
        private readonly IPlainStorageAccessor<UsersImportProcess> importUsersProcessRepository;
        private readonly IPlainStorageAccessor<UserToImport> importUsersRepository;
        private readonly IUserRepository userStorage;
        private readonly IUserImportVerifier userImportVerifier;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ISessionProvider sessionProvider;
        private readonly UsersImportTask usersImportTask;

        private readonly Guid supervisorRoleId = UserRoles.Supervisor.ToUserId();
        private readonly Guid interviewerRoleId = UserRoles.Interviewer.ToUserId();

        public UserImportService(
            UserPreloadingSettings userPreloadingSettings,
            ICsvReader csvReader,
            IPlainStorageAccessor<UsersImportProcess> importUsersProcessRepository,
            IPlainStorageAccessor<UserToImport> importUsersRepository,
            IUserRepository userStorage,
            IUserImportVerifier userImportVerifier,
            IAuthorizedUser authorizedUser,
            ISessionProvider sessionProvider,
            UsersImportTask usersImportTask)
        {
            this.userPreloadingSettings = userPreloadingSettings;
            this.csvReader = csvReader;
            this.importUsersProcessRepository = importUsersProcessRepository;
            this.importUsersRepository = importUsersRepository;
            this.userStorage = userStorage;
            this.userImportVerifier = userImportVerifier;
            this.authorizedUser = authorizedUser;
            this.sessionProvider = sessionProvider;
            this.usersImportTask = usersImportTask;
        }

        public IEnumerable<UserImportVerificationError> VerifyAndSaveIfNoErrors(byte[] data, string fileName)
        {
            if (this.usersImportTask.IsJobRunning())
                throw new PreloadingException(UserPreloadingServiceMessages.HasUsersToImport);

            var csvDelimiter = ExportFileSettings.DataFileSeparator.ToString();

            var requiredColumns = this.GetRequiredUserProperties();

            var columns = this.csvReader.ReadHeader(new MemoryStream(data), csvDelimiter)
                .Select(x => x.ToLower()).ToArray();

            var missingColumns = requiredColumns.Where(x => !columns.Contains(x));

            if (missingColumns.Any())
                throw new PreloadingException(string.Format(UserPreloadingServiceMessages.FileColumnsMissingFormat,
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
            var hasErrors = false;

            using (var userToImports = this.csvReader.ReadAll<UserToImport>(new MemoryStream(data), csvDelimiter).GetEnumerator())
            {
                do
                {
                    try
                    {
                        if (!userToImports.MoveNext())
                        {
                            break;
                        }
                    }
                    catch (CsvHelper.BadDataException dataException)
                    {
                        throw new PreloadingException(
                            string.Format(UserPreloadingServiceMessages.CannotParseIncomingFile,
                                dataException.ReadingContext.Row));
                    }

                    if (userToImports.Current?.Email?.Trim() == string.Empty)
                        userToImports.Current.Email = null;

                    usersToImport.Add(userToImports.Current);

                    foreach (var validator in validations)
                    {
                        if (!validator.ValidationFunction(userToImports.Current)) continue;

                        hasErrors = true;
                        yield return ToVerificationError(validator, userToImports.Current, usersToImport.Count);
                    }

                    if (usersToImport.Count > userPreloadingSettings.MaxAllowedRecordNumber)
                        throw new PreloadingException(string.Format(
                            UserPreloadingServiceMessages.TheDatasetMaxRecordNumberReachedFormat,
                            this.userPreloadingSettings.MaxAllowedRecordNumber));
                } while (userToImports.Current != null);
            }

            validations = this.userImportVerifier.GetAllUsersValidations(allInterviewersAndSupervisors, usersToImport);

            for (int userIndex = 0; userIndex < usersToImport.Count; userIndex++)
            {
                var userToImport = usersToImport[userIndex];

                foreach (var validator in validations)
                {
                    if (!validator.ValidationFunction(userToImport)) continue;

                    hasErrors = true;
                    yield return ToVerificationError(validator, userToImport, userIndex + 1);
                }
            }

            if (!hasErrors) this.Save(fileName, usersToImport);

            usersImportTask.Run();
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
            this.SaveProcess(fileName, usersToImport);
            this.SaveUsers(usersToImport);
        }

        private void SaveProcess(string fileName, IList<UserToImport> usersToImport)
        {
            var process = this.importUsersProcessRepository.Query(x => x.FirstOrDefault()) ?? new UsersImportProcess();
            process.FileName = fileName;
            process.InterviewersCount = usersToImport.Count(x => x.UserRole == UserRoles.Interviewer);
            process.SupervisorsCount = usersToImport.Count(x => x.UserRole == UserRoles.Supervisor);
            process.Responsible = this.authorizedUser.UserName;
            process.StartedDate = DateTime.UtcNow;

            this.importUsersProcessRepository.Store(process, process?.Id);
        }

        private void SaveUsers(IList<UserToImport> usersToImport)
        {
            var npgsqlConnection = this.sessionProvider.GetSession().Connection as NpgsqlConnection;

            using (var writer = npgsqlConnection.BeginBinaryImport($"COPY  {UserToImportTableName} (login, email, fullname, password, phonenumber, role, supervisor) " +
                                                                   "FROM STDIN BINARY;"))
            {
                foreach (var userToImport in usersToImport.OrderBy(x => x.UserRole))
                {
                    writer.StartRow();
                    writer.Write(userToImport.Login, NpgsqlDbType.Text);
                    writer.Write(userToImport.Email, NpgsqlDbType.Text);
                    writer.Write(userToImport.FullName, NpgsqlDbType.Text);
                    writer.Write(userToImport.Password, NpgsqlDbType.Text);
                    writer.Write(userToImport.PhoneNumber, NpgsqlDbType.Text);
                    writer.Write(userToImport.Role, NpgsqlDbType.Text);
                    writer.Write(userToImport.Supervisor, NpgsqlDbType.Text);
                }
            }
        }

        public UsersImportStatus GetImportStatus()
        {
            var process = this.importUsersProcessRepository.Query(x => x.FirstOrDefault());
            var usersInQueue = this.importUsersRepository.Query(x => x.Count());

            return new UsersImportStatus
            {
                IsOwnerOfRunningProcess = process?.Responsible == this.authorizedUser.UserName,
                IsInProgress = usersInQueue > 0,
                TotalUsersToImport = process?.InterviewersCount + process?.SupervisorsCount ?? 0,
                UsersInQueue = usersInQueue,
                FileName = process?.FileName,
                StartedDate = process?.StartedDate,
                Responsible = process?.Responsible
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

        public void RemoveAllUsersToImport() => this.sessionProvider.GetSession().Connection.Execute(
            $"DELETE FROM {UserToImportTableName};" +
            $"DELETE FROM {UsersImportProcessTableName};");

        public UserToImport GetUserToImport() => this.importUsersRepository.Query(x => x.FirstOrDefault());

        public void RemoveImportedUser(UserToImport importedUser)
            => this.importUsersRepository.Remove(new[] { importedUser });

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
