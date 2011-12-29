using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireViewEnumerable
    {
        public string Id { get; set; }
        public string Title { get;  set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate{ get; set; }

        public SurveyStatus Status { get; set; }

        public UserLight Responsible { set; get; }
        public CompleteGroupView CurrentGroup { get; set; }


        public CompleteQuestionnaireViewEnumerable(CompleteQuestionnaireDocument doc, RavenQuestionnaire.Core.Entities.SubEntities.Group currentGroup)
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.Title = doc.Questionnaire.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            if(currentGroup!=null)
            {
                this.CurrentGroup = new CompleteGroupView(doc, currentGroup);
            }

        }
        public CompleteQuestionnaireViewEnumerable(QuestionnaireDocument template)
        {
            this.Title = template.Title;
            this.CurrentGroup = new CompleteGroupView(new CompleteQuestionnaireDocument() {Questionnaire = template},
                                                      new Entities.SubEntities.Group() {Questions = template.Questions});
            /*  this.CurrentQuestion = new CompleteQuestionView(template.Questions[0], template.Id);*/
        }
       
    }
}
