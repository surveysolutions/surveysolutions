using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public class ValidationDescription
    {
        public Func<bool> PreexecutionCheck { set; get; }

        public Dictionary<int, Func<bool>> Validations { set; get; }

        public bool IsFromQuestion { get; set; }
        public bool IsFromStaticText { get; set; }

        public static ValidationDescription ForQuestion(Func<bool> preexecutionCheck, Dictionary<int, Func<bool>> validations) => new ValidationDescription
        {
            IsFromQuestion = true,
            PreexecutionCheck = preexecutionCheck,
            Validations = validations,
        };

        public static ValidationDescription ForStaticText(Dictionary<int, Func<bool>> validations) => new ValidationDescription
        {
            IsFromStaticText = true,
            PreexecutionCheck = () => false,
            Validations = validations,
        };
    }
}
