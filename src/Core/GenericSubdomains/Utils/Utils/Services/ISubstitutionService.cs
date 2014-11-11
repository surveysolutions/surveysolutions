namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface ISubstitutionService
    {
        string[] GetAllSubstitutionVariableNames(string source);
        string ReplaceSubstitutionVariable(string text, string variable, string replaceTo);
        string RosterTitleSubstitutionReference { get; }
        string DefaultSubstitutionText { get; }
    }
}