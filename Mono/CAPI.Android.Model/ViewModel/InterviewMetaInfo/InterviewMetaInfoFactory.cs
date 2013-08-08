using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace CAPI.Android.Core.Model.ViewModel.InterviewMetaInfo
{
    public class InterviewMetaInfoFactory : IViewFactory<InterviewMetaInfoInputModel, WB.Core.SharedKernel.Structures.Synchronization.InterviewMetaInfo>
    {
        private readonly IFilterableReadSideRepositoryReader<QuestionnaireDTO> questionnaireDtoDocumentStorage;

        public InterviewMetaInfoFactory(IFilterableReadSideRepositoryReader<QuestionnaireDTO> questionnaireDtoDocumentStorage)
        {
            this.questionnaireDtoDocumentStorage = questionnaireDtoDocumentStorage;
        }

        public WB.Core.SharedKernel.Structures.Synchronization.InterviewMetaInfo Load(InterviewMetaInfoInputModel input)
        {
            var interview = questionnaireDtoDocumentStorage.GetById(input.InterviewId);
            if (interview == null)
                return null;
            var status = new SurveyStatusMeta() {Id = Guid.Parse(interview.Status)};
            return new WB.Core.SharedKernel.Structures.Synchronization.InterviewMetaInfo()
                {
                    PublicKey = input.InterviewId,
                    ResponsibleId =
                        Guid.Parse(interview.Responsible),
                    Status = status,
                    TemplateId = Guid.Parse(interview.Survey)
                };
        }
    }
}