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
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> _documents;

        public InterviewersViewFactory(
            IQueryableDenormalizerStorage<UserDocument> users,
            IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession) : base(users)
        {
            this.users = users;
            this._documents = documentSession;
        }
        
        public InterviewersView Load(InterviewersInputModel input)
        {
            var interviewers = this.GetInterviewersListForViewer(input.ViewerId);

            if (!interviewers.Any())
            {
                return new InterviewersView(
                    input.Page, 
                    input.PageSize, 
                    new InterviewersItem[0],
                    input.ViewerId);
            }

            IQueryable<InterviewersItem> items =
                interviewers.Select(
                    x =>
                    new InterviewersItem(
                        x.PublicKey, 
                        x.UserName, 
                        x.Email, 
                        x.CreationDate, 
                        x.IsLocked)).AsQueryable();
            if (input.Orders.Count > 0)
            {
                items = input.Orders[0].Direction == OrderDirection.Asc
                            ? items.OrderBy(input.Orders[0].Field)
                            : items.OrderByDescending(input.Orders[0].Field);
            }

            items = items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
            return new InterviewersView(
                input.Page, input.PageSize, items, input.ViewerId);
        }
    }
}