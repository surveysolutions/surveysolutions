using System;
using System.Linq;
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

        private static IPasswordHasher PasswordHasher
        {
            get { return ServiceLocator.Current.GetInstance<IPasswordHasher>(); }
        }

        private static ISupervisorLoginService SupervisorLoginService
        {
            get { return ServiceLocator.Current.GetInstance<ISupervisorLoginService>(); }
        }

        private static IIdentityValidator<string> PasswordValidator
        {
            get { return ServiceLocator.Current.GetInstance<CustomPasswordValidator>(); }
        } 

        private static ApplicationPasswordPolicySettings ApplicationPasswordPolicySettings
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationPasswordPolicySettings>(); }
        }

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
                throw new SurveyException(string.Format(Resources.SupervisorLoginAlreadyTaken, login));
            }
        }

        private void ThrowIfSupervisorsPasswordDoesnotMeetApplicationPasswordPolicy (string password)
        {
            IdentityResult identityResult = PasswordValidator.ValidateAsync(password).Result;

            if (!identityResult.Succeeded)
            {
                throw new SurveyException(identityResult.Errors.First());
            }
        }

        private void ThrowIfSurveyNameIsEmpty(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new SurveyException(Resources.SurveyNameCannotBeEmpty);
        }

        #endregion
    }
}