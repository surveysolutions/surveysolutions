namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IPatchGenerator
    {
        string Diff(string left, string right);
    }
}
