using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:ImageUploaded")]
    public class ImageUploaded
    {
        public Guid QuestionKey { get; set; }

        public Guid ImageKey { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
    }
}
