using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Views.Statistics;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyGroupViewFactory : IViewFactory<SurveyGroupInputModel, SurveyBrowseView>
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        public SurveyGroupViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }

        public SurveyBrowseView Load(SurveyGroupInputModel input)
        {
            var questionnaires = documentItemSession.Query().Where(x => x.TemplateId == input.Id).ToList<CompleteQuestionnaireBrowseItem>();
            var model = new SurveyBrowseView();
            foreach (var name in questionnaires.SelectMany(item => item.FeaturedQuestions.Select(t => t.QuestionText).Where(name => !model.Headers.Contains(name))))
                model.Headers.Add(name);
            if (!string.IsNullOrEmpty(input.QuestionnaireId))
            {
                var inputQuestionnaire = questionnaires.Where(t => t.CompleteQuestionnaireId == input.QuestionnaireId).FirstOrDefault();
                questionnaires = new List<CompleteQuestionnaireBrowseItem>() { inputQuestionnaire };                
            }
            foreach (var item in questionnaires)
            {
                var surveyItem = new SurveyBrowseItem(Guid.Parse(item.CompleteQuestionnaireId),
                                                     item.CompleteQuestionnaireId, item.TemplateId, item.Status,
                                                     item.Responsible);
                foreach (var nameField in model.Headers)
                {
                    var val = item.FeaturedQuestions.Where(t => t.QuestionText == nameField) as QuestionStatisticView;
                    surveyItem.FeatureadValue.Add(nameField, (val!=null ? val.AnswerValue : string.Empty));
                }
                model.Items.Add(surveyItem);
            }
            return model;
        }
    }
}
