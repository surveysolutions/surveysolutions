namespace WB.Core.SharedKernels.SurveyManagement.Views.Supervisor
{
    public interface ISupervisorsViewFactory
    {
        SupervisorsView Load(SupervisorsInputModel input);
    }
}