using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.Synchronization.MetaInfo
{
    public class MetaInfoBuilder : IMetaInfoBuilder
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnarieStorage;

        public MetaInfoBuilder(IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnarieStorage)
        {
            this.questionnarieStorage = questionnarieStorage;
        }

        public InterviewMetaInfo GetInterviewMetaInfo(InterviewSynchronizationDto doc)
        {
            if (doc == null)
                return null;

            var storedQuestionnarie = questionnarieStorage.GetById(doc.QuestionnaireId, doc.QuestionnaireVersion);
            if (storedQuestionnarie == null)
                return null;
            var questionnarie = storedQuestionnarie.Questionnaire;
            var metaInfo = new InterviewMetaInfo();

            metaInfo.ResponsibleId = doc.UserId;

            metaInfo.PublicKey = doc.Id;
            metaInfo.TemplateId = doc.QuestionnaireId;
            metaInfo.Status = (int) doc.Status;

            var featuredQuestionList = new List<FeaturedQuestionMeta>();

            foreach (var featuredQuestion in questionnarie.Find<IQuestion>(q => q.Featured))
            {
                var answerOnFeaturedQuestion = doc.Answers.FirstOrDefault(q => q.Id == featuredQuestion.PublicKey);
                if(answerOnFeaturedQuestion==null)
                    continue;
                featuredQuestionList.Add(new FeaturedQuestionMeta(featuredQuestion.PublicKey,featuredQuestion.QuestionText,answerOnFeaturedQuestion.Answer.ToString()));
            }

            metaInfo.FeaturedQuestionsMeta = featuredQuestionList;

            return metaInfo;
        }
    }
}
