﻿using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;
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

            var storedQuestionnarie = this.questionnarieStorage.GetById(doc.QuestionnaireId, doc.QuestionnaireVersion);
            if (storedQuestionnarie == null)
                return null;
            var questionnarie = storedQuestionnarie.Questionnaire;
            var metaInfo = new InterviewMetaInfo();

            metaInfo.ResponsibleId = doc.UserId;

            metaInfo.PublicKey = doc.Id;
            metaInfo.TemplateId = doc.QuestionnaireId;
            metaInfo.Status = (int) doc.Status;
            metaInfo.CreatedOnClient = doc.CreatedOnClient;

            var featuredQuestionList = new List<FeaturedQuestionMeta>();

            foreach (var featuredQuestion in questionnarie.Find<IQuestion>(q => q.Featured))
            {
                var answerOnFeaturedQuestion = doc.Answers.FirstOrDefault(q => q.Id == featuredQuestion.PublicKey);

                if (answerOnFeaturedQuestion != null && answerOnFeaturedQuestion.Answer != null)
                {
                    featuredQuestionList.Add(new FeaturedQuestionMeta(featuredQuestion.PublicKey, featuredQuestion.QuestionText,
                        AnswerUtils.AnswerToString(answerOnFeaturedQuestion.Answer)));
                }
            }

            metaInfo.FeaturedQuestionsMeta = featuredQuestionList;

            return metaInfo;
        }
    }
}
