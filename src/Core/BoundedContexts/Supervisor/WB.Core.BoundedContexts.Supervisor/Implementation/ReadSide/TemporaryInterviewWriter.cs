using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide
{
    internal class TemporaryInterviewWriter : IStorageStrategy<InterviewData>, IDisposable
    {
        private readonly Guid tempViewId;
        private readonly InterviewDenormalizerFunctional denormalizer;
        private InterviewData tempView;

        public TemporaryInterviewWriter(Guid id, InterviewData view, IReadSideRepositoryWriter<UserDocument> users,
            IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure>
                questionnaireRosterStructures)
        {
            this.tempViewId = id;
            this.tempView = view;
            this.denormalizer = new InterviewDenormalizerFunctional(users, questionnaireRosterStructures, this);
        }

        public InterviewData GetById(Guid id)
        {
            if (id != tempViewId)
                return null;
            return tempView;
        }

        public void Dispose()
        {
            tempView = null;
        }

        public void PublishEvents(CommittedEventStream events)
        {
            foreach (var @event in events)
            {
                try
                {
                    denormalizer.Handle(@event);
                }
                catch
                {
                }
            }
        }

        public InterviewData Select(Guid id)
        {
            return tempView;
        }

        public void AddOrUpdate(InterviewData projection, Guid id)
        {
            tempView = projection;
        }

        public void Delete(InterviewData projection, Guid id)
        {
           
        }
    }
}
