using System;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class SurveyStatus
    {
        public SurveyStatus()
        {
        }

        public SurveyStatus(Guid id, string name)
        {
            PublicId = id;
            Name = name;
        }


        public Guid PublicId { get; set; }
        public string Name { get; set; }
        public string ChangeComment { get; set; }

    }
}
