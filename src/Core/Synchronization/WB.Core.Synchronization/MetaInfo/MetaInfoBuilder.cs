using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.Synchronization.MetaInfo
{
    public class MetaInfoBuilder : IMetaInfoBuilder
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnarieStorage;

        public MetaInfoBuilder(IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnarieStorage)
        {
            this.questionnarieStorage = questionnarieStorage;
        }

        public InterviewMetaInfo GetInterviewMetaInfo(InterviewSynchronizationDto doc)
        {
            if (doc == null)
                return null;

            var storedQuestionnaire = this.questionnarieStorage.AsVersioned().Get(doc.QuestionnaireId.FormatGuid(), doc.QuestionnaireVersion);
            if (storedQuestionnaire == null)
                return null;

            return new InterviewMetaInfo
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
                FeaturedQuestionsMeta = GetFeaturedQuestionsMeta(doc, storedQuestionnaire.Questionnaire)
            };
        }

        private static List<FeaturedQuestionMeta> GetFeaturedQuestionsMeta(InterviewSynchronizationDto doc, QuestionnaireDocument questionnarie)
        {
            var featuredQuestionList = new List<FeaturedQuestionMeta>();

            foreach (var featuredQuestion in questionnarie.Find<IQuestion>(q => q.Featured))
            {
                var answerOnFeaturedQuestion = doc.Answers.FirstOrDefault(q => q.Id == featuredQuestion.PublicKey);

                if (answerOnFeaturedQuestion != null && answerOnFeaturedQuestion.Answer != null)
                {
                    featuredQuestionList.Add(new FeaturedQuestionMeta(featuredQuestion.PublicKey, featuredQuestion.QuestionText,
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
