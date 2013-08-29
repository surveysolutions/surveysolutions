using System.Linq;
using Main.Core.Documents;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.MetaInfo
{
    public class MetaInfoBuilder : IMetaInfoBuilder
    {
        public InterviewMetaInfo GetInterviewMetaInfo(CompleteQuestionnaireDocument doc)
        {
            if (doc == null)
                return null;

            var metaInfo = new InterviewMetaInfo();
            if (doc.Responsible != null)
                metaInfo.ResponsibleId = doc.Responsible.Id;

            metaInfo.PublicKey = doc.PublicKey;
            metaInfo.TemplateId = doc.TemplateId;
            metaInfo.Title = doc.Title;
            metaInfo.Status = new SurveyStatusMeta()
                {
                    Id = doc.Status.PublicId,
                    Name = doc.Status.Name
                };

            metaInfo.FeaturedQuestionsMeta = doc.GetFeaturedQuestions()
                .Select(q => new FeaturedQuestionMeta(q.PublicKey, q.QuestionText, q.GetAnswerString())).ToList();

            return metaInfo;
        }
    }
}
