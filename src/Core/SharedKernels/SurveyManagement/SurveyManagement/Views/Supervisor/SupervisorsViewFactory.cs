using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;


namespace WB.Core.SharedKernels.SurveyManagement.Views.Supervisor
{
    class SupervisorsViewFactory : ISupervisorsViewFactory
    {
        private class SupervisorUserDocument
        {
            public UserDocument UserDocument { get; set; }
            public int InterviewersCount { get; set; }
            public int ConnectedToDeviceInterviewersCount { get; set; }
        }

        private readonly IQueryableReadSideRepositoryReader<UserDocument> readSideRepositoryIndexAccessor;

        public SupervisorsViewFactory(IQueryableReadSideRepositoryReader<UserDocument> readSideRepositoryIndexAccessor)
        {
            this.readSideRepositoryIndexAccessor = readSideRepositoryIndexAccessor;
        }

        public SupervisorsView Load(SupervisorsInputModel input)
        {
            var users = this.readSideRepositoryIndexAccessor.Query(_ =>
            {
                var supervisors = ApplyFilter(_, input);

                supervisors = supervisors.OrderUsingSortExpression(input.Order)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize);

                return supervisors.ToList();

            })
            .Select(x => new SupervisorsItem(
                        id: x.UserDocument.PublicKey,
                        creationDate: x.UserDocument.CreationDate,
                        email: x.UserDocument.Email,
                        isLockedBySupervisor: x.UserDocument.IsLockedBySupervisor,
                        isLockedByHQ: x.UserDocument.IsLockedByHQ,
                        name: x.UserDocument.UserName,
                        interviewersCount: x.InterviewersCount,
                        connectedToDeviceInterviewersCount: x.ConnectedToDeviceInterviewersCount
                        ));


            var totalCount = this.readSideRepositoryIndexAccessor.Query(_ => ApplyFilter(_, input)).Count();

            return new SupervisorsView
            {
                TotalCount = totalCount,
                Items = users.ToList()
            };
        }

        private static IQueryable<SupervisorUserDocument> ApplyFilter(IQueryable<UserDocument> _, SupervisorsInputModel input)
        {
            var allUsers = _.Where(x => x.IsArchived == input.Archived && x.Roles.Contains(UserRoles.Supervisor));

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                allUsers = allUsers.Where(x => x.UserName.Contains(input.SearchBy) || x.Email.Contains(input.SearchBy));
            }

            var supervisors = allUsers.Select(ud => new SupervisorUserDocument()
            {
                UserDocument = ud,
                InterviewersCount = _.Count(pr => pr.Supervisor.Id == ud.PublicKey),
                ConnectedToDeviceInterviewersCount = _.Count(pr => pr.Supervisor.Id == ud.PublicKey && pr.DeviceId != null)
            });

            return supervisors;
        }
    }
}