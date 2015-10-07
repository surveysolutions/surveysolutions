using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Supervisor
{
    public class SupervisorsInputModel : ListViewModelBase
    {
        public string SearchBy { get; set; }
        public bool Archived { get; set; }
    }
}