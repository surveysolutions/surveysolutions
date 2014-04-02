using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View;
using WB.Core.BoundedContexts.Headquarters.Team.Models;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Team.ViewFactories
{
    public class InterviewersViewFactory : IViewFactory<InterviewersInputModel, InterviewersView>
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public InterviewersViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.users = users;
        }

        public InterviewersView Load(InterviewersInputModel input)
        {
            var interviewers = this.users.Query(_ => _
                .Where(user => user.Roles.Any(role => role == UserRoles.Operator) && user.Supervisor.Id == input.SupervisorId)
                .ToList());

            var items = interviewers
                .AsQueryable()
                .OrderUsingSortExpression(input.Order)
                .Skip((input.Page - 1)*input.PageSize)
                .Take(input.PageSize)
                .Select(x => new InterviewersItem(x.PublicKey, x.UserName, x.Email, x.CreationDate, x.IsLocked));

            return new InterviewersView { Items = items, TotalCount = interviewers.Count() };
        }
    }
}