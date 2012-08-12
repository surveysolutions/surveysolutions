using System;
using System.Linq;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyViewFactory : IViewFactory<SurveyViewInputModel, SurveyBrowseView>
    {
        private readonly IDenormalizerStorage<SurveyBrowseItem> documentItemSession;

        public SurveyViewFactory(IDenormalizerStorage<SurveyBrowseItem> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }

        public SurveyBrowseView Load(SurveyViewInputModel input)
        {
            var model = new SurveyBrowseView();
            var statuses = SurveyStatus.GetAllStatuses().Select(s => s.Name).ToList();
            statuses.Insert(0, "UnAssignment");
            model.Headers = statuses;
            var alltemplate = documentItemSession.Query().Select(x => x.TemplateId).Distinct();
            foreach (var template in alltemplate)
            {
                var title = documentItemSession.Query().Where(t => t.TemplateId == template).FirstOrDefault().Title;
                var item = new SurveyBrowseItem(Guid.NewGuid(), title, template, null, null);
                foreach (var statusename in statuses)
                {
                    int count = 0;
                    if (statusename == "UnAssignment")
                        count = documentItemSession.Query().Where(t => t.TemplateId == template).Sum(x => x.UnAssignment);
                    else
                        count += Enumerable.Count(documentItemSession.Query().Where(t => t.TemplateId == template), bitem => bitem.Status.Name == statusename && bitem.UnAssignment == 0);
                    item.Statistics.Add(statusename, count);
                }
                model.Items.Add(item);
            }
            return model;
        }
    }
}
