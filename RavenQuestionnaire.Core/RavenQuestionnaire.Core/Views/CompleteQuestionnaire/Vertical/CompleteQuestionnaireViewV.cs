using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical
{
    public class CompleteQuestionnaireViewV
    {
        public CompleteQuestionnaireViewV(CompleteQuestionnaireDocument doc, ICompleteGroup currentGroup)
        {
            Id = IdUtil.ParseId(doc.Id);
            Title = doc.Title;
            CreationDate = doc.CreationDate;
            LastEntryDate = doc.LastEntryDate;
            Status = doc.Status;
            Responsible = doc.Responsible;
            
            var cg = currentGroup as ICompleteGroup<ICompleteGroup, ICompleteQuestion>;
            CurrentGroup = new CompleteGroupViewV(doc, currentGroup as CompleteGroup);

            InitGroups(doc, CurrentGroup.PublicKey);

        }

        public CompleteQuestionnaireViewV(CompleteQuestionnaireDocument doc)
        {
            Title = doc.Title;
            Title = doc.Title;
            CreationDate = doc.CreationDate;
            LastEntryDate = doc.LastEntryDate;
            Status = doc.Status;
            Responsible = doc.Responsible;

            var group = new CompleteGroup { Questions = doc.Questions };
            CurrentGroup = new CompleteGroupViewV(doc, group);
            InitGroups(doc, CurrentGroup.PublicKey);
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public SurveyStatus Status { get; set; }

        public UserLight Responsible { set; get; }
        public CompleteGroupViewV CurrentGroup { get; set; }
        public CompleteGroupHeaders[] Groups { get; set; }

        protected void InitGroups(CompleteQuestionnaireDocument doc, Guid currentGroupPublicKey)
        {
            if (doc.Questions.Count > 0)
            {
                Groups = new CompleteGroupHeaders[doc.Groups.Count + 1];

                Groups[0] = new CompleteGroupHeaders
                                {
                                    PublicKey = Guid.Empty,
                                    GroupText = "Main",
                                    
                                };
                for (var i = 1; i <= doc.Groups.Count; i++)
                {
                    Groups[i] = new CompleteGroupHeaders
                                    {
                                        PublicKey = doc.Groups[i - 1].PublicKey,
                                        GroupText = doc.Groups[i - 1].Title
                                    };
                }
            }
            else
            {
                Groups = new CompleteGroupHeaders[doc.Groups.Count];
                for (var i = 0; i < doc.Groups.Count; i++)
                {
                    Groups[i] = new CompleteGroupHeaders
                                    {
                                        PublicKey = doc.Groups[i].PublicKey,
                                        GroupText = doc.Groups[i].Title
                                    };
                }
            }
            var current = Groups.FirstOrDefault(g => g.PublicKey == currentGroupPublicKey);
            current.IsCurrent = true;
        }
    }
}