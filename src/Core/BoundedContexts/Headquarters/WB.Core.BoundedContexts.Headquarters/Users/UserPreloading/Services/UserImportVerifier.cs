using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public class UserImportVerifier : IUserImportVerifier
    {
        private readonly UserPreloadingSettings userPreloadingSettings;
        private readonly Regex loginValidationRegex;
        private readonly Regex emailValidationRegex;
        private readonly Regex phoneNumberValidationRegex;
        private readonly Regex fullNameRegex;
        private IOptions<IdentityOptions> passwordValidator;
        
        public UserImportVerifier(UserPreloadingSettings userPreloadingSettings, 
            IOptions<IdentityOptions> passwordValidator)
        {
            this.userPreloadingSettings = userPreloadingSettings;
            this.passwordValidator = passwordValidator;
            this.loginValidationRegex = new Regex(this.userPreloadingSettings.LoginFormatRegex);
            this.emailValidationRegex = new Regex(this.userPreloadingSettings.EmailFormatRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            this.phoneNumberValidationRegex = new Regex(this.userPreloadingSettings.PhoneNumberFormatRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            this.fullNameRegex = new Regex(this.userPreloadingSettings.PersonNameFormatRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public PreloadedDataValidator[] GetEachUserValidations(UserToValidate[] allInterviewersAndSupervisors)
        {
            var activeUserNames = allInterviewersAndSupervisors
                .Where(u => !u.IsArchived)
                .Select(u => u.UserName.ToLower())
                .ToImmutableHashSet();

            var archivedSupervisorNames = allInterviewersAndSupervisors
                .Where(u => u.IsArchived && u.IsSupervisor)
                .Select(u => u.UserName.ToLower())
                .ToImmutableHashSet();

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
                new PreloadedDataValidator(EmailFormatVerification, "PLU0007", u => u.Email),
                new PreloadedDataValidator(PhoneNumberFormatVerification, "PLU0008", u => u.PhoneNumber),
                new PreloadedDataValidator(RoleVerification, "PLU0009", u => u.Role),
                new PreloadedDataValidator(SupervisorColumnMustBeEmptyForUserInSupervisorRole, "PLU0011", u => u.Supervisor),
                new PreloadedDataValidator(FullNameLengthVerification, "PLU0012", u => u.FullName),
                new PreloadedDataValidator(PhoneLengthVerification, "PLU0013", u => u.PhoneNumber),
                new PreloadedDataValidator(FullNameAllowedSymbolsValidation, "PLU0014", u => u.FullName),
                
                new PreloadedDataValidator(PasswordLength, "PLU0015", u => u.Password),
                new PreloadedDataValidator(PasswordRequireNonAlphanumeric, "PLU0016", u => u.Password),
                new PreloadedDataValidator(PasswordRequireDigit, "PLU0017", u => u.Password),
                new PreloadedDataValidator(PasswordRequireLowercase, "PLU0018", u => u.Password),
                new PreloadedDataValidator(PasswordRequireUppercase, "PLU0019", u => u.Password),
                new PreloadedDataValidator(PasswordRequiredUniqueChars, "PLU0020", u => u.Password),
                new PreloadedDataValidator(PasswordRequired, "PLU0021", u => u.Password),
            };
        }

        private bool PasswordRequired(UserToImport userPreloadingDataRecord)
        {
            return string.IsNullOrEmpty(userPreloadingDataRecord.Password);
        }
        
        private bool PasswordLength(UserToImport userPreloadingDataRecord)
        {
            var options = this.passwordValidator.Value.Password;

            return !string.IsNullOrEmpty(userPreloadingDataRecord.Password) &&
                   userPreloadingDataRecord.Password.Length < options.RequiredLength;
        }
        
        private bool PasswordRequireNonAlphanumeric(UserToImport userPreloadingDataRecord)
        {
            var options = this.passwordValidator.Value.Password;

            return options.RequireNonAlphanumeric &&
                   !string.IsNullOrEmpty(userPreloadingDataRecord.Password) &&
                   userPreloadingDataRecord.Password.All(char.IsLetterOrDigit) == true;
        }
        
        private bool PasswordRequireDigit(UserToImport userPreloadingDataRecord)
        {
            var options = this.passwordValidator.Value.Password;

            return options.RequireDigit && 
                   !string.IsNullOrEmpty(userPreloadingDataRecord.Password) &&
                   userPreloadingDataRecord.Password?.Any(char.IsDigit) == false;
        }

        private bool PasswordRequireLowercase(UserToImport userPreloadingDataRecord)
        {
            var options = this.passwordValidator.Value.Password;

            return options.RequireLowercase && 
                   !string.IsNullOrEmpty(userPreloadingDataRecord.Password) &&
                   userPreloadingDataRecord.Password.Any(char.IsLower) == false;
        }

        private bool PasswordRequireUppercase(UserToImport userPreloadingDataRecord)
        {
            var options = this.passwordValidator.Value.Password;

            return options.RequireUppercase && 
                   !string.IsNullOrEmpty(userPreloadingDataRecord.Password) &&
                   userPreloadingDataRecord.Password?.Any(char.IsUpper) == false;
        }

        private bool PasswordRequiredUniqueChars(UserToImport userToImport)
        {
            var options = this.passwordValidator.Value.Password;

            return options.RequiredUniqueChars >= 1 &&
                   !string.IsNullOrEmpty(userToImport.Password) &&
                   userToImport.Password.Distinct().Count() < options.RequiredUniqueChars;
        }
        
        public PreloadedDataValidator[] GetAllUsersValidations(
            UserToValidate[] allInterviewersAndSupervisors, IList<UserToImport> usersToImport)
        {
            var activeSupervisorNames = allInterviewersAndSupervisors
                .Where(u => !u.IsArchived && u.IsSupervisor && u.IsInCurrentWorkspace)
                .Select(u => u.UserName.ToLower())
                .ToImmutableHashSet();

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

        private bool LoginFormatVerification(UserToImport userPreloadingDataRecord)
            => !this.loginValidationRegex.IsMatch(userPreloadingDataRecord.Login);

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

        private bool LoginNameUsedByExistingUser(ICollection<string> activeUserNames, UserToImport userPreloadingDataRecord) 
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
            ICollection<string> archivedSupervisorNames, UserToImport userPreloadingDataRecord)
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
            ICollection<string> activeSupervisors, UserToImport userPreloadingDataRecord)
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
