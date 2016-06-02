namespace WB.Core.BoundedContexts.Headquarters.Views.Supervisor
{
    public interface ISupervisorsViewFactory
    {
        SupervisorsView Load(SupervisorsInputModel input);
    }
}