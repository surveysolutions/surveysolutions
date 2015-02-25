using System;
using System.Linq;
using WB.Core.BoundedContexts.Capi.Views.InterviewMetaInfo;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.UI.Capi.ViewModel.Dashboard;

namespace WB.UI.Capi.ViewModel.InterviewMetaInfo
{
    public class InterviewMetaInfoFactory :
        IViewFactory<InterviewMetaInfoInputModel, WB.Core.SharedKernel.Structures.Synchronization.InterviewMetaInfo>
    {
        private readonly IFilterableReadSideRepositoryReader<QuestionnaireDTO> questionnaireDtoDocumentStorage;

        public InterviewMetaInfoFactory(
            IFilterableReadSideRepositoryReader<QuestionnaireDTO> questionnaireDtoDocumentStorage)
        {
            this.questionnaireDtoDocumentStorage = questionnaireDtoDocumentStorage;
        }

        public WB.Core.SharedKernel.Structures.Synchronization.InterviewMetaInfo Load(InterviewMetaInfoInputModel input)
        {
            var interview = this.questionnaireDtoDocumentStorage.GetById(input.InterviewId);
            if (interview == null)
                return null;

            var featuredQuestionList = interview.CreatedOnClient != null && interview.CreatedOnClient.Value
                ? interview.GetProperties()
                    .Select(
                        featuredQuestion =>
                            new FeaturedQuestionMeta(featuredQuestion.PublicKey, featuredQuestion.Title, featuredQuestion.Value)).ToList()
                : null;

            return new WB.Core.SharedKernel.Structures.Synchronization.InterviewMetaInfo()
            {
                PublicKey = input.InterviewId,
                ResponsibleId = Guid.Parse(interview.Responsible),
                Status = (int) interview.Status,
                TemplateId = Guid.Parse(interview.Survey),
                Comments = interview.Comments,
                Valid = interview.Valid,
                FeaturedQuestionsMeta = featuredQuestionList,
                CreatedOnClient = interview.JustInitilized,
                TemplateVersion = interview.SurveyVersion
            };
        }
    }
}