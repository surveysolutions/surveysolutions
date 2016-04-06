using System;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.SharedKernels.DataCollection.Events.User
{
    [Obsolete("This event must be deleted after all clients who STARTED data collection with version 5.6 or earlier will fade away. " +
             "Pay attentions on the word 'STARTED'." +
             "If the client started with 5.5 and had been updated later to 5.8 he still might have the event which need to be handled")]
    public class UserLockedBySupervisor : IEvent { }
}