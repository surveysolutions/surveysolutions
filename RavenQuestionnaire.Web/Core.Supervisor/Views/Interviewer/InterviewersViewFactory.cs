using Main.DenormalizerStorage;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Core.Supervisor.Views.Interviewer
{
    using System.Linq;
    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Utility;
    using Main.Core.View;

    public class InterviewersViewFactory : BaseUserViewFactory, IViewFactory<InterviewersInputModel, InterviewersView>
    {
        public InterviewersViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users)
            : base(users)
        {
            this.users = users;
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