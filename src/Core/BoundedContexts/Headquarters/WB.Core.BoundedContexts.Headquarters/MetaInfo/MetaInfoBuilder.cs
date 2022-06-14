using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.Synchronization.MetaInfo
{
    public class MetaInfoBuilder : IMetaInfoBuilder
    {
        private readonly IQuestionnaireStorage questionnaireStorage;

        public MetaInfoBuilder(IQuestionnaireStorage questionnaireStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
        }

        public InterviewMetaInfo GetInterviewMetaInfo(InterviewSynchronizationDto doc)
        {
            if (doc == null)
                return null;

            var storedQuestionnaire = this.questionnaireStorage.GetQuestionnaire(new QuestionnaireIdentity(doc.QuestionnaireId, doc.QuestionnaireVersion), null);
            if (storedQuestionnaire == null)
                return null;

            return new InterviewMetaInfo(GetFeaturedQuestionsMeta(doc, storedQuestionnaire))
            {
                ResponsibleId = doc.UserId,
                PublicKey = doc.Id,
                TemplateId = doc.QuestionnaireId,
                TemplateVersion = doc.QuestionnaireVersion,
                Status = (int) doc.Status,
                CreatedOnClient = doc.CreatedOnClient,
                Comments = doc.Comments,
                RejectDateTime = doc.RejectDateTime,
                InterviewerAssignedDateTime = doc.InterviewerAssignedDateTime,
            };
        }

        private static List<FeaturedQuestionMeta> GetFeaturedQuestionsMeta(InterviewSynchronizationDto doc, IQuestionnaire questionnarie)
        {
            var featuredQuestionList = new List<FeaturedQuestionMeta>();

            foreach (var featuredQuestionId in questionnarie.GetPrefilledQuestions())
            {
                var answerOnFeaturedQuestion = doc.Answers.FirstOrDefault(q => q.Id == featuredQuestionId);

                if (answerOnFeaturedQuestion != null && answerOnFeaturedQuestion.Answer != null)
                {
                    featuredQuestionList.Add(new FeaturedQuestionMeta(featuredQuestionId, 
                        questionnarie.GetQuestionTitle(featuredQuestionId),
                        GetAnswerOnPrefilledQuestion(answerOnFeaturedQuestion)));
                }
            }
            return featuredQuestionList;
        }

        private static string GetAnswerOnPrefilledQuestion(AnsweredQuestionSynchronizationDto answerOnFeaturedQuestion)
        {
            if (answerOnFeaturedQuestion.Answer is DateTime)
            {
                return ((DateTime)answerOnFeaturedQuestion.Answer).ToString("u");
            }

            return AnswerUtils.AnswerToString(answerOnFeaturedQuestion.Answer);
        }
    }
}
