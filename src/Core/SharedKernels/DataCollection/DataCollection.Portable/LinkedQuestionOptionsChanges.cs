using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public class LinkedQuestionOptionsChanges
    {
        internal LinkedQuestionOptionsChanges() : this(new Dictionary<Guid, RosterVector[]>()) { }
        public LinkedQuestionOptionsChanges(Dictionary<Guid, RosterVector[]> linkedQuestionOptions)
        {
            this.LinkedQuestionOptions = linkedQuestionOptions;
        }

        public Dictionary<Guid, RosterVector[]> LinkedQuestionOptions { get; private set; }

        public void Clear()
        {
            this.LinkedQuestionOptions.Clear();
        }
    }
}