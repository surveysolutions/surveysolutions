using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Status.SubView;

namespace RavenQuestionnaire.Core.Views.Status.StatusElement
{
    /// <summary>
    /// 
    /// </summary>
    public class StatusItemView
    {
        public Guid PublicKey { get; set; }

        public string Title { get; set; }
        public bool IsVisible { get; set; }

        public string QuestionnaireId
        {
            get { return IdUtil.ParseId(_questionnaireId); }
            set { _questionnaireId = value; }
        }

        private string _questionnaireId;

        public string StatusId 
        {
            get { return IdUtil.ParseId(_statusId); }
            set { _statusId = value; }
        }

        private string _statusId;

        public bool IsInitial { get; set; }

        public Dictionary<Guid, FlowRule> FlowRules { get; set; }

        public Dictionary<string, List<SurveyStatus>> StatusRoles { private set; get; }

        private List<StatusByRole> _statusRolesMatrix;
        public List<StatusByRole> StatusRolesMatrix
        {
            set { _statusRolesMatrix = value; }
            get { return _statusRolesMatrix ?? (_statusRolesMatrix = new List<StatusByRole>()); }
        }


        public StatusItemView()
        {
            StatusRolesMatrix = new List<StatusByRole>();
        }

        public StatusItemView(StatusItem item, string statusId, String questionnaireId)
        {
            this.PublicKey = item.PublicKey;
            this.Title = item.Title;
            this.IsVisible = item.IsVisible;
            StatusRoles = item.StatusRoles;
            this.FlowRules = item.FlowRules;
            this.IsInitial = item.IsInitial;


            StatusId = statusId;
            QuestionnaireId = questionnaireId;
        }


        /*public StatusItemView(Guid key, 
            string title, bool isVisible, 
            Dictionary<string, List<SurveyStatus>> statusRoles, 
            string questionnaireId,
            Dictionary <Guid, FlowRule> flowRules):this()
        {
            this.PublicKey = key; 
            this.Title = title;
            this.IsVisible = isVisible;
            StatusRoles = statusRoles;
            QuestionnaireId = questionnaireId;
            this.FlowRules = flowRules;
        }*/
        public static StatusView New()
        {
            return new StatusView();
        }
    }
}
