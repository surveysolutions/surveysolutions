using System;
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
            public Guid PublicKey { get; set; }
            public DateTime CreationDate { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public bool IsLockedBySupervisor { get; set; }
            public bool IsLockedByHQ { get; set; }
            public int InterviewersCount { get; set; }
            public int NotConnectedToDeviceInterviewersCount { get; set; }
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
                        id: x.PublicKey,
                        creationDate: x.CreationDate,
                        email: x.Email,
                        isLockedBySupervisor: x.IsLockedBySupervisor,
                        isLockedByHQ: x.IsLockedByHQ,
                        name: x.UserName,
                        interviewersCount: x.InterviewersCount,
                        notConnectedToDeviceInterviewersCount: x.NotConnectedToDeviceInterviewersCount
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
                allUsers = allUsers.Where(x => x.UserName.ToLower().Contains(input.SearchBy.ToLower()) || x.Email.ToLower().Contains(input.SearchBy.ToLower()));
            }

            var supervisors = allUsers.Select(ud => new SupervisorUserDocument()
            {
                PublicKey = ud.PublicKey,
                CreationDate = ud.CreationDate,
                Email = ud.Email,
                IsLockedBySupervisor = ud.IsLockedBySupervisor,
                IsLockedByHQ = ud.IsLockedByHQ,
                UserName = ud.UserName,
                InterviewersCount = _.Count(pr => pr.Supervisor.Id == ud.PublicKey && pr.IsArchived == false),
                NotConnectedToDeviceInterviewersCount = _.Count(pr => pr.Supervisor.Id == ud.PublicKey && pr.DeviceId == null && pr.IsArchived == false)
            });

            return supervisors;
        }
    }
}