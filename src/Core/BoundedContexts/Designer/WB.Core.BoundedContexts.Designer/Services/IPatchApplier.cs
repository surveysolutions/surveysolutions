namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IPatchApplier
    {
        string Apply(string left, string patch);
    }
}
