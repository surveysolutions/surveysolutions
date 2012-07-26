using Ncqrs;
using System;
using Ncqrs.Eventing.Storage;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;

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
            if (snapshot == null )//very bad...
            {
                if(typeof(T) == typeof(CompleteQuestionnaireDocument) )
                {
                    commandService.Execute(new PreLoadCompleteQuestionnaireCommand() { CompleteQuestionnaireId = key});
                }
                else if (typeof(T) == typeof(QuestionnaireDocument))
                {
                    commandService.Execute(new PreLoadQuestionnaireCommand() { QuestionnaireId = key });
                }
                
                snapshot = this.store.GetSnapshot(key, int.MaxValue);
                if(snapshot == null)
                    return null;
            }
            
            return snapshot.Payload as T;
        }

        #endregion
    }
}
