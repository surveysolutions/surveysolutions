using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide
{
    public class ReadSideRepositoryReaderWithSequence<T> : IReadSideRepositoryReader<T> where T : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideRepositoryReader<ViewWithSequence<T>> readsideReader;
        private static ConcurrentDictionary<Guid, bool> packagesInProcess = new ConcurrentDictionary<Guid, bool>();
        private Action<Guid> additionalEventChecker;
        private const int CountOfAttempt = 60;

        public ReadSideRepositoryReaderWithSequence(
            IReadSideRepositoryReader<ViewWithSequence<T>> readsideReader, Action<Guid> additionalEventChecker)
        {
            this.readsideReader = readsideReader;
            this.additionalEventChecker = additionalEventChecker;
        }

        public int Count()
        {
            return this.readsideReader.Count();
        }

        public T GetById(Guid id)
        {
            var view = this.readsideReader.GetById(id);
            if (this.IsViewWasUpdatedFromEventStream(id))
            {
                view = this.readsideReader.GetById(id);
                if (view == null)
                    return null;
            }

            return view.Document;
        }

        private bool IsViewWasUpdatedFromEventStream(Guid id)
        {
            if (!this.WaitUntilViewCanBeProcessed(id))
                return false;

            var result = false;

            try
            {
                this.additionalEventChecker(id);
                result = true;
            }
            finally
            {
                this.ReleaseSpotForOtherThread(id);
            }

            return result;
        }

        private void ReleaseSpotForOtherThread(Guid id)
        {
            bool dummyBool;
            packagesInProcess.TryRemove(id, out dummyBool);
        }

        private bool WaitUntilViewCanBeProcessed(Guid id)
        {
            int i = 0;
            while (!packagesInProcess.TryAdd(id,true))
            {
                if (i > CountOfAttempt)
                {
                    return false;
                }
                Thread.Sleep(1000);
                i++;
            }
            return true;
        }
    }
}

