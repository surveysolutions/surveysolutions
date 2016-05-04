using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public class ValidationDescription
    {
        public Func<bool> PreexecutionCheck { set; get; }

        public Dictionary<int, Func<bool>> Validations { set; get; }
    }
}
