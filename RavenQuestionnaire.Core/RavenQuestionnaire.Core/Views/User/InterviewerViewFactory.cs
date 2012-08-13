using System.Collections.Generic;
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
        private IDenormalizerStorage<UserDocument> users;

        public InterviewerViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession, IDenormalizerStorage<UserDocument> users)
        {
            this.documentItemSession = documentSession;
            this.users = users;
        }

        #region Implementation of IViewFactory<UserViewInputModel,UserView>

        public InterviewerView Load(InterviewerInputModel input)
        {
            var count = this.documentItemSession.Query().Where(input.Expression).Count();
            var user =  this.users.Query().FirstOrDefault(u => u.Id == input.UserId);
            if (count == 0)
            {
                return new InterviewerView(input.Page, input.PageSize, count, user.UserName, new List<CompleteQuestionnaireBrowseItem>(0));
            }
            var docs = this.documentItemSession.Query().Where(input.Expression).Skip((input.Page - 1) * input.PageSize)
                  .Take(input.PageSize).ToList();

            return new InterviewerView(input.Page, input.PageSize, count, user.UserName, docs);
        }


        #endregion
    }
}
