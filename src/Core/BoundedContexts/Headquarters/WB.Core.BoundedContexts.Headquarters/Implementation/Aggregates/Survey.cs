using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Exceptions;
using WB.Core.BoundedContexts.Headquarters.PasswordPolicy;
using WB.Core.BoundedContexts.Headquarters.Services;
using IPasswordHasher = WB.Core.GenericSubdomains.Utils.IPasswordHasher;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates
{
    internal class Survey : AggregateRootMappedByConvention
    {
        #region State

        private void Apply(NewSurveyStarted @event) { }

        private void Apply(SupervisorRegistered @event) { }

        #endregion

        #region Dependencies

        private static IPasswordHasher PasswordHasher
        {
            get { return ServiceLocator.Current.GetInstance<IPasswordHasher>(); }
        }

        private static ISupervisorLoginService SupervisorLoginService
        {
            get { return ServiceLocator.Current.GetInstance<ISupervisorLoginService>(); }
        }

        private static ApplicationPasswordPolicySettings ApplicationPasswordPolicySettings
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationPasswordPolicySettings>(); }
        }
        #endregion

        /// <remarks>Is used to restore aggregate from event stream.</remarks>
        public Survey() { }

        public Survey(Guid id, string name)
            : base(id)
        {
            this.ThrowIfSurveyNameIsEmpty(name);

            this.ApplyEvent(new NewSurveyStarted(name));
        }

        public void RegisterSupervisor(string login, string password)
        {
            this.ThrowIfSupervisorsLoginIsNotUnique(login);

            this.ThrowIfSupervisorsPasswordDoesnotMeetApplicationPasswordPolicy(password);

            var passwordHash = PasswordHasher.Hash(password);

            this.ApplyEvent(new SupervisorRegistered(login, passwordHash));
        }

        #region Invariants

        private void ThrowIfSupervisorsLoginIsNotUnique(string login)
        {
            if (!SupervisorLoginService.IsUnique(login))
            {
                throw new SurveyException(string.Format("Supervisor's login {0} is already taken", login));
            }
        }

        private void ThrowIfSupervisorsPasswordDoesnotMeetApplicationPasswordPolicy (string password)
        {
            if (String.IsNullOrEmpty(password) || password.Length < ApplicationPasswordPolicySettings.MinPasswordLength)
            {
                throw new SurveyException(string.Format("Supervisor's password is too short. Password should have at least {0} characters", ApplicationPasswordPolicySettings.MinPasswordLength));
            }

            if (!string.IsNullOrEmpty(ApplicationPasswordPolicySettings.PasswordPattern) && !Regex.IsMatch(password, ApplicationPasswordPolicySettings.PasswordPattern))
            {
                throw new SurveyException("Password must contain at least one number, one upper case character and one lower case character");
            }
        }

        private void ThrowIfSurveyNameIsEmpty(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new SurveyException("Survey name cannot be empty.");
        }

        #endregion
    }
}