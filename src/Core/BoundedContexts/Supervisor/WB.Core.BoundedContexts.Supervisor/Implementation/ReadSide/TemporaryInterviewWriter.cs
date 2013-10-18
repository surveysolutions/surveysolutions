using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
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
    internal class TemporaryInterviewWriter : IReadSideRepositoryWriter<InterviewData>, IDisposable
    {
        private readonly Guid tempViewId;
        private readonly InterviewDenormalizer denormalizer;
        private readonly InProcessEventBus eventBus = new InProcessEventBus(false);
        private InterviewData tempView;

        public TemporaryInterviewWriter(Guid id, InterviewData view, IReadSideRepositoryWriter<UserDocument> users,
                                        IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure>
                                            questionnairePropagationStructures)
        {
            this.tempViewId = id;
            this.tempView = view;
            this.denormalizer = new InterviewDenormalizer(users, questionnairePropagationStructures, this);
            RegisterInterviewDenormalizerAtProcessBus();
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
            if (id != tempViewId)
                return;
            tempView = view;
        }

        public void Dispose()
        {
            tempView = null;
        }



        private void RegisterInterviewDenormalizerAtProcessBus()
        {
            IEnumerable<Type> ieventHandlers =
                denormalizer.GetType()
                            .GetInterfaces()
                            .Where(
                                type =>
                                type.IsInterface && type.IsGenericType &&
                                type.GetGenericTypeDefinition() == typeof (IEventHandler<>));
            foreach (Type ieventHandler in ieventHandlers)
            {
                eventBus.RegisterHandler(denormalizer, ieventHandler.GetGenericArguments()[0]);
            }
        }

        public void PublishEvents(CommittedEventStream events)
        {
            foreach (var @event in events)
            {
                try
                {
                    eventBus.Publish(@event);
                }
                catch
                {
                }
            }
        }
    }
}
