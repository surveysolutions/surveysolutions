using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide
{
    internal class TemporaryInterviewWriter: IReadSideRepositoryWriter<InterviewData>, IDisposable
    {
        private readonly Guid tempViewId;
        private readonly InterviewDenormalizer denormalizer;
        private InterviewData tempView;

        public TemporaryInterviewWriter(Guid id, InterviewData view, InterviewDenormalizer denormalizer)
        {
            this.tempViewId = id;
            this.tempView = view;
            this.denormalizer = denormalizer;
            denormalizer.SetStorage(this);
        }

        public InterviewData GetById(Guid id)
        {
            if (id != tempViewId)
                return null;
            return tempView;
        }

        public void Remove(Guid id)
        {
            tempView = null;
        }

        public void Store(InterviewData view, Guid id)
        {
            if(id!=tempViewId)
                return;
            tempView = view;
        }

        public void Dispose()
        {
            denormalizer.ClearStorage();
            tempView = null;
        }
    }
}
