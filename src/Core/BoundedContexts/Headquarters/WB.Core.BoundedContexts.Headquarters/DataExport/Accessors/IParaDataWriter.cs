using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    internal interface IParaDataWriter: IReadSideRepositoryWriter<InterviewHistoryView>
    {
        void ClearParaData();
        string CreateParaData();
    }
}