using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class CloneQuestionnaireModel
    {
        public CloneQuestionnaireModel(Guid id, long version, string title)
        {
            this.Id = id;
            this.Version = version;
            this.Title = title;
        }

        public Guid Id { get; }
        public long Version { get; }
        public string Title { get; }
    }
}