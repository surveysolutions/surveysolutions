using System.Linq;
using Main.Core.Documents;
using Main.Core.Utility;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
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
                                x => new InterviewersItem(x.PublicKey, x.UserName, x.Email, x.CreationDate, x.IsLockedBySupervisor, x.IsLockedByHQ));
            return new InterviewersView() {Items = items, TotalCount = interviewers.Count()};
        }
    }
}