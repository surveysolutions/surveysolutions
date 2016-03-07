using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public class LinkedQuestionOptionsChanges
    {
        internal LinkedQuestionOptionsChanges() : this(new List<LinkedQuestionOption>(), new List<LinkedQuestionOption>()) { }
        public LinkedQuestionOptionsChanges(List<LinkedQuestionOption> optionsDeclaredEnabled, List<LinkedQuestionOption> optionsDeclaredDisabled)
        {
            this.OptionsDeclaredEnabled = optionsDeclaredEnabled;
            this.OptionsDeclaredDisabled = optionsDeclaredDisabled;
        }

        public List<LinkedQuestionOption> OptionsDeclaredEnabled { get; private set; }
        public List<LinkedQuestionOption> OptionsDeclaredDisabled { get; private set; }

        public void Clear()
        {
            this.OptionsDeclaredEnabled.Clear();
            this.OptionsDeclaredDisabled.Clear();
        }
    }

    public class LinkedQuestionOption
    {
        public Guid LinkedQuestionId { get; set; }
        public RosterVector RosterVector { get; set; }
    }
}