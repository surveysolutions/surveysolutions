using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerInputModel
    {
        public InterviewerInputModel(string id)
        {
            UserId = IdUtil.CreateUserId(id);
        }

        public string UserId { get; private set; }
    }
}
