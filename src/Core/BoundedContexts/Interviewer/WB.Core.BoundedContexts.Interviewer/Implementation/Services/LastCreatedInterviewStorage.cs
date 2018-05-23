using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class LastCreatedInterviewStorage : ILastCreatedInterviewStorage
    {
        private static string lastId;

        public void Store(string interviewId)
        {
            lastId = interviewId;
        }

        public bool WasJustCreated(string interviewId)
        {
            if (lastId == interviewId)
            {
                lastId = null;
                return true;
            }

            return false;
        }
    }
}