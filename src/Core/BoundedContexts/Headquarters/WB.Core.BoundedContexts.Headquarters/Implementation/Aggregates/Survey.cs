using System;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Exceptions;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates
{
    internal class Survey : AggregateRootMappedByConvention
    {
        #region State

        private void Apply(NewSurveyStarted @event) { }

        private void Apply(SupervisorAccountRegistered @event) { }

        #endregion

        #region Dependencies

        private static IPasswordHasher PasswordHasher
        {
            get { return ServiceLocator.Current.GetInstance<IPasswordHasher>(); }
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

        public void RegisterSupervisorAccount(string login, string password)
        {
            this.ThrowIfSupervisorsLoginIsNotUnique(login);

            var passwordHash = PasswordHasher.Hash(password);

            this.ApplyEvent(new SupervisorAccountRegistered(login, passwordHash));
        }

        #region Invariants

        private void ThrowIfSupervisorsLoginIsNotUnique(string login)
        {
            return;
        }

        private void ThrowIfSurveyNameIsEmpty(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new SurveyException("Survey name cannot be empty.");
        }

        #endregion
    }
}