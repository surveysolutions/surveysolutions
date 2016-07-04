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

        [Obsolete ("v 5.10, release 01 jul 16")]
        public Dictionary<Guid, RosterVector[]> LinkedQuestionOptions { get; private set; }

        public Dictionary<Identity, RosterVector[]> LinkedQuestionOptionsSet { get; private set; } = new Dictionary<Identity, RosterVector[]>();

        public void Clear()
        {
            this.LinkedQuestionOptions.Clear();
            this.LinkedQuestionOptionsSet.Clear();
        }
    }
}