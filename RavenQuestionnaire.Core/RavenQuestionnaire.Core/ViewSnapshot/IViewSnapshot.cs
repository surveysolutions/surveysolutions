using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.ViewSnapshot
{
    public interface IViewSnapshot
    {
        T ReadByGuid<T>(Guid key) where T : class;

    }

    public class DefaultViewSnapshot : IViewSnapshot
    {
        private readonly ISnapshotStore store;
        public DefaultViewSnapshot()
        {
            this.store = NcqrsEnvironment.Get<ISnapshotStore>();
            //NcqrsEnvironment.Get<IUnitOfWorkFactory>().CreateUnitOfWork(Guid.NewGuid()).GetById<>()
        }

        #region Implementation of IViewSnapshot

        public T ReadByGuid<T>(Guid key) where T : class
        {
            var snapshot = this.store.GetSnapshot(key, int.MaxValue);
            if (snapshot == null)
                return null;
            return snapshot.Payload as T;
        }

        #endregion
    }
}
