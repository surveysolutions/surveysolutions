using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class UserImportVerifier : IUserImportVerifier
    {
        private readonly UserPreloadingSettings userPreloadingSettings;
        private readonly Regex passwordValidationRegex;
        private readonly Regex loginValidatioRegex;
        private readonly Regex emailValidationRegex;
        private readonly Regex phoneNumberValidationRegex;
        private readonly Regex fullNameRegex;

        public UserImportVerifier(UserPreloadingSettings userPreloadingSettings)
        {
            this.userPreloadingSettings = userPreloadingSettings;
            this.passwordValidationRegex = new Regex(this.userPreloadingSettings.PasswordFormatRegex);
            this.loginValidatioRegex = new Regex(this.userPreloadingSettings.LoginFormatRegex);
            this.emailValidationRegex = new Regex(this.userPreloadingSettings.EmailFormatRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            this.phoneNumberValidationRegex = new Regex(this.userPreloadingSettings.PhoneNumberFormatRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            this.fullNameRegex = new Regex(this.userPreloadingSettings.PersonNameFormatRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public PreloadedDataValidator[] GetEachUserValidations(UserToValidate[] allInterviewersAndSupervisors)
        {
            var activeUserNames = allInterviewersAndSupervisors
                .Where(u => !u.IsArchived)
                .Select(u => u.UserName.ToLower())
                .ToHashSet();

            var archivedSupervisorNames = allInterviewersAndSupervisors
                .Where(u => u.IsArchived && u.IsSupervisor)
                .Select(u => u.UserName.ToLower())
                .ToHashSet();

            var archivedInterviewerNamesMappedOnSupervisorName = allInterviewersAndSupervisors
                .Where(u => u.IsArchived && u.IsInterviewer)
                .Select(u =>
                    new
                    {
                        u.UserName,
                        SupervisorName = allInterviewersAndSupervisors
                                             .FirstOrDefault(user => user.UserId == u.SupervisorId)?.UserName ?? ""
                    })
                .ToDictionary(u => u.UserName.ToLower(), u => u.SupervisorName.ToLower());

            return new[]
            {
                new PreloadedDataValidator(row => LoginNameUsedByExistingUser(activeUserNames, row), "PLU0001", u => u.Login),
                new PreloadedDataValidator(row => LoginOfArchiveUserCantBeReusedBecauseItBelongsToOtherTeam(archivedInterviewerNamesMappedOnSupervisorName, row), "PLU0003", u => u.Login),
                new PreloadedDataValidator(row => LoginOfArchiveUserCantBeReusedBecauseItExistsInOtherRole(archivedInterviewerNamesMappedOnSupervisorName, archivedSupervisorNames, row), "PLU0004", u => u.Login),
                new PreloadedDataValidator(LoginFormatVerification, "PLU0005", u => u.Login),
                new PreloadedDataValidator(PasswordFormatVerification, "PLU0006", u => u.Password),
                new PreloadedDataValidator(EmailFormatVerification, "PLU0007", u => u.Email),
                new PreloadedDataValidator(PhoneNumberFormatVerification, "PLU0008", u => u.PhoneNumber),
                new PreloadedDataValidator(RoleVerification, "PLU0009", u => u.Role),
                new PreloadedDataValidator(SupervisorColumnMustBeEmptyForUserInSupervisorRole, "PLU0011", u => u.Supervisor),
                new PreloadedDataValidator(FullNameLengthVerification, "PLU0012", u => u.FullName),
                new PreloadedDataValidator(PhoneLengthVerification, "PLU0013", u => u.PhoneNumber),
                new PreloadedDataValidator(FullNameAllowedSymbolsValidation, "PLU0014", u => u.FullName),
            };
        }

        public PreloadedDataValidator[] GetAllUsersValidations(
            UserToValidate[] allInterviewersAndSupervisors, IList<UserToImport> usersToImport)
        {
            var activeSupervisorNames = allInterviewersAndSupervisors
                .Where(u => !u.IsArchived && u.IsSupervisor)
                .Select(u => u.UserName.ToLower())
                .Distinct()
                .ToHashSet();

            var preloadedUsersGroupedByUserName =
                usersToImport.GroupBy(x => x.Login.ToLower()).ToDictionary(x => x.Key, x => x.Count());

            return new[]
            {
                new PreloadedDataValidator(
                    row => LoginDublicationInDataset(preloadedUsersGroupedByUserName, row),
                    "PLU0002", u => u.Login),

                new PreloadedDataValidator(row => SupervisorVerification(usersToImport, activeSupervisorNames, row),
                    "PLU0010", u => u.Supervisor)
            };
        }

        private bool FullNameAllowedSymbolsValidation(UserToImport arg)
        {
            if (string.IsNullOrEmpty(arg.FullName))
                return false;

            return !this.fullNameRegex.IsMatch(arg.FullName);
        }

        private bool PhoneLengthVerification(UserToImport arg)
            => arg.PhoneNumber?.Length > this.userPreloadingSettings.PhoneNumberMaxLength;

        private bool FullNameLengthVerification(UserToImport record)
            => record.FullName?.Length > this.userPreloadingSettings.FullNameMaxLength;

        private bool SupervisorColumnMustBeEmptyForUserInSupervisorRole(UserToImport userPreloadingDataRecord)
        {
            var role = userPreloadingDataRecord.UserRole;
            if (role != UserRoles.Supervisor)
                return false;

            return !string.IsNullOrEmpty(userPreloadingDataRecord.Supervisor);
        }

        private bool PasswordFormatVerification(UserToImport userPreloadingDataRecord)
        {
            if (string.IsNullOrEmpty(userPreloadingDataRecord.Password))
                return true;

            if (userPreloadingDataRecord.Password.Length > 100)
                return true;

            if (userPreloadingDataRecord.Password.Length < 10)
                return true;

            return !this.passwordValidationRegex.IsMatch(userPreloadingDataRecord.Password);
        }

        private bool LoginFormatVerification(UserToImport userPreloadingDataRecord)
            => !this.loginValidatioRegex.IsMatch(userPreloadingDataRecord.Login);

        private bool EmailFormatVerification(UserToImport userPreloadingDataRecord)
        {
            if (string.IsNullOrEmpty(userPreloadingDataRecord.Email))
                return false;

            return !this.emailValidationRegex.IsMatch(userPreloadingDataRecord.Email);
        }

        private bool PhoneNumberFormatVerification(UserToImport userPreloadingDataRecord)
        {
            if (string.IsNullOrEmpty(userPreloadingDataRecord.PhoneNumber))
                return false;

            return !this.phoneNumberValidationRegex.IsMatch(userPreloadingDataRecord.PhoneNumber);
        }

        private bool LoginNameUsedByExistingUser(HashSet<string> activeUserNames, UserToImport userPreloadingDataRecord) 
            => activeUserNames.Contains(userPreloadingDataRecord.Login.ToLower());

        private bool LoginDublicationInDataset(Dictionary<string, int> data,
            UserToImport userPreloadingDataRecord) => data[userPreloadingDataRecord.Login.ToLower()] > 1;

        bool LoginOfArchiveUserCantBeReusedBecauseItBelongsToOtherTeam(
            Dictionary<string, string> archivedInterviewerNamesMappedOnSupervisorName,
            UserToImport userPreloadingDataRecord)
        {
            var desiredRole = userPreloadingDataRecord.UserRole;
            if (desiredRole != UserRoles.Interviewer)
                return false;

            var loginName = userPreloadingDataRecord.Login.ToLower();
            if (!archivedInterviewerNamesMappedOnSupervisorName.ContainsKey(loginName))
                return false;

            if (archivedInterviewerNamesMappedOnSupervisorName[loginName] !=
                userPreloadingDataRecord.Supervisor.ToLower())
                return true;

            return false;
        }

        bool LoginOfArchiveUserCantBeReusedBecauseItExistsInOtherRole(
            Dictionary<string, string> archivedInterviewerNamesMappedOnSupervisorName,
            HashSet<string> archivedSupervisorNames, UserToImport userPreloadingDataRecord)
        {
            var desiredRole = userPreloadingDataRecord.UserRole;

            var loginName = userPreloadingDataRecord.Login.ToLower();
            switch (desiredRole)
            {
                case UserRoles.Interviewer:
                    return archivedSupervisorNames.Contains(loginName);
                case UserRoles.Supervisor:
                    return
                        archivedInterviewerNamesMappedOnSupervisorName.ContainsKey(
                            loginName);
                default:
                    return false;
            }
        }

        private bool RoleVerification(UserToImport userPreloadingDataRecord)
            => userPreloadingDataRecord.UserRole == 0;

        private bool SupervisorVerification(IList<UserToImport> data,
            HashSet<string> activeSupervisors, UserToImport userPreloadingDataRecord)
        {
            var role = userPreloadingDataRecord.UserRole;
            if (role != UserRoles.Interviewer)
                return false;

            if (string.IsNullOrEmpty(userPreloadingDataRecord.Supervisor))
                return true;

            var supervisorNameLowerCase = userPreloadingDataRecord.Supervisor.ToLower();
            if (activeSupervisors.Contains(supervisorNameLowerCase))
            {
                return false;
            }

            var supervisorsToPreload =
                data.Where(u => u.Login.ToLower() == supervisorNameLowerCase).ToArray();

            if (!supervisorsToPreload.Any())
                return true;

            foreach (var supervisorToPreload in supervisorsToPreload)
            {
                var possibleSupervisorRole = supervisorToPreload.UserRole;
                if (possibleSupervisorRole == UserRoles.Supervisor)
                    return false;
            }

            return true;
        }
    }
}
