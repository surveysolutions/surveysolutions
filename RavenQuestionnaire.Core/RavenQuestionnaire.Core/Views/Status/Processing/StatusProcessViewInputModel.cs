using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Status.Processing
{
    public class StatusProcessViewInputModel
    {
        public StatusProcessViewInputModel(string id)
        {
            StatusId = IdUtil.CreateStatusId(id);
        }

        public string StatusId { get; private set; }
    }
}
