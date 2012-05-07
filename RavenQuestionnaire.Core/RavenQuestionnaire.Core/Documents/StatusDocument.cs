using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    /// <summary>
    /// Describes the status in the system
    /// </summary>
    public class StatusDocument : AbstractDocument
    {
        /// <summary>
        ///Containes ref to the correspondent Q.
        /// </summary>
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// List of Statuses of Q.
        /// </summary>
        public List<StatusItem> Statuses { get; set; }

        
        public StatusDocument()
        {
            Statuses = new List<StatusItem>();
        }

    }
}
