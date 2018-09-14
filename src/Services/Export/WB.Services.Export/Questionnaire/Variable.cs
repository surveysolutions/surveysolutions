using System;

namespace WB.Services.Export.Questionnaire
{
    public class Variable
    {
        public Guid PublicKey { get; set; }
        public VariableType Type { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
    }

    public enum VariableType
    {
        LongInteger = 1,
        Double = 2,
        Boolean = 3,
        DateTime = 4,
        String = 5
    }
}
