namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface ISubstitutionService
    {
        string[] GetAllSubstitutionVariableNames(string source, string selfVariable);
        string ReplaceSubstitutionVariable(string text, string selfVariableName, string variable, string replaceTo);
        string RosterTitleSubstitutionReference { get; }
        string DefaultSubstitutionText { get; }
        bool ContainsRosterTitle(string input);
    }
}
