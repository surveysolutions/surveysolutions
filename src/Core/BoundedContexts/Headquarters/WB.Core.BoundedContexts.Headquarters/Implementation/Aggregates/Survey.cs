using System;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Exceptions;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Utils;

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

        private static ILoginsChecker LoginsChecker
        {
            get { return ServiceLocator.Current.GetInstance<ILoginsChecker>(); }
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

            var passwordHash = PasswordHasher.Hash(password);

            this.ApplyEvent(new SupervisorRegistered(login, passwordHash));
        }

        #region Invariants

        private void ThrowIfSupervisorsLoginIsNotUnique(string login)
        {
            if (!LoginsChecker.IsUnique(login))
            {
                throw new SurveyException(string.Format("Supervisor's login {0} is already taken", login));
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