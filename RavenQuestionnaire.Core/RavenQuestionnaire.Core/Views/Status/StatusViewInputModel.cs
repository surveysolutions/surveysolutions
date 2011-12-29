using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Status
{
    public class StatusViewInputModel
    {
        public StatusViewInputModel(string id)
        {
            StatusId = IdUtil.CreateStatusId(id);
        }

        public StatusViewInputModel(bool getDefault)
        {
            GetDefault = getDefault;
        }

        public string StatusId { get; private set; }
        public bool GetDefault { get; set; }
    }
}
