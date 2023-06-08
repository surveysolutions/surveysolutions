using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.DroidX.RecyclerView.ItemTemplates;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class CoverInterviewTemplateSelector : IMvxTemplateSelector
    {
        public int GetItemViewType(object forItemObject)
        {
            if (forItemObject is CoverPrefilledEntity coverEntity)
            {
                if (coverEntity.IsGpsAnswered)
                    return Resource.Layout.interview_cover_prefilled_gps_question_template;
                if (coverEntity.Attachment != null)
                    return Resource.Layout.interview_cover_prefilled_entity_with_attachment_template;
            }
            
            return Resource.Layout.interview_cover_prefilled_question_template;
        }

        public int GetItemLayoutId(int fromViewType) => fromViewType;

        public int ItemTemplateId { get; set; }
    }
}
