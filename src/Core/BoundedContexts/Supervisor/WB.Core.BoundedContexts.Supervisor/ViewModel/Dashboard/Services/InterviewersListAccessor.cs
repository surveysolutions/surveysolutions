using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services
{
    public class InterviewersListAccessor : IInterviewersListAccessor
    {
        public List<InterviewerAssignInfo> GetInterviewers()
        {
            return new List<InterviewerAssignInfo>
            {
                new InterviewerAssignInfo
                {
                    Login = "int1",
                    Name = "Konstantin Konstantinopolskiy",
                    AssingmentsCount = 120
                },
                new InterviewerAssignInfo
                {
                    Login = "int2",
                    Name = "Vasya",
                    AssingmentsCount = 0
                },
                new InterviewerAssignInfo
                {
                    Login = "interviewer12345",
                    AssingmentsCount = 5
                }
            };
        }
    }

    public interface IInterviewersListAccessor
    {
        List<InterviewerAssignInfo> GetInterviewers();
    }

    public class InterviewerAssignInfo
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public int AssingmentsCount { get; set; } = 0;
    }
}
