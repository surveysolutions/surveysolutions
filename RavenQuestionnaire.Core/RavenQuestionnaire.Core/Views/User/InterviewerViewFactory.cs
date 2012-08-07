using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.ViewSnapshot;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerViewFactory : IViewFactory<InterviewerInputModel, InterviewerView>
    {
        private IDocumentSession documentSession;
        private readonly IViewSnapshot store;

        public InterviewerViewFactory(IDocumentSession documentSession, IViewSnapshot store)
        {
            this.documentSession = documentSession;
            this.store = store;
        }

        #region Implementation of IViewFactory<UserViewInputModel,UserView>

        public InterviewerView Load(InterviewerInputModel input)
        {
            var doc = this.documentSession.Load<UserDocument>(input.UserId);
            
            return new InterviewerView();
        }


        #endregion
    }
}
