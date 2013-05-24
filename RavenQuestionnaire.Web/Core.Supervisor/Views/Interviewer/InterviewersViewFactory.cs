using Main.DenormalizerStorage;

namespace Core.Supervisor.Views.Interviewer
{
    using System.Linq;
    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;

    public class InterviewersViewFactory : BaseUserViewFactory, IViewFactory<InterviewersInputModel, InterviewersView>
    {
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> documents;

        public InterviewersViewFactory(
            IQueryableDenormalizerStorage<UserDocument> users,
            IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession) : base(users)
        {
            this.users = users;
            this.documents = documentSession;
        }
        
        public InterviewersView Load(InterviewersInputModel input)
        {
            var interviewers = this.GetInterviewersListForViewer(input.ViewerId);
            
            var items =
                interviewers.AsQueryable()
                            .OrderUsingSortExpression(input.Order)
                            .Skip((input.Page - 1) * input.PageSize)
                            .Take(input.PageSize)
                            .Select(
                                x => new InterviewersItem(x.PublicKey, x.UserName, x.Email, x.CreationDate, x.IsLocked));
            return new InterviewersView(
                input.Page, input.PageSize, items, input.ViewerId);
        }
    }
}