using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels
{
    internal class LabeledVariable
    {
        private readonly Dictionary<string, VariableValueLabel> variableValueLabels;

        public LabeledVariable(string variableName, string label, Guid? questionId, VariableValueLabel[] variableValueLabels)
        {
            this.VariableName = variableName;
            this.Label = label;
            this.QuestionId = questionId;
            this.variableValueLabels = variableValueLabels.ToDictionary(x => x.Value, x => x);
        }

        public LabeledVariable(string variableName, string label):this(variableName,label,null, new VariableValueLabel[0])
        {
        }

        public string VariableName { get; private set; }
        public string Label { get; private set; }
        public Guid? QuestionId { get; private set; }
        public VariableValueLabel[] VariableValueLabels => this.variableValueLabels.Values.ToArray();
    }
}