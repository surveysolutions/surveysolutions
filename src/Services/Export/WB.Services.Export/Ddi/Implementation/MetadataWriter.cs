using ddidotnet;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Ddi.Implementation
{
    internal class MetadataWriter : IMetadataWriter
    {
        private readonly IMetaDescription metaDescription;

        public MetadataWriter(IMetaDescription metaDescription)
        {
            this.metaDescription = metaDescription;
        }

        public void SetMetadataTitle(string questionnaireTitle)
        {
            this.metaDescription.Study.Title = this.metaDescription.Document.Title = questionnaireTitle;
            this.metaDescription.Study.Idno = "QUEST";
        }
        public void SaveMetadataInFile(string fileName)
        {
            this.metaDescription.WriteXml(fileName);
        }

        public DdiDataFile CreateDdiDataFile(string fileName)
        {
            return this.metaDescription.AddDataFile(fileName);
        }

        public DdiVariable AddDdiVariableToFile(DdiDataFile ddiDataFile, string variableName, DdiDataType type, string label, string instruction, string literal, DdiVariableScale? ddiVariableScale)
        {
            var variable = ddiDataFile.AddVariable(type);
            variable.Name = variableName.RemoveHtmlTags();
            variable.Label = label.RemoveHtmlTags();

            if (!string.IsNullOrEmpty(instruction))
                variable.IvuInstr = instruction.RemoveHtmlTags();

            if (!string.IsNullOrEmpty(literal))
                variable.QstnLit = literal.RemoveHtmlTags();

            if(ddiVariableScale.HasValue)
                variable.VariableScale = ddiVariableScale.Value;

            return variable;
        }

        public void AddValueLabelToVariable(DdiVariable variable, decimal valueName, string labelName)
        {
            variable.AddValueLabel(valueName, labelName);
        }
    }
}
