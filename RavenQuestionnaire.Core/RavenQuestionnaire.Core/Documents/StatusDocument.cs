using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    /// <summary>
    /// Describes the status in the system
    /// </summary>
    public class StatusDocument
    {
        /// <summary>
        /// Document ID.
        /// </summary>
        public string Id { get; set; }
        public string Title { get; set; }
        public bool IsVisible {get; set;}

        /// <summary>
        /// Is used for defining of the status for initially created CQ.
        /// </summary>
        public bool IsInitial { get; set;}

        /// <summary>
        ///Containes ref to the correspondent Q.
        /// </summary>
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// Holds restriction by role.
        /// </summary>
        public Dictionary<string, List<SurveyStatus>> StatusRoles { set;get;}

        public StatusDocument()
        {
            IsVisible = true;
            StatusRoles = new Dictionary<string, List<SurveyStatus>>();
            FlowRules = new Dictionary<Guid, FlowRule>();
        }
        
        /// <summary>
        /// Flag displays status is used for stuck item in the commonon flow.
        /// </summary>
        public bool IsDefaultStuck { get; set; }

        /// <summary>
        /// List of flow rules is used for status changing.
        /// </summary>
        public Dictionary<Guid,FlowRule> FlowRules { get; set;}
        
    }
}
