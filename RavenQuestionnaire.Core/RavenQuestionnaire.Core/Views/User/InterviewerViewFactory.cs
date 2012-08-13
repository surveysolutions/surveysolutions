using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.ViewSnapshot;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerViewFactory : IViewFactory<InterviewerInputModel, InterviewerView>
    {
        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        private readonly IViewSnapshot store;

        public InterviewerViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession, IViewSnapshot store)
        {
            this.documentItemSession = documentSession;
            this.store = store;
        }

        #region Implementation of IViewFactory<UserViewInputModel,UserView>

        public InterviewerView Load(InterviewerInputModel input)
        {
            var doc = this.documentItemSession.Query();
            
            return new InterviewerView();
        }


        #endregion
    }
}
