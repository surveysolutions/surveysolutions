using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class DashboardStorageViewModel: IReadSideRepositoryEntity
    {
        public string Id { get; set; }
        public List<QuestionnaireListItem> Questionnaires { get; set; }   
    }
}