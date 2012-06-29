using System;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.ViewSnapshot
{
    public interface IViewSnapshot
    {
        T ReadByGuid<T>(Guid key) where T : class;

    }

    public class DefaultViewSnapshot : IViewSnapshot
    {
        private readonly ISnapshotStore store;
        private readonly ICommandService commandService;
        public DefaultViewSnapshot()
        {
            this.store = NcqrsEnvironment.Get<ISnapshotStore>();
            this.commandService = NcqrsEnvironment.Get<ICommandService>();
            //NcqrsEnvironment.Get<IUnitOfWorkFactory>().CreateUnitOfWork(Guid.NewGuid()).GetById<>()
        }

        #region Implementation of IViewSnapshot

        public T ReadByGuid<T>(Guid key) where T : class
        {
            var snapshot = this.store.GetSnapshot(key, int.MaxValue);
            if (snapshot == null && typeof(T)== typeof(CompleteQuestionnaireDocument))//very bad...
            {
                commandService.Execute(new PreLoadCompleteQuestionnaireCommand() { CompleteQuestionnaireId = key});
                snapshot = this.store.GetSnapshot(key, int.MaxValue);
                if(snapshot == null)
                    return null;
            }
            
            return snapshot.Payload as T;
        }

        #endregion
    }
}
