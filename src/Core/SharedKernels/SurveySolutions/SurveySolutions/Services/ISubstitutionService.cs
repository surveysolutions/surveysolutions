namespace WB.Core.SharedKernels.SurveySolutions.Services
{
    public interface ISubstitutionService
    {
        string[] GetAllSubstitutionVariableNames(string source);
        string ReplaceSubstitutionVariable(string text, string variable, string replaceTo);
        string RosterTitleSubstitutionReference { get; }
        string DefaultSubstitutionText { get; }
        bool ContainsRosterTitle(string input);
    }
}