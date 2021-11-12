using System;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    public class TranslationItem
    {
        public TranslationType Type { get; set; }

        public Guid? EntityId { get; set; }
        
        public string? EntityVariableName { get; set; }

        public string? TranslationIndex { get; set; }

        public string? Value { get; set; }
    }

    public enum TranslationType
    {
        Title = 1,
        ValidationMessage = 2,
        Instruction = 3,
        OptionTitle = 4,
        FixedRosterTitle = 5,
        SpecialValue = 6,
        Categories = 7
    }
}