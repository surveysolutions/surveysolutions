using System;
using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class StatusItem
    {
        public Guid PublicKey { get; set; }
        public string Title { get; set; }
        public bool IsVisible {get; set;}

        /// <summary>
        /// Is used for defining of the status for initially created CQ.
        /// </summary>
        public bool IsInitial { get; set;}

        /// <summary>
        /// Holds restriction by role.
        /// </summary>
        public Dictionary<string, List<SurveyStatus>> StatusRoles { set;get;}
        
        /// <summary>
        /// Flag displays status is used for stuck item in the commonon flow.
        /// </summary>
        public bool IsDefaultStuck { get; set; }

        /// <summary>
        /// List of flow rules is used for status changing.
        /// </summary>
        public Dictionary<Guid,FlowRule> FlowRules { get; set;}

        public StatusItem()
        {
            this.PublicKey = Guid.NewGuid();
            IsVisible = true;
            StatusRoles = new Dictionary<string, List<SurveyStatus>>();
            FlowRules = new Dictionary<Guid, FlowRule>();
        }
    }
}
