using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Status.Processing
{
    public class StatusProcessViewInputModel
    {
        public StatusProcessViewInputModel(string id, Guid publicId)
        {
            StatusId = IdUtil.CreateStatusId(id);
            PublicId = publicId;
        }

        public string StatusId { get; private set; }

        public Guid PublicId { get; private set; }
    }
}
