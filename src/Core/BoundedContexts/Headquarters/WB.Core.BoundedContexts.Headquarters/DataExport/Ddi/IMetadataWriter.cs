using ddidotnet;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Ddi
{
    internal interface IMetadataWriter
    {
        void SetMetadataTitle(string questionnaireTitle);

        DdiDataFile CreateDdiDataFile(string fileName);

        DdiVariable AddDdiVariableToFile(DdiDataFile ddiDataFile, string variableName, DdiDataType type,string label, string instruction, string literal, DdiVariableScale? ddiVariableScale);

        void AddValueLabelToVariable(DdiVariable variable, decimal valueName, string labelName);

        void SaveMetadataInFile(string fileName);
    }
}