// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;
using WB.UI.Designer.Providers.Roles;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.UI.Designer.Providers.CQRS.Accounts
{
    using WB.UI.Designer.Providers.CQRS.Accounts.Events;
    using WB.UI.Designer.Providers.Roles;

    /// <summary>
    /// The user denormalizer.
    /// </summary>
    public class AccountDenormalizer : IEventHandler<AccountConfirmed>,
                                       IEventHandler<AccountDeleted>,
                                       IEventHandler<AccountLocked>,
                                       IEventHandler<AccountOnlineUpdated>,
                                       IEventHandler<AccountPasswordChanged>,
                                       IEventHandler<AccountPasswordQuestionAndAnswerChanged>,
                                       IEventHandler<AccountPasswordReset>,
                                       IEventHandler<AccountRegistered>,
                                       IEventHandler<AccountUnlocked>,
                                       IEventHandler<AccountUpdated>,
                                       IEventHandler<AccountValidated>,
                                       IEventHandler<AccountRoleAdded>,
                                       IEventHandler<AccountRoleRemoved>,
                                       IEventHandler<AccountLoginFailed>
    {
        #region Constants and Fields

        /// <summary>
        /// The accounts.
        /// </summary>
        private readonly IDenormalizerStorage<AccountDocument> _accounts;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDenormalizer"/> class.
        /// </summary>
        /// <param name="accounts">
        /// The users.
        /// </param>
        public AccountDenormalizer(IDenormalizerStorage<AccountDocument> accounts)
        {
            _accounts = accounts;
        }

        #endregion

        public void Handle(IPublishedEvent<AccountConfirmed> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.IsConfirmed = true;
        }

        public void Handle(IPublishedEvent<AccountDeleted> @event)
        {
            _accounts.Remove(@event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountLocked> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.IsLockedOut = true;
            item.LastLockedOutAt = @event.Payload.LastLockedOutAt;
        }

        public void Handle(IPublishedEvent<AccountOnlineUpdated> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.LastActivityAt = @event.Payload.LastActivityAt;
        }

        public void Handle(IPublishedEvent<AccountPasswordChanged> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.Password = @event.Payload.Password;
            item.LastPasswordChangeAt = @event.Payload.LastPasswordChangeAt;
        }

        public void Handle(IPublishedEvent<AccountPasswordQuestionAndAnswerChanged> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.PasswordAnswer = @event.Payload.PasswordAnswer;
            item.PasswordQuestion = @event.Payload.PasswordQuestion;
        }

        public void Handle(IPublishedEvent<AccountPasswordReset> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.PasswordSalt = @event.Payload.PasswordSalt;
            item.Password = @event.Payload.Password;
        }

        public void Handle(IPublishedEvent<AccountRegistered> @event)
        {
            _accounts.Store(
                new AccountDocument
                    {
                        ProviderUserKey = @event.EventSourceId,
                        UserName = @event.Payload.UserName,
                        Email = @event.Payload.Email,
                        ConfirmationToken = @event.Payload.ConfirmationToken,
                        ApplicationName = @event.Payload.ApplicationName,
                        CreatedAt = @event.Payload.CreatedDate,
                        FailedPasswordAnswerWindowAttemptCount = 0,
                        FailedPasswordWindowAttemptCount = 0,
                        SimpleRoles = new List<SimpleRoleEnum>()
                    },
                @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountUnlocked> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.IsLockedOut = false;
            item.FailedPasswordAnswerWindowAttemptCount = 0;
            item.FailedPasswordAnswerWindowStartedAt = DateTime.MinValue;
            item.FailedPasswordWindowAttemptCount = 0;
            item.FailedPasswordWindowStartedAt = DateTime.MinValue;
        }

        public void Handle(IPublishedEvent<AccountUpdated> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.Comment = @event.Payload.Comment;
            item.Email = @event.Payload.Email;
            item.PasswordQuestion = @event.Payload.PasswordQuestion;
            item.UserName = @event.Payload.UserName;
        }

        public void Handle(IPublishedEvent<AccountValidated> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.LastLoginAt = @event.Payload.LastLoginAt;
            item.FailedPasswordWindowStartedAt = DateTime.MinValue;
            item.FailedPasswordWindowAttemptCount = 0;
        }

        public void Handle(IPublishedEvent<AccountRoleAdded> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.SimpleRoles.Add(@event.Payload.Role);
        }

        public void Handle(IPublishedEvent<AccountRoleRemoved> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.SimpleRoles.Remove(@event.Payload.Role);
        }

        public void Handle(IPublishedEvent<AccountLoginFailed> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            item.FailedPasswordWindowStartedAt = @event.Payload.FailedPasswordWindowStartedAt;
            item.FailedPasswordWindowAttemptCount += 1;
        }
    }
}